# API Contracts: SignalR Hubs

**Date**: 2026-06-10
**Service**: BSDCPolls BFF — SignalR hubs hosted by `BSDCPolls.BFF`
**Client package**: `@microsoft/signalr` (Angular)
**Auth**: All connections require `Authorization: Bearer <JWT>` via the `accessTokenFactory`
option in the `HubConnectionBuilder`. Unauthenticated connections are rejected at the hub level.

---

## Hub 1: PollHub

**Connection URL**: `/hubs/poll`

**Query params** (required on connection):
- `pollUid`: `guid` — the poll this connection is for

**Purpose**: Real-time question broadcasting and live vote-count updates for a specific poll
session. Participants and the creator connect to this hub when viewing a poll.

**Authorization on connect**: The hub middleware validates the JWT, resolves the current user,
then checks:
- If the poll is public: any authenticated user may connect
- If the poll is invite-only: the user must be the creator OR appear in the `Invitations` table
  for this poll

If the user is not authorized, the connection is rejected with `HubException`.

**Group management**: On successful connection, the user is automatically added to a SignalR
group keyed by `pollUid.ToString()`. On disconnect, SignalR removes them from all groups
automatically.

**Creator tracking**: When the poll creator connects, their `ConnectionId` is registered in
the `IPollSessionTracker` service (BFF-scoped, in-memory). When they disconnect, it is removed.

---

### Server → Client Methods (events pushed from BFF to Angular)

#### `QuestionPushed`

Sent to all group members (except the creator's own connection) when the creator pushes a
new question.

**Payload** (`PollQuestionPushedPayload`):
```typescript
interface PollQuestionPushedPayload {
  questionUid: string;           // guid
  text: string;
  orderIndex: number;
  pushedAt: string;              // ISO 8601 UTC datetime
  options: Array<{
    optionUid: string;           // guid
    text: string;
    orderIndex: number;
  }>;
}
```

**Angular handling**: The NgRX Poll Session store updates the active question. The UI
displays the new question as a popup/modal or replaces the current question panel.

---

#### `VoteCountUpdated`

Sent only to the creator's connection (via `Clients.Client(creatorConnectionId)`) when any
participant submits a vote.

**Payload** (`PollVoteCountUpdatedPayload`):
```typescript
interface PollVoteCountUpdatedPayload {
  questionUid: string;           // guid
  options: Array<{
    optionUid: string;           // guid
    voteCount: number;
    percentage: number;          // 0.00–100.00
  }>;
  totalVotes: number;
}
```

**Angular handling**: The NgRX Poll Session store updates the results dashboard reactively.

---

#### `PollStatusChanged`

Sent to all group members when the poll status changes (e.g., Active → Closed).

**Payload** (`PollStatusChangedPayload`):
```typescript
interface PollStatusChangedPayload {
  pollUid: string;               // guid
  newStatus: 'Active' | 'Closed';
  changedAt: string;             // ISO 8601 UTC datetime
}
```

---

### Client → Server Methods (invocations from Angular to BFF)

#### `SubmitVote`

Called by a participant to submit their answer to the currently active question.

**Parameters**:
```typescript
interface SubmitVoteArgs {
  questionUid: string;           // guid
  selectedOptionUid: string;     // guid
}
```

**Returns**: `void` — errors are thrown as `HubException` with a message string.

**Server-side behaviour**:
1. Validates the user is authorized for this poll
2. Validates the question is the currently active (most recently pushed) question
3. Validates the user has not already submitted for this question
4. Persists the `PollSubmission` via the backend API
5. Sends `VoteCountUpdated` to the creator's connection with updated tallies

**HubException messages** (for Angular error handling):
- `"VOTE_DUPLICATE"` — user already submitted for this question
- `"VOTE_NOT_AUTHORIZED"` — user is not eligible for this poll
- `"VOTE_QUESTION_NOT_ACTIVE"` — question is not the current active question
- `"VOTE_POLL_NOT_ACTIVE"` — poll is not in Active status

---

### Reconnect Strategy (Angular)

```typescript
// Recommended HubConnectionBuilder configuration
const connection = new HubConnectionBuilder()
  .withUrl('/hubs/poll', {
    accessTokenFactory: () => authStore.accessToken(),
    withCredentials: false
  })
  .withAutomaticReconnect([0, 2000, 5000, 10000, 30000])
  .build();
```

On reconnect, Angular MUST call `GET /api/polls/{pollUid}` to re-sync the current poll state
(in case questions were pushed during the disconnection). The SignalR connection resumes
delivering future events after the REST sync.

---

## Hub 2: NotificationHub

**Connection URL**: `/hubs/notifications`

**Query params**: None.

**Purpose**: Real-time delivery of invitation notifications to connected users. Each authenticated
user connects to this hub for the duration of their session.

**Authorization on connect**: Standard JWT validation. Any authenticated user may connect.

**Group management**: On connection, the user is added to a personal group keyed by
`userId.ToString()`. This group receives their personal notifications only.

---

### Server → Client Methods

#### `InvitationReceived`

Sent to the target user's personal group when another user creates an invitation for them.

**Payload** (`InvitationReceivedPayload`):
```typescript
interface InvitationReceivedPayload {
  notificationUid: string;       // guid — for marking as read
  invitationUid: string;         // guid
  inviterUsername: string;
  pollUid?: string;              // guid — present if invitation is for a poll
  pollTitle?: string;
  surveyUid?: string;            // guid — present if invitation is for a survey
  surveyTitle?: string;
  createdOn: string;             // ISO 8601 UTC datetime
}
```

**Angular handling**: The NgRX Notifications store increments the unread count and prepends
the notification to the list. The notification bell badge updates reactively.

---

### Client → Server Methods

NotificationHub has no client-invocable methods. All notification interactions (marking read,
fetching history) are handled via REST endpoints (`GET/PATCH /api/notifications`).

---

### Reconnect Strategy (Angular)

```typescript
const connection = new HubConnectionBuilder()
  .withUrl('/hubs/notifications', {
    accessTokenFactory: () => authStore.accessToken()
  })
  .withAutomaticReconnect([0, 2000, 5000, 10000, 60000])
  .build();
```

On reconnect, Angular MUST call `GET /api/notifications?unreadOnly=true` to re-sync any
notifications received while disconnected.

---

## Internal: BFF → BFF Notification Trigger

When the backend API creates an `Invitation` record, it calls an internal BFF endpoint to
trigger the SignalR push. This is a BFF-to-BFF HTTP call on the internal Aspire network.

**Internal endpoint**: `POST /internal/notifications/push`  
**Auth**: Aspire service-to-service (internal network only; not exposed externally)

**Request**:
```json
{
  "targetUserId": "integer",
  "payload": { /* InvitationReceivedPayload */ }
}
```

This endpoint is NOT part of the public BFF API and MUST NOT be exposed through the public
route configuration.

---

## Hub Contract Summary

| Hub | URL | Direction | Event | Trigger |
|-----|-----|-----------|-------|---------|
| PollHub | `/hubs/poll?pollUid=` | Server → Client | `QuestionPushed` | Creator pushes a question |
| PollHub | `/hubs/poll?pollUid=` | Server → Client | `VoteCountUpdated` | Participant submits a vote |
| PollHub | `/hubs/poll?pollUid=` | Server → Client | `PollStatusChanged` | Creator changes poll status |
| PollHub | `/hubs/poll?pollUid=` | Client → Server | `SubmitVote` | Participant answers a question |
| NotificationHub | `/hubs/notifications` | Server → Client | `InvitationReceived` | User is invited to a poll or survey |
