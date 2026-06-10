# Quickstart & Validation Guide: BSDCPolls Platform

**Date**: 2026-06-10
**Purpose**: Runnable validation scenarios that prove each user story works end-to-end against
the full local Aspire stack. This is a run guide, not an implementation reference.

---

## Prerequisites

Before running any scenario, the full local Aspire stack must be running:

```bash
# From the repository root
cd src/BSDCPolls.AppHost
dotnet run
```

The Aspire dashboard opens at `http://localhost:15888`. All services must show "Running" status:
- `bsdcpolls-bff` — BFF ASP.NET Core
- `bsdcpolls-api` — Backend API ASP.NET Core
- `bsdcpolls-migration-worker` — (completes and exits; status shows "Finished")
- `supabase-postgres` — PostgreSQL
- `supabase-gotrue` — Auth service
- `signoz` — Observability (accessible at `http://localhost:3301`)

The Angular dev server runs separately:

```bash
# From BSDCPolls.Web/
npm install
npm run generate-api    # Generate TypeScript from BFF OpenAPI spec
npm start               # Angular dev server at http://localhost:4200
```

---

## Scenario 1: User Registration and Login (User Story 1 — P1)

**Validates**: FR-001, FR-002, FR-003, FR-004, SC-001

**Steps**:

1. Navigate to `http://localhost:4200`
2. Click **Sign Up**
3. Enter only a password (12+ characters, mixed case, digit, special character). No email, no name.
4. Click **Create Account**

**Expected**: An auto-generated username (e.g., "swift-amber-moon") is displayed. No other
personal data is requested or shown. The user is redirected to the home page.

**Verify**:
- The username is three lowercase words separated by hyphens
- No profane word appears in any component
- The Supabase PostgreSQL table `application_users` has one row with the generated username
  and a synthetic email column that is never surfaced in the UI
- No real email, name, or personal information is stored in any table

5. Log out, then navigate back and log in using the displayed username and the same password.

**Expected**: Login succeeds. The user sees their home feed.

6. Attempt to log in with the username but an incorrect password.

**Expected**: Login rejected with a clear error. No account lockout for this single attempt.

---

## Scenario 2: Create and Conduct a Live Poll (User Stories 2 & 3 — P2)

**Validates**: FR-007–FR-013, SC-002, SC-003, SC-004, SC-009

**Requires**: Two browser sessions (Creator = User A, Participant = User B). Open two browser
profiles or use an incognito window for one.

**Steps (Creator)**:

1. User A logs in. On the home feed, click **Create Poll**.
2. Enter title "Coffee Preferences" and toggle **Invite Only**.
3. Click **Create**. Note the unique poll URL (e.g., `/polls/3f7a1c2b-...`).
4. The poll dashboard opens. Status shows **Draft**.
5. Click **Activate Poll** to start the session.

**Steps (Invite Participant)**:

6. Creator: Click **Invite** and enter User B's username. Click **Send Invitation**.

**Steps (Participant)**:

7. User B refreshes or checks their notification bell. An unread badge appears.
8. User B clicks the bell, sees the invitation from User A to "Coffee Preferences".
9. User B clicks the invitation link (or navigates to the unique poll URL).
10. User B sees the poll waiting for the first question.

**Steps (Live Session)**:

11. Creator: Click **Add Question**, enter "Which coffee do you prefer?" with options
    "Espresso", "Latte", "Cappuccino". Select **Push Now**.

**Expected (within 1 second)**:
- User B sees the question appear as a popup without refreshing the page. (Validates SC-002)
- Creator dashboard does not yet show votes.

12. User B selects "Latte" and clicks **Submit**.

**Expected (within 1 second)**:
- Creator's dashboard shows 1 vote for "Latte" (100%). (Validates SC-003)
- User B cannot submit again for the same question (duplicate rejected).

13. Creator pushes a second question: "Milk preference?" with "Full Fat", "Oat", "Almond".
14. User B answers "Oat".
15. Creator closes the poll (status → Closed).

