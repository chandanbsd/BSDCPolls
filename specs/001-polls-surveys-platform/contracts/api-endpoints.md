# API Contracts: REST Endpoints

**Date**: 2026-06-10
**Service**: BSDCPolls BFF (Angular → BFF only; frontend MUST NOT call backend API directly)
**Base URL**: `https://bff.bsdcpolls.internal` (resolved via Aspire service discovery)
**Auth**: `Authorization: Bearer <Supabase GoTrue JWT>` on all authenticated endpoints
**Trace propagation**: All requests carry `traceparent` header injected by Angular HTTP interceptor

---

## Auth Endpoints

### POST /api/auth/register

Register a new account. The system generates a username; the caller provides only a password.

**Request** (`RegisterRequest`):
```json
{
  "password": "string (min 12 chars, at least 1 upper, 1 lower, 1 digit, 1 special)"
}
```

**Response 201** (`RegisterResponse`):
```json
{
  "username": "string (generated, e.g. 'swift-amber-moon')",
  "userUid": "guid"
}
```

**Response 400**: FluentValidation failure — password does not meet strength requirements.
**Response 409**: Username generation collision after max retries (extremely rare).

**Notes**:
- BFF generates a synthetic Supabase email (`<random-uuid>@internal.bsdcpolls`) and registers
  with GoTrue using that email + the provided password.
- The generated username is stored in `ApplicationUser.Username`.
- The synthetic email is NEVER returned to the caller or shown in any UI.
- Response does NOT include a token — caller must call `/api/auth/login` after registration.

---

### POST /api/auth/login

Authenticate with username and password. Returns a JWT for use in subsequent requests.

**Request** (`LoginRequest`):
```json
{
  "username": "string",
  "password": "string"
}
```

**Response 200** (`LoginResponse`):
```json
{
  "accessToken": "string (Supabase JWT)",
  "expiresAt": "datetime (ISO 8601 UTC)",
  "userUid": "guid",
  "username": "string"
}
```

**Response 401**: Invalid username or password.

**Notes**:
- BFF resolves `username → SupabaseUserId → synthetic email`, then calls GoTrue sign-in.

---

## User / Profile Endpoints

### GET /api/users/me

Get the current user's profile.

**Auth**: Required.

**Response 200** (`UserProfileResponse`):
```json
{
  "userUid": "guid",
  "username": "string",
  "createdOn": "datetime (ISO 8601 UTC)"
}
```

---

### POST /api/users/me/username/change

Request a new auto-generated username.

**Auth**: Required.

**Request**: Empty body (no parameters).

**Response 200** (`UsernameChangeResponse`):
```json
{
  "newUsername": "string"
}
```

**Response 429**: Too many username change requests (rate-limited to 3 per 24 hours).

---

## Privacy Settings Endpoints

### GET /api/users/me/privacy

Get the current user's privacy settings.

**Auth**: Required.

**Response 200** (`PrivacySettingsResponse`):
```json
{
  "showPublicContent": "boolean",
  "invitePermission": "Everyone | Nobody | AllowlistOnly",
  "allowlist": [
    { "userUid": "guid", "username": "string" }
  ]
}
```

---

### PUT /api/users/me/privacy

Update privacy settings.

**Auth**: Required.

**Request** (`UpdatePrivacySettingsRequest`):
```json
{
  "showPublicContent": "boolean",
  "invitePermission": "Everyone | Nobody | AllowlistOnly"
}
```

**Response 200**: Updated `PrivacySettingsResponse`.
**Response 400**: FluentValidation failure.

---

### POST /api/users/me/privacy/allowlist

Add a user to the invite allowlist.

**Auth**: Required.

**Request** (`AddAllowlistEntryRequest`):
```json
{
  "username": "string"
}
```

**Response 201** (`AllowlistEntryResponse`):
```json
{
  "userUid": "guid",
  "username": "string"
}
```

**Response 400**: Username not found (validated by BFF before accepting).
**Response 409**: User already on the allowlist.

---

### DELETE /api/users/me/privacy/allowlist/{userUid}

Remove a user from the invite allowlist.

**Auth**: Required.

