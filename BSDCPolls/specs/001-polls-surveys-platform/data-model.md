# Data Model: BSDCPolls — Real-Time Polls & Surveys Platform

**Date**: 2026-06-10
**Phase**: 1 — EF Core entities, relationships, validation rules, and state transitions

All entities inherit from `AuditableEntity` (Principle XIII). All entities use integer PKs
internally and expose GUIDs publicly. Private setters and static factory methods are mandatory.

---

## Base: AuditableEntity

Every entity in `BSDCPolls.Api.Data` extends this base class.

```
AuditableEntity
├── int Id                    — auto-increment integer PK (internal/FK use only)
├── Guid Uid                  — public identifier; set once via Guid.NewGuid() at creation
├── bool IsActive             — soft-delete flag; default true; hard deletes PROHIBITED
├── DateTime CreatedOn        — UTC; set by SaveChangesInterceptor on INSERT
├── int CreatedById           — FK → ApplicationUser.Id; set once on creation
├── ApplicationUser CreatedBy — navigation property (virtual)
├── DateTime UpdatedOn        — UTC; set by SaveChangesInterceptor on every UPDATE
├── int UpdatedById           — FK → ApplicationUser.Id; updated on every change
└── ApplicationUser UpdatedBy — navigation property (virtual)
```

---

## Entity: ApplicationUser

Represents a registered user. No PII is stored — only the system-generated username and
Supabase auth reference.

**Table**: `application_users`

| Property | Type | Constraints | Notes |
|----------|------|-------------|-------|
| Id | int | PK, auto-increment | Internal only |
| Uid | Guid | UNIQUE, NOT NULL | Exposed in all external APIs |
| IsActive | bool | NOT NULL, DEFAULT true | |
| Username | string | UNIQUE, NOT NULL, MaxLength(60) | 3-word hyphen-separated; profanity-free |
| SupabaseUserId | string | UNIQUE, NOT NULL, MaxLength(200) | GoTrue subject claim (`sub`); synthetic email key |
| CreatedOn | DateTime | NOT NULL | UTC |
| CreatedById | int | FK → ApplicationUser.Id | Self-referential; system sentinel (Id=1) for first user |
| UpdatedOn | DateTime | NOT NULL | UTC |
| UpdatedById | int | FK → ApplicationUser.Id | |

**State transitions**: Active users can be deactivated (IsActive = false) when no recovery
option is exercised and the account is considered abandoned. No hard delete.

**Relationships**:
- One-to-one: `UserPrivacySettings`
- One-to-many: `Poll` (as creator), `Survey` (as creator)
- One-to-many: `PollSubmission`, `SurveyResponse` (as respondent)
- One-to-many: `Invitation` (as inviter), `Invitation` (as invitee)
- One-to-many: `InviteAllowlistEntry` (as owner), `InviteAllowlistEntry` (as allowed user)
- One-to-many: `Notification` (as recipient)

**Seed**: Migration seeds the system sentinel user:
- `Id = 1`, `Uid = Guid("00000000-0000-0000-0000-000000000001")`, `Username = "system"`,
  `IsActive = false`, `SupabaseUserId = "SYSTEM"`, `CreatedById = 1`, `UpdatedById = 1`

---

## Entity: UsernameHistory

Tracks previously used usernames to prevent reuse within 90 days.

**Table**: `username_history`

| Property | Type | Constraints | Notes |
|----------|------|-------------|-------|
| Id | int | PK | |
| Uid | Guid | UNIQUE, NOT NULL | |
| IsActive | bool | NOT NULL | Always true for history records |
| UserId | int | FK → ApplicationUser.Id | |
| Username | string | NOT NULL, MaxLength(60) | The former username |
| RetiredAt | DateTime | NOT NULL | UTC when username was replaced |
| CreatedById / UpdatedById | int | FK → ApplicationUser.Id | Audit fields |

---

## Entity: UserPrivacySettings

Per-user privacy preferences. Created automatically when user registers (via `ApplicationUser.Create`).

**Table**: `user_privacy_settings`