**Verify**:
- `poll_submissions` table has 2 rows for User B
- `poll_questions` table has 2 rows, both with `pushed_at` set
- User B attempting to access the unique poll URL of a different invite-only poll returns 403

---

## Scenario 3: Create a Conditional Survey (User Stories 4 & 5 — P3)

**Validates**: FR-014–FR-020, SC-006, SC-009

**Requires**: Two browser sessions (Creator = User A, Respondent = User B).

**Steps (Creator builds survey)**:

1. User A navigates to home feed → **Surveys** tab → **Create Survey**.
2. Title: "Post-Event Feedback". Toggle **Invite Only**.
3. Add question 1: "How was your overall experience?" — Type: **Multiple Choice**
   - Choices: "Excellent", "Good", "Poor"
4. Under the "Poor" choice, add a conditional follow-up question:
   - "What specifically disappointed you?" — Type: **Long Text**
5. Add question 2 (top-level): "Would you upload your feedback form?" — Type: **Document Upload**
6. Click **Publish Survey**. Note the unique survey URL.

**Expected**: Survey status changes to Published. The unique URL is displayed.

**Steps (Respondent — happy path: chooses "Excellent")**:

7. User B receives invitation notification and navigates to the survey.
8. Answers Q1: "Excellent".

**Expected**: The conditional follow-up ("What specifically disappointed you?") does NOT appear.

9. Q2 appears: "Would you upload your feedback form?"
10. User B uploads a valid PDF (≤ 10 MB).
11. User B clicks **Submit Survey**.

**Expected**: Submission confirmed. `survey_responses.is_complete = true`.

**Steps (Respondent — sad path: chooses "Poor" and sees conditional question)**:

12. Register a third user (User C). Creator invites User C.
13. User C answers Q1: "Poor".

**Expected**: The conditional follow-up "What specifically disappointed you?" appears.

14. User C fills in the text, uploads a PDF for Q2, and submits.

**Steps (Non-PDF rejection)**:

15. User C's upload step: try uploading a `.docx` file.

**Expected**: Upload rejected with a clear "PDF files only" error before submission.

**Verify**:
- `surveys.question_tree` column contains valid JSONB with the conditional branch structure
- `survey_responses.answers_json` for User B has no entry for the conditional question
- `survey_responses.answers_json` for User C has an entry for the conditional question with text
- `survey_documents` table has entries for each uploaded PDF

---

## Scenario 4: Home Feed with Privacy Controls (User Story 6 — P4)

**Validates**: FR-022, FR-026, FR-027, FR-028, FR-032, FR-033, SC-005

**Requires**: Three users — Creator (User A), Invited (User B), Non-Invited (User C).

**Steps**:

1. User A creates a **public** poll titled "Public Lunch Poll" and activates it.
2. User B logs in. Navigate to home feed → **Polls** tab.

**Expected**: "Public Lunch Poll" appears in User B's feed (public discovery enabled by default).

3. User B navigates to **Profile** → disables **Show Public Content**.
4. User B refreshes the Polls feed.

**Expected**: "Public Lunch Poll" is no longer visible. Only personally invited polls appear.

5. User B re-enables **Show Public Content**. "Public Lunch Poll" reappears.
6. User A opens the **Results** tab.

**Expected**: User A sees "Public Lunch Poll" with current vote counts. (SC-005: Feed loads
within 2 seconds — measure visually or via browser DevTools Network tab.)

---

## Scenario 5: Notification Bell and Invite Permission Controls (User Stories 7 & 8 — P4/P5)

**Validates**: FR-023–FR-025, FR-029–FR-034, SC-007, SC-008

**Steps (basic invite notification)**:

1. User A invites User B to a poll by username.
2. Within 5 seconds, User B's notification bell shows an unread badge count of 1. (SC-007)
3. User B clicks the bell. The notification panel opens showing the invitation.
4. The badge count drops to 0 after viewing.

**Steps (invite permission — Nobody)**:

5. User B navigates to **Profile** → sets **Invite Permission** to **Nobody**.
6. User A attempts to invite User B to a new poll.