**Response 204**: Removed.
**Response 404**: Entry not found.

---

## Poll Endpoints

### GET /api/polls

Get the feed of polls visible to the current user (public + invited).

**Auth**: Required.

**Query params**:
- `status`: `Active | Closed | Draft` (optional; default: `Active`)
- `page`: integer (default 1), `pageSize`: integer (default 20, max 50)

**Response 200** (`PollFeedResponse`):
```json
{
  "items": [
    {
      "pollUid": "guid",
      "title": "string",
      "isPublic": "boolean",
      "status": "Draft | Active | Closed",
      "creatorUsername": "string",
      "questionCount": "integer",
      "createdOn": "datetime (ISO 8601 UTC)",
      "invitedAt": "datetime? (null if public access)"
    }
  ],
  "totalCount": "integer",
  "page": "integer",
  "pageSize": "integer"
}
```

---

### POST /api/polls

Create a new poll.

**Auth**: Required.

**Request** (`CreatePollRequest`):
```json
{
  "title": "string (1–200 chars)",
  "isPublic": "boolean"
}
```

**Response 201** (`PollDetailResponse`):
```json
{
  "pollUid": "guid",
  "title": "string",
  "isPublic": "boolean",
  "status": "Draft",
  "createdOn": "datetime (ISO 8601 UTC)"
}
```

---

### GET /api/polls/{pollUid}

Get poll details including all pushed questions and their answer options.

**Auth**: Required (invitee or creator or public poll).

**Response 200** (`PollDetailResponse`):
```json
{
  "pollUid": "guid",
  "title": "string",
  "isPublic": "boolean",
  "status": "Draft | Active | Closed",
  "createdOn": "datetime (ISO 8601 UTC)",
  "isCreator": "boolean",
  "questions": [
    {
      "questionUid": "guid",
      "text": "string",
      "orderIndex": "integer",
      "pushedAt": "datetime? (ISO 8601 UTC)",
      "options": [
        { "optionUid": "guid", "text": "string", "orderIndex": "integer" }
      ]
    }
  ]
}
```

**Response 403**: User is not authorized to view this poll.
**Response 404**: Poll not found or soft-deleted.

---

### PATCH /api/polls/{pollUid}/status

Change poll status (activate or close).

**Auth**: Required (creator only).

**Request** (`ChangePollStatusRequest`):
```json
{
  "status": "Active | Closed"
}
```

**Response 200**: Updated `PollDetailResponse`.
**Response 400**: Invalid transition (e.g., Closed → Active).
**Response 403**: Not the poll creator.

---

### POST /api/polls/{pollUid}/questions

Add a question to the poll (staged or immediately pushed).

**Auth**: Required (creator only).

**Request** (`AddPollQuestionRequest`):
```json
{
  "text": "string (1–500 chars)",
  "options": [
    { "text": "string (1–200 chars)", "orderIndex": "integer" }
  ],
  "pushImmediately": "boolean (if true, sets PushedAt and broadcasts via SignalR)"
}
```

**Response 201** (`PollQuestionResponse`):
```json
{
  "questionUid": "guid",
  "text": "string",
  "orderIndex": "integer",
  "pushedAt": "datetime? (ISO 8601 UTC)",
  "options": [
    { "optionUid": "guid", "text": "string", "orderIndex": "integer" }
  ]
}
```

**Notes**: When `pushImmediately = true`, the BFF broadcasts `QuestionPushed` to all connected
poll group members via `PollHub`.

---

### POST /api/polls/{pollUid}/questions/{questionUid}/push

Push a previously staged question to live participants.

**Auth**: Required (creator only).

**Request**: Empty body.

**Response 200**: Updated `PollQuestionResponse` with `pushedAt` set.
**Response 400**: Poll is not Active, or question already pushed.
**Response 403**: Not the creator.

---

### POST /api/polls/{pollUid}/submissions

Submit a vote for the currently active poll question.

**Auth**: Required (invitee or public poll participant).

**Request** (`SubmitPollVoteRequest`):
```json
{
  "questionUid": "guid",
  "selectedOptionUid": "guid"
}
```