| Property | Type | Constraints | Notes |
|----------|------|-------------|-------|
| Id | int | PK | |
| Uid | Guid | UNIQUE, NOT NULL | |
| IsActive | bool | NOT NULL | |
| UserId | int | UNIQUE FK → ApplicationUser.Id | One-to-one |
| ShowPublicContent | bool | NOT NULL, DEFAULT true | Whether user sees public polls/surveys in feed |
| InvitePermission | InvitePermission (enum) | NOT NULL, DEFAULT Everyone | Everyone / Nobody / AllowlistOnly |

**InvitePermission enum values**: `Everyone = 0`, `Nobody = 1`, `AllowlistOnly = 2`

---

## Entity: InviteAllowlistEntry

Tracks which specific users are allowed to invite the owning user when `InvitePermission = AllowlistOnly`.

**Table**: `invite_allowlist_entries`

| Property | Type | Constraints | Notes |
|----------|------|-------------|-------|
| Id | int | PK | |
| Uid | Guid | UNIQUE, NOT NULL | |
| IsActive | bool | NOT NULL | Soft-delete removes entry |
| OwnerId | int | FK → ApplicationUser.Id | The user who maintains this allowlist |
| AllowedUserId | int | FK → ApplicationUser.Id | The user permitted to invite the owner |

**Unique constraint**: `(OwnerId, AllowedUserId)` — no duplicate entries per owner/allowed pair.

**Username change invalidation**: When `ApplicationUser.Username` changes, entries where
`AllowedUserId` matches that user remain valid (they reference the user by Id, not by username).
The UI, however, showed usernames at the time of entry; the allowlist management screen
re-resolves usernames from Ids for display. This is correct behavior — the allowlist reference
survives username changes.

---

## Entity: Poll

A real-time live polling session.

**Table**: `polls`

| Property | Type | Constraints | Notes |
|----------|------|-------------|-------|
| Id | int | PK | |
| Uid | Guid | UNIQUE, NOT NULL | Used in public URL: `/polls/{uid}` |
| IsActive | bool | NOT NULL | |
| Title | string | NOT NULL, MaxLength(200) | |
| IsPublic | bool | NOT NULL, DEFAULT false | Public polls appear in all users' feeds |
| Status | PollStatus (enum) | NOT NULL, DEFAULT Draft | Draft / Active / Closed |
| CreatedById | int | FK → ApplicationUser.Id | The creator/owner |

**PollStatus enum values**: `Draft = 0`, `Active = 1`, `Closed = 2`

**State transitions**:
- `Draft → Active`: Creator explicitly activates the poll (starts the live session)
- `Active → Closed`: Creator closes the session; no further questions or votes accepted
- `Closed → Active`: Not permitted (polls cannot be reopened)

**Relationships**:
- One-to-many: `PollQuestion`
- One-to-many: `Invitation` (for invite-only polls)

---

## Entity: PollQuestion

A single multiple-choice question within a poll session. Questions are pushed by the creator
one at a time during an active session.

**Table**: `poll_questions`

| Property | Type | Constraints | Notes |
|----------|------|-------------|-------|
| Id | int | PK | |
| Uid | Guid | UNIQUE, NOT NULL | |
| IsActive | bool | NOT NULL | |
| PollId | int | FK → Poll.Id, CASCADE | |
| Poll | Poll | virtual navigation | |
| Text | string | NOT NULL, MaxLength(500) | The question text |
| OrderIndex | int | NOT NULL | Sequence in which questions were pushed |
| PushedAt | DateTime? | NULL until creator pushes | UTC timestamp of when question was sent to participants |

**State**: A question is "pushed" when `PushedAt` is set. Before that it is "staged" (created
but not yet visible to participants). Only the most-recently-pushed question is answerable.

**Relationships**:
- One-to-many: `PollAnswerOption`
- One-to-many: `PollSubmission`

---

## Entity: PollAnswerOption

One choice within a `PollQuestion`.

**Table**: `poll_answer_options`

| Property | Type | Constraints | Notes |
|----------|------|-------------|-------|
| Id | int | PK | |
| Uid | Guid | UNIQUE, NOT NULL | |
| IsActive | bool | NOT NULL | |
| PollQuestionId | int | FK → PollQuestion.Id, CASCADE | |
| PollQuestion | PollQuestion | virtual navigation | |
| Text | string | NOT NULL, MaxLength(200) | |
| OrderIndex | int | NOT NULL | Display order |