**Expected**: User A sees a message: "This user is not accepting invitations." No notification
is delivered to User B.

**Steps (invite permission — Allowlist Only)**:

7. User B sets **Invite Permission** to **Selected Users Only**.
8. User B enters User A's username in the allowlist. The system validates the username exists
   before saving.
9. User B tries to enter a non-existent username (e.g., "fake-user-xyz").

**Expected**: Non-existent username is rejected before the entry is saved.

10. User A invites User B to another poll.

**Expected**: Invitation delivered and notification received (User A is on the allowlist).

11. User C (not on the allowlist) attempts to invite User B.

**Expected**: User C sees "This user is not accepting invitations from you." No notification
to User B.

**Steps (username change)**:

12. User B navigates to **Profile** → clicks **Change Username**.

**Expected**: A new auto-generated username is displayed immediately. The old username is
no longer valid for login. User B is prompted to note their new username (no recovery exists).

---

## Scenario 6: Access Control Enforcement (Cross-Cutting — P1 gate)

**Validates**: FR-006, FR-021, SC-009, SC-010

**Steps**:

1. User A creates an invite-only poll and does NOT invite User C.
2. User C attempts to navigate directly to the poll's unique URL.

**Expected**: HTTP 403 — poll content is not displayed.

3. User C attempts to call `GET /api/polls/{pollUid}` directly with their JWT.

**Expected**: 403 response with error body containing `traceId`. No poll data leaked.

4. Inspect the PostgreSQL `application_users` table.

**Expected**: Only `id`, `uid`, `username`, `supabase_user_id`, `is_active`, audit timestamps
and FKs. No email columns, no name columns, no phone columns.

5. Inspect the BFF error response body for any error scenario.

**Expected**: Response body contains only `traceId`, `status`, `message`, and `errors`.
No stack traces, no exception type names, no internal service names.

---

## Scenario 7: Observability Verification

**Validates**: Principle XI (Observability & Structured Logging)

**Steps**:

1. Navigate to SigNoz at `http://localhost:3301`.
2. Trigger a poll creation and a vote submission in the Angular app.
3. In SigNoz Traces, filter by service `bsdcpolls-bff`.

**Expected**: Distributed traces show the full span tree:
- Angular request → BFF controller → BFF.Business service → API (via internal call) → EF Core
  database query → response

4. In SigNoz Logs, search for `PollId` structured field.

**Expected**: Structured log entries show named placeholders (`{PollId}`, `{UserId}`) as
queryable fields, not interpolated strings.

5. Trigger a 404 error (navigate to a non-existent poll URL).

**Expected**: Error response body contains `traceId`. Navigate to SigNoz Traces with that
trace ID to find the full trace.

---

## Scenario 8: Code Quality Gates (CI Validation)

**Validates**: Principles IX, XII, XV

```bash
# From repository root

# C# formatting check
dotnet csharpier --check .

# C# build with warnings-as-errors
dotnet build --configuration Release

# Angular TypeScript generation (check for diffs)
cd BSDCPolls.Web && npm run generate-api && git diff --exit-code src/app/generated/

# Angular linting (zero warnings)
ng lint --max-warnings 0

# Angular formatting check
npx prettier --check .

# No test files exist
find . -name "*.spec.ts" -o -name "*Tests.csproj" | grep -q . && echo "TEST FILES FOUND - REJECT" || echo "No test files - OK"
```

**Expected**: All commands exit with code 0. Any non-zero exit code is a CI pipeline failure.

---

## Artifact References

| Artifact | Location |
|----------|----------|
| Entity data model | [data-model.md](./data-model.md) |
| REST API contracts | [contracts/api-endpoints.md](./contracts/api-endpoints.md) |
| SignalR hub contracts | [contracts/signalr-hubs.md](./contracts/signalr-hubs.md) |
| DTO validation rules | [contracts/dto-schemas.md](./contracts/dto-schemas.md) |
| Technical decisions | [research.md](./research.md) |
| Feature specification | [spec.md](./spec.md) |