**Response 201** (`PollSubmissionResponse`):
```json
{
  "submissionUid": "guid",
  "questionUid": "guid",
  "selectedOptionUid": "guid",
  "submittedAt": "datetime (ISO 8601 UTC)"
}
```

**Response 400**: Question not pushed, poll not Active, or invalid option.
**Response 409**: User already submitted for this question.
**Response 403**: User not authorized to submit to this poll.

---

### GET /api/polls/{pollUid}/results

Get aggregated vote counts for a poll (all questions).

**Auth**: Required (creator or post-close public view — see notes).

**Response 200** (`PollResultsResponse`):
```json
{
  "pollUid": "guid",
  "title": "string",
  "status": "Draft | Active | Closed",
  "questions": [
    {
      "questionUid": "guid",
      "text": "string",
      "pushedAt": "datetime? (ISO 8601 UTC)",
      "totalVotes": "integer",
      "options": [
        {
          "optionUid": "guid",
          "text": "string",
          "voteCount": "integer",
          "percentage": "number (0.00–100.00)"
        }
      ]
    }
  ]
}
```

**Notes**: During an active poll, only the creator can access results. After the poll is Closed,
the creator can still access. Live vote updates during a session are pushed via SignalR, not this
endpoint.

---

### POST /api/polls/{pollUid}/invitations

Invite a user to a poll by their username.

**Auth**: Required (creator only).

**Request** (`CreateInvitationRequest`):
```json
{
  "targetUsername": "string"
}
```

**Response 201** (`InvitationResponse`):
```json
{
  "invitationUid": "guid",
  "targetUsername": "string",
  "targetUserUid": "guid",
  "createdOn": "datetime (ISO 8601 UTC)"
}
```

**Response 400**: Username not found, or user has disabled all invitations.
**Response 409**: User already invited to this poll.

---

## Survey Endpoints

### GET /api/surveys

Get the feed of surveys visible to the current user.

**Auth**: Required.

**Query params**: `status`, `page`, `pageSize` (same as `/api/polls`).

**Response 200** (`SurveyFeedResponse`): Same shape as `PollFeedResponse` with `surveyUid`.

---

### POST /api/surveys

Create a new survey.

**Auth**: Required.

**Request** (`CreateSurveyRequest`):
```json
{
  "title": "string (1–200 chars)",
  "isPublic": "boolean",
  "questionTree": {
    "questions": [ /* SurveyQuestionNode array — see data-model.md */ ]
  }
}
```

**Response 201** (`SurveyDetailResponse`):
```json
{
  "surveyUid": "guid",
  "title": "string",
  "isPublic": "boolean",
  "status": "Draft",
  "questionTree": { /* SurveyQuestionTreeDocument */ },
  "createdOn": "datetime (ISO 8601 UTC)"
}
```

---

### GET /api/surveys/{surveyUid}

Get survey details including the full question tree.

**Auth**: Required (invitee, creator, or public survey).

**Response 200** (`SurveyDetailResponse`): Full survey including `questionTree`.
**Response 403**: Not authorized.
**Response 404**: Not found.

---

### PATCH /api/surveys/{surveyUid}/status

Change survey status.

**Auth**: Required (creator only).

**Request** (`ChangeSurveyStatusRequest`):
```json
{
  "status": "Published | Closed"
}
```

**Response 200**: Updated `SurveyDetailResponse`.

---

### PUT /api/surveys/{surveyUid}/questions

Replace the entire question tree (only allowed in Draft status).

**Auth**: Required (creator only).

**Request** (`UpdateSurveyQuestionsRequest`):
```json
{
  "questionTree": { /* SurveyQuestionTreeDocument */ }
}
```

**Response 200**: Updated `SurveyDetailResponse`.
**Response 400**: Survey is not in Draft status.

---

### POST /api/surveys/{surveyUid}/responses

Start or update a survey response. Call repeatedly as the respondent progresses; submit as final
with `isSubmitting: true`.

**Auth**: Required (invitee or public survey participant).