---

## Entity: PollSubmission

One participant's answer to one poll question. Unique per (participant, question).

**Table**: `poll_submissions`

| Property | Type | Constraints | Notes |
|----------|------|-------------|-------|
| Id | int | PK | |
| Uid | Guid | UNIQUE, NOT NULL | |
| IsActive | bool | NOT NULL | |
| PollQuestionId | int | FK → PollQuestion.Id, CASCADE | |
| PollQuestion | PollQuestion | virtual navigation | |
| SelectedOptionId | int | FK → PollAnswerOption.Id, RESTRICT | |
| SelectedOption | PollAnswerOption | virtual navigation | |
| RespondentId | int | FK → ApplicationUser.Id, CASCADE | |
| Respondent | ApplicationUser | virtual navigation | |

**Unique constraint**: `(PollQuestionId, RespondentId)` — prevents duplicate submissions.

---

## Entity: Survey

A structured questionnaire with configurable visibility and conditional branching.

**Table**: `surveys`

| Property | Type | Constraints | Notes |
|----------|------|-------------|-------|
| Id | int | PK | |
| Uid | Guid | UNIQUE, NOT NULL | Used in public URL: `/surveys/{uid}` |
| IsActive | bool | NOT NULL | |
| Title | string | NOT NULL, MaxLength(200) | |
| IsPublic | bool | NOT NULL, DEFAULT false | |
| Status | SurveyStatus (enum) | NOT NULL, DEFAULT Draft | Draft / Published / Closed |
| QuestionTree | SurveyQuestionTreeDocument | JSONB, NOT NULL | Full conditional question tree (see R-004) |
| CreatedById | int | FK → ApplicationUser.Id | Creator |

**SurveyStatus enum values**: `Draft = 0`, `Published = 1`, `Closed = 2`

**State transitions**:
- `Draft → Published`: Creator publishes; respondents can now access and complete the survey
- `Published → Closed`: Creator closes; no new responses accepted
- `Closed → Published`: Not permitted

**SurveyQuestionTreeDocument JSONB shape** (see research.md R-004):
```json
{
  "questions": [
    {
      "uid": "...",
      "text": "How satisfied are you?",
      "answerType": "MultipleChoice",
      "isRequired": true,
      "choices": [
        { "uid": "...", "text": "Very satisfied" },
        { "uid": "...", "text": "Dissatisfied" }
      ],
      "branches": [
        {
          "parentChoiceUid": "<uid of 'Dissatisfied' choice>",
          "questions": [
            {
              "uid": "...",
              "text": "What could be improved?",
              "answerType": "LongText",
              "isRequired": false,
              "choices": null,
              "branches": null
            }
          ]
        }
      ]
    }
  ]
}
```

---

## Entity: SurveyResponse

One respondent's complete (or in-progress) answer to a survey.

**Table**: `survey_responses`

| Property | Type | Constraints | Notes |
|----------|------|-------------|-------|
| Id | int | PK | |
| Uid | Guid | UNIQUE, NOT NULL | |
| IsActive | bool | NOT NULL | |
| SurveyId | int | FK → Survey.Id, CASCADE | |
| Survey | Survey | virtual navigation | |
| RespondentId | int | FK → ApplicationUser.Id | |
| Respondent | ApplicationUser | virtual navigation | |
| AnswersJson | SurveyAnswersDocument | JSONB, NOT NULL | All answered question UIDs with their values |
| IsComplete | bool | NOT NULL, DEFAULT false | True only after explicit submission |
| SubmittedAt | DateTime? | NULL until submitted | UTC |

**Unique constraint**: `(SurveyId, RespondentId)` — one response per user per survey.

**SurveyAnswersDocument JSONB shape**:
```json
{
  "answers": [
    { "questionUid": "...", "answerType": "MultipleChoice", "selectedChoiceUid": "..." },
    { "questionUid": "...", "answerType": "LongText", "textValue": "The UI is confusing." },
    { "questionUid": "...", "answerType": "DocumentUpload", "documentUid": "..." }
  ]
}
```

---

## Entity: SurveyDocument

A PDF file uploaded by a respondent as the answer to a document-upload question.