**Request** (`SaveSurveyResponseRequest`):
```json
{
  "answers": [
    {
      "questionUid": "guid",
      "answerType": "MultipleChoice | ShortText | LongText | DocumentUpload",
      "selectedChoiceUid": "guid? (MultipleChoice only)",
      "textValue": "string? (ShortText/LongText only)",
      "documentUid": "guid? (DocumentUpload — must be uploaded first)"
    }
  ],
  "isSubmitting": "boolean (true = final submission; false = save progress)"
}
```

**Response 200/201** (`SurveyResponseStatusResponse`):
```json
{
  "responseUid": "guid",
  "isComplete": "boolean",
  "submittedAt": "datetime? (ISO 8601 UTC)"
}
```

**Response 400**: Validation failure (required question unanswered on final submission,
unrecognized questionUid, etc.).
**Response 409**: Survey already submitted (isComplete = true already).
**Response 403**: Not authorized to respond.

---

### POST /api/surveys/{surveyUid}/responses/{responseUid}/documents

Upload a PDF for a document-upload question. Returns a `documentUid` for use in the response
answers.

**Auth**: Required (respondent who owns the response).

**Request**: `multipart/form-data`
- `file`: PDF file (Content-Type must be `application/pdf`; max 10 MB)
- `questionUid`: guid (which question this PDF answers)

**Response 201** (`SurveyDocumentResponse`):
```json
{
  "documentUid": "guid",
  "fileName": "string",
  "fileSizeBytes": "integer"
}
```

**Response 400**: Not a PDF, file exceeds 10 MB, or invalid `questionUid`.
**Response 403**: Not the response owner.

---

### GET /api/surveys/{surveyUid}/results

Get aggregated survey results (creator only).

**Auth**: Required (creator only).

**Response 200** (`SurveyResultsResponse`):
```json
{
  "surveyUid": "guid",
  "title": "string",
  "totalResponses": "integer",
  "completeResponses": "integer",
  "questionSummaries": [
    {
      "questionUid": "guid",
      "text": "string",
      "answerType": "MultipleChoice | ShortText | LongText | DocumentUpload",
      "responseCount": "integer",
      "choiceTallies": [
        { "choiceUid": "guid", "text": "string", "count": "integer" }
      ],
      "textAnswers": ["string"],
      "documentCount": "integer"
    }
  ]
}
```

---

### POST /api/surveys/{surveyUid}/invitations

Invite a user to a survey. Same shape as poll invitations.

**Auth**: Required (creator only).

**Request/Response**: Same as `POST /api/polls/{pollUid}/invitations`.

---

## Notification Endpoints

### GET /api/notifications

Get the current user's notifications (paginated, newest first).

**Auth**: Required.

**Query params**: `unreadOnly`: boolean (default false), `page`, `pageSize`.

**Response 200** (`NotificationListResponse`):
```json
{
  "unreadCount": "integer",
  "items": [
    {
      "notificationUid": "guid",
      "isRead": "boolean",
      "createdOn": "datetime (ISO 8601 UTC)",
      "invitation": {
        "invitationUid": "guid",
        "inviterUsername": "string",
        "pollUid": "guid?",
        "pollTitle": "string?",
        "surveyUid": "guid?",
        "surveyTitle": "string?"
      }
    }
  ],
  "totalCount": "integer",
  "page": "integer",
  "pageSize": "integer"
}
```

---

### PATCH /api/notifications/{notificationUid}/read

Mark a notification as read.

**Auth**: Required (must be the notification recipient).

**Request**: Empty body.

**Response 200** (`NotificationReadResponse`):
```json
{
  "notificationUid": "guid",
  "isRead": true,
  "readAt": "datetime (ISO 8601 UTC)"
}
```

---

### PATCH /api/notifications/read-all

Mark all unread notifications as read.

**Auth**: Required.

**Response 200**:
```json
{ "markedReadCount": "integer" }
```

---

## Error Response Shape

All error responses follow a consistent shape:

```json
{
  "traceId": "string (W3C traceparent trace-id component — safe to surface to user)",
  "status": "integer (HTTP status code)",
  "message": "string (user-readable, no stack traces or internal service names)",
  "errors": [
    { "field": "string?", "message": "string" }
  ]
}
```

**No stack traces, exception type names, or internal service names in any error response.**
The `traceId` is the only bridge between a user-reported error and the full SigNoz trace.