**Table**: `survey_documents`

| Property | Type | Constraints | Notes |
|----------|------|-------------|-------|
| Id | int | PK | |
| Uid | Guid | UNIQUE, NOT NULL | Referenced by `SurveyAnswersDocument.documentUid` |
| IsActive | bool | NOT NULL | |
| SurveyResponseId | int | FK → SurveyResponse.Id, CASCADE | |
| SurveyResponse | SurveyResponse | virtual navigation | |
| QuestionUid | Guid | NOT NULL | Which question within the survey this answers |
| FileName | string | NOT NULL, MaxLength(255) | Original filename from the browser |
| FileSizeBytes | long | NOT NULL | Validated ≤ 10,485,760 (10 MB) at BFF |
| FileData | byte[] | NOT NULL | Raw PDF bytes as PostgreSQL bytea |

---

## Entity: Invitation

A directed invitation from a creator to a specific user for a poll or survey.

**Table**: `invitations`

| Property | Type | Constraints | Notes |
|----------|------|-------------|-------|
| Id | int | PK | |
| Uid | Guid | UNIQUE, NOT NULL | |
| IsActive | bool | NOT NULL | |
| InviterId | int | FK → ApplicationUser.Id | Creator sending the invitation |
| Inviter | ApplicationUser | virtual navigation | |
| InviteeId | int | FK → ApplicationUser.Id | Target user receiving the invitation |
| Invitee | ApplicationUser | virtual navigation | |
| PollId | int? | FK → Poll.Id, nullable | Exactly one of PollId or SurveyId must be non-null |
| Poll | Poll? | virtual navigation | |
| SurveyId | int? | FK → Survey.Id, nullable | |
| Survey | Survey? | virtual navigation | |
| Status | InvitationStatus (enum) | NOT NULL, DEFAULT Pending | Pending / Viewed / Declined |

**InvitationStatus enum values**: `Pending = 0`, `Viewed = 1`, `Declined = 2`

**Cross-field constraint** (FluentValidation in Contracts):
- Exactly one of `PollId` or `SurveyId` must be provided; both null or both non-null is invalid.
- Angular custom validator mirrors: `// Mirrors: BSDCPolls.Contracts.CreateInvitationRequestValidator.PollOrSurveyRequired`

---

## Entity: Notification

An in-app alert linked to an invitation. Created when an invitation is created.

**Table**: `notifications`

| Property | Type | Constraints | Notes |
|----------|------|-------------|-------|
| Id | int | PK | |
| Uid | Guid | UNIQUE, NOT NULL | |
| IsActive | bool | NOT NULL | |
| RecipientId | int | FK → ApplicationUser.Id | |
| Recipient | ApplicationUser | virtual navigation | |
| InvitationId | int | FK → Invitation.Id, CASCADE | |
| Invitation | Invitation | virtual navigation | |
| IsRead | bool | NOT NULL, DEFAULT false | |
| ReadAt | DateTime? | NULL until read | UTC |

---

## Entity: AuditLog

Append-only change history for all entities. Populated by `SaveChangesInterceptor`.
This table is NEVER updated or deleted by application code.

**Table**: `audit_logs`

| Property | Type | Notes |
|----------|------|-------|
| Id | bigint | PK, auto-increment (bigint for high cardinality) |
| EntityType | string | e.g., "Poll", "PollSubmission" |
| EntityId | int | The entity's integer Id |
| EntityUid | Guid | The entity's Guid Uid |
| Action | string | "Create" / "Update" / "Delete" (soft-delete sets IsActive=false) |
| ChangedProperties | jsonb | Names of changed properties |
| OldValues | jsonb | Values before change |
| NewValues | jsonb | Values after change |
| ChangedOn | DateTime | UTC |
| ChangedById | int | FK → ApplicationUser.Id (nullable — system operations may have no user) |

> `AuditLog` does NOT extend `AuditableEntity` to avoid circular audit-of-audit writes.

---

## Relationship Summary

```
ApplicationUser 1──────────── 1 UserPrivacySettings
ApplicationUser 1──────────── * InviteAllowlistEntry (as owner)
ApplicationUser 1──────────── * InviteAllowlistEntry (as allowed user)

ApplicationUser 1──────────── * Poll (as creator)
Poll            1──────────── * PollQuestion
PollQuestion    1──────────── * PollAnswerOption
PollQuestion    1──────────── * PollSubmission
PollAnswerOption 1─────────── * PollSubmission
ApplicationUser 1──────────── * PollSubmission (as respondent)

ApplicationUser 1──────────── * Survey (as creator)
Survey          1──────────── * SurveyResponse
Survey.QuestionTree ──[JSONB]── SurveyQuestionNode tree
SurveyResponse  1──────────── * SurveyDocument
ApplicationUser 1──────────── * SurveyResponse (as respondent)
SurveyResponse.AnswersJson ─[JSONB]── SurveyAnswerEntry list

ApplicationUser 1──────────── * Invitation (as inviter)
ApplicationUser 1──────────── * Invitation (as invitee)
Poll            1──────────── * Invitation
Survey          1──────────── * Invitation
Invitation      1──────────── 1 Notification
ApplicationUser 1──────────── * Notification (as recipient)

ApplicationUser 1──────────── * UsernameHistory
```

---

## Indexes

| Table | Index | Type | Reason |
|-------|-------|------|--------|
| application_users | `username` | UNIQUE | Username lookup and uniqueness enforcement |
| application_users | `supabase_user_id` | UNIQUE | JWT sub → user lookup on every authenticated request |
| polls | `uid` | UNIQUE | URL routing by GUID |
| polls | `created_by_id, status, is_public` | COMPOSITE | Feed queries |
| surveys | `uid` | UNIQUE | URL routing by GUID |
| surveys | `created_by_id, status, is_public` | COMPOSITE | Feed queries |
| poll_questions | `poll_id, order_index` | COMPOSITE | Ordered question retrieval |
| poll_submissions | `poll_question_id, respondent_id` | UNIQUE | Duplicate submission prevention |
| survey_responses | `survey_id, respondent_id` | UNIQUE | Duplicate submission prevention |
| invitations | `invitee_id, status` | COMPOSITE | Pending invitations for a user |
| notifications | `recipient_id, is_read` | COMPOSITE | Unread notification count |
| invite_allowlist_entries | `owner_id, allowed_user_id` | UNIQUE | Duplicate allowlist entry prevention |

---

## Value Objects / JSONB Documents (C# records in BSDCPolls.Contracts)

These are defined in `BSDCPolls.Contracts` and used as EF Core value converter targets in
`BSDCPolls.Api.Data`.

```csharp
// Survey question tree — stored in Survey.QuestionTree (jsonb)
public sealed record SurveyQuestionTreeDocument(
    IReadOnlyList<SurveyQuestionNode> Questions);

public sealed record SurveyQuestionNode(
    Guid Uid,
    string Text,
    SurveyAnswerType AnswerType,
    bool IsRequired,
    IReadOnlyList<SurveyChoiceOption>? Choices,
    IReadOnlyList<SurveyConditionalBranch>? Branches);

public sealed record SurveyChoiceOption(Guid Uid, string Text);

public sealed record SurveyConditionalBranch(
    Guid ParentChoiceUid,
    IReadOnlyList<SurveyQuestionNode> Questions);

// Survey response answers — stored in SurveyResponse.AnswersJson (jsonb)
public sealed record SurveyAnswersDocument(
    IReadOnlyList<SurveyAnswerEntry> Answers);

public sealed record SurveyAnswerEntry(
    Guid QuestionUid,
    SurveyAnswerType AnswerType,
    Guid? SelectedChoiceUid,      // MultipleChoice answers
    string? TextValue,            // ShortText / LongText answers
    Guid? DocumentUid);           // DocumentUpload answers → SurveyDocument.Uid

// Enums
public enum SurveyAnswerType { MultipleChoice = 0, ShortText = 1, LongText = 2, DocumentUpload = 3 }
public enum PollStatus { Draft = 0, Active = 1, Closed = 2 }
public enum SurveyStatus { Draft = 0, Published = 1, Closed = 2 }
public enum InvitePermission { Everyone = 0, Nobody = 1, AllowlistOnly = 2 }
public enum InvitationStatus { Pending = 0, Viewed = 1, Declined = 2 }
```
