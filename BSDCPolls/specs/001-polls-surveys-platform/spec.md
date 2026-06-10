# Feature Specification: BSDCPolls — Real-Time Polls & Surveys Platform

**Feature Branch**: `001-polls-surveys-platform`

**Created**: 2026-06-10

**Status**: Draft

**Input**: User description: "Implement an application that is used to publish either real time polls or surveys. In polls question will have a multiple choice option and user selection are considered as submissions and each polls dashboard shows the values. In a poll the user can submit as many question and possible answers in real time and the user will see new question popup and will be able to answer in real time. All of these are stored are relational database tables. In surveys questions will have questions and answers can be either multiple choice, single word or multi word, user can also be allowed to upload documents (only pdf allowed). Surveys can be more complex where the creator can make nested questions and answers. There we will store all the survey question and then all the survey options as NoSQL documents within PostGres. We also have strict adherence to most minimal data collection of our users..."

## User Scenarios & Testing *(mandatory)*

### User Story 1 - Anonymous-Free Account Registration (Priority: P1)

A new visitor creates an account. The system automatically generates a unique, three-word username (e.g., "swift-amber-moon") with no profanity. The user sets a secure password. No email address, real name, or any other personally identifiable information is collected or required — ever.

**Why this priority**: All other features depend on authenticated identity. This story defines the project's core privacy stance. Without it, nothing else works.

**Independent Test**: Can be fully tested by navigating to the sign-up screen, completing registration, and confirming the generated username is the only identifier assigned with no PII requested at any step.

**Acceptance Scenarios**:

1. **Given** a visitor is on the registration screen, **When** they submit only a secure password, **Then** the system auto-generates a unique three-word hyphen-separated username and creates their account with no other personal data stored.
2. **Given** a user has forgotten their username, **When** they attempt to recover access, **Then** the system offers no recovery option — the account is permanently inaccessible and a new account must be created.
3. **Given** a user has forgotten their password, **When** they attempt to recover access, **Then** the system offers no recovery option — the account is permanently inaccessible and a new account must be created.
4. **Given** a registration attempt, **When** the system generates a username, **Then** the username MUST NOT contain any profane word in any of its three components.

---

### User Story 2 - Create and Conduct a Live Poll (Priority: P2)

A logged-in user creates a live poll session and, in real time, adds questions with multiple-choice answer options. Invited or eligible participants see each new question appear automatically on their screen and can submit their answer immediately. The creator views a live results dashboard for each question as responses arrive.

**Why this priority**: Real-time polls are the primary differentiating feature. The value of the entire application depends on this working reliably.

**Independent Test**: Can be fully tested by one user creating a poll, inviting another, and both seeing real-time question pushes and vote tallies update without a page reload.

**Acceptance Scenarios**:

1. **Given** a logged-in user, **When** they create a new poll and designate it as public or invite-only, **Then** the poll is assigned a unique URL based on its identifier.
2. **Given** an active poll session, **When** the creator pushes a new question with answer choices, **Then** all currently-connected eligible participants see the question appear in real time without refreshing the page.
3. **Given** an eligible participant viewing an active question, **When** they select and submit an answer, **Then** the creator's dashboard updates the vote count for that question in real time.
4. **Given** a user who is not invited to a non-public poll, **When** they navigate to the poll's unique URL, **Then** they are denied access and see no poll content.

---

### User Story 3 - Participate in a Live Poll (Priority: P2)

An invited or eligible participant navigates to or is directed to a live poll. New questions appear as popups as the creator pushes them. The participant selects an answer and submits. Their answer is recorded immediately.

**Why this priority**: Without participants, a poll has no value. This story is the consumer-side of Story 2 and must be delivered together.

**Independent Test**: Can be fully tested by an invited user opening a poll URL, receiving a real-time question popup, and submitting an answer that appears on the creator's dashboard.

**Acceptance Scenarios**:

1. **Given** a participant is on the poll page, **When** the creator pushes a new question, **Then** the question appears as a popup or prominent panel without a page refresh.
2. **Given** a participant has already answered a question, **When** they attempt to answer the same question again, **Then** the additional submission is rejected.
3. **Given** a participant connects mid-session after some questions have already been pushed, **When** they join, **Then** they see only the currently active question; previously pushed questions are not re-displayed for answering.

---

### User Story 4 - Create a Structured Survey (Priority: P3)

A logged-in user builds a survey containing questions with configurable answer types: multiple-choice, single-word text, multi-word text, and optional PDF document upload. The creator can structure the survey with nested (conditional) questions where follow-up questions appear only based on the respondent's prior answers. The completed survey is published with a unique URL and visibility settings.

**Why this priority**: Surveys are the second major feature pillar. More complex to build than polls but also more flexible for structured data collection.

**Independent Test**: Can be fully tested by creating a multi-section survey with at least one conditional branch, sharing the link, and confirming a respondent navigates the correct question path.

**Acceptance Scenarios**:

1. **Given** a logged-in user creating a survey, **When** they add a question, **Then** they can choose the answer type: multiple-choice, single-word text, multi-word text, or document upload (PDF only).
2. **Given** a question with a defined conditional follow-up, **When** a respondent answers the parent question, **Then** only the follow-up questions relevant to that specific answer are shown; unrelated branches remain hidden.
3. **Given** a document-upload question, **When** the respondent attempts to upload a non-PDF file, **Then** the upload is rejected with a clear error message.
4. **Given** a completed survey definition, **When** the creator publishes it, **Then** the survey is assigned a unique URL and is accessible via that URL according to its visibility setting.

---

### User Story 5 - Complete a Survey (Priority: P3)

An invited or eligible respondent opens a survey via its unique URL, answers all applicable questions (navigating conditional branches as determined by their answers), optionally uploads a PDF where permitted, and submits the completed survey.

**Why this priority**: The consumer-side of surveys. Paired with Story 4.

**Independent Test**: Can be fully tested by an invited user completing a survey end-to-end, including navigating a conditional branch and uploading a valid PDF document.

**Acceptance Scenarios**:

1. **Given** a respondent progressing through a survey, **When** they answer a question with conditional follow-ups, **Then** subsequent questions appear or remain hidden based on their answer.
2. **Given** a PDF upload question, **When** the respondent uploads a valid PDF file within the permitted size limit, **Then** the upload is accepted and associated with their submission.
3. **Given** a respondent submitting a completed survey, **When** they confirm submission, **Then** their answers are permanently saved and they cannot re-submit the same survey.

---

### User Story 6 - Home Feed with Public and Invited Content (Priority: P4)

A logged-in user lands on the home page which presents three tabs: a Polls feed (showing public and personally-invited polls), a Surveys feed (showing public and personally-invited surveys), and a Results tab showing their own created polls and surveys with aggregated response data.

**Why this priority**: The primary discovery and engagement surface. Users need a way to find and access content without requiring direct URL sharing for every item.

**Independent Test**: Can be fully tested by observing that a user sees public polls/surveys alongside their personal invitations, while invite-only content from uninvited creators does not appear.

**Acceptance Scenarios**:

1. **Given** a logged-in user with public content discovery enabled in their profile, **When** they open the Polls feed tab, **Then** they see all public polls alongside polls they have been specifically invited to.
2. **Given** a logged-in user who has created polls and surveys, **When** they open the Results tab, **Then** they can view aggregated response data for each of their own polls and surveys.
3. **Given** a logged-in user who has disabled public content discovery in their profile, **When** they open either feed tab, **Then** they see only items they have been specifically invited to.

---

### User Story 7 - Invitation Notifications (Priority: P4)

A user receives a notification when another user invites them to a poll or survey by their username. A notification bell icon on the home page visually indicates pending unread invitations. Clicking the bell reveals the invitation list.

**Why this priority**: Notifications are the primary mechanism by which users discover invitations. Without them, invite-only content is practically undiscoverable.

**Independent Test**: Can be fully tested by User A inviting User B to a poll and confirming that User B's notification bell shows an unread indicator and the invitation appears in their notification panel.

**Acceptance Scenarios**:

1. **Given** User A creates a poll and invites User B by username, **When** User B views the home page, **Then** the notification bell shows an unread count badge.
2. **Given** User B opens the notification panel, **When** they view the invitation, **Then** the notification is marked as read and the unread count decreases.
3. **Given** a user who has disabled all invitations in their profile settings, **When** any user attempts to invite them, **Then** the invitation is not delivered and the inviting user receives a message indicating the target user is not accepting invitations.

---

### User Story 8 - User Profile & Privacy Controls (Priority: P5)

A logged-in user can access their profile by clicking their avatar or initials in the application toolbar. From the profile view, they can see their auto-generated username, request a new auto-generated username, control who can invite them to polls/surveys, and toggle whether their content appears in public feeds.

**Why this priority**: Privacy and identity controls are important but can be delivered after core poll/survey functionality is working.

**Independent Test**: Can be fully tested by opening the profile view, toggling each privacy setting, and confirming the changes are reflected in content visibility and invite permissions for other users.

**Acceptance Scenarios**:

1. **Given** a logged-in user, **When** they open the profile view, **Then** they see their current auto-generated username.
2. **Given** a user who requests a username change, **When** they confirm the request, **Then** the system immediately assigns a new auto-generated, non-profane, unique username.
3. **Given** a user configuring invite permissions to "selected users only", **When** they enter a username into the allowlist, **Then** the system validates that the username corresponds to an existing user before accepting it; non-existent usernames are rejected.
4. **Given** a user who has set invite permissions to "selected users only", **When** a user NOT on their allowlist attempts to invite them, **Then** the invitation is blocked and not delivered.

---

### Edge Cases

- What happens when a participant loses their connection mid-poll? They should be able to reconnect and see the current active question without losing previously submitted answers.
- What if two users register simultaneously and the same username is generated? The system must guarantee uniqueness and retry generation if a collision occurs.
- What if a PDF file exceeds a reasonable size limit during survey upload? The upload should be rejected with a clear size-limit message before submission.
- What if a user on another user's invite allowlist changes their username? The allowlist entry with the old username becomes invalid and is silently removed; the allowlist owner would need to re-add them by new username.
- What happens to in-progress poll answers if the creator ends or deletes the poll session? Existing submitted answers should be preserved and visible on the results tab; the session simply stops accepting new answers.
- What if a survey respondent navigates away mid-completion? Their progress should be saved so they can resume from where they left off within a reasonable time window; an uncompleted survey is not counted as a submission.
- What happens when a creator tries to invite a user who has disabled invitations entirely? The creator sees a clear message; the invitation is not recorded or delivered.

## Requirements *(mandatory)*

### Functional Requirements

**Authentication & Identity**

- **FR-001**: System MUST auto-generate a unique three-word hyphen-separated username for every new user; the user MUST NOT be allowed to choose their own username at registration.
- **FR-002**: System MUST NOT collect, store, or request any personally identifiable information at registration or at any later point (no email address, real name, phone number, date of birth, or any identifying attribute beyond the generated username and authentication credential).
- **FR-003**: System MUST accept a user-chosen secure password at registration as the sole credential; no recovery mechanism for forgotten usernames or passwords is provided.
- **FR-004**: Auto-generated usernames MUST NOT contain profane words in any of their three word components, verified against a profanity filter at generation time.
- **FR-005**: Users MUST be able to request a username change at any time; the new username MUST also be auto-generated, unique, and profanity-free and takes effect immediately upon request.
- **FR-006**: System MUST enforce access controls on all actions: poll and survey creation is permitted for all authenticated users; answering is restricted to users who are explicitly invited or when the poll or survey is marked public.

**Poll Management**

- **FR-007**: An authenticated user MUST be able to create a new poll session and designate it as public (visible in all eligible users' feeds) or invite-only.
- **FR-008**: Every poll MUST be accessible via a unique URL derived from its system-assigned identifier.
- **FR-009**: During an active poll session, the creator MUST be able to push new questions with multiple-choice answer options in real time, adding questions at any pace.
- **FR-010**: All eligible participants currently connected to a poll session MUST receive each new question in real time without a page reload.
- **FR-011**: Each participant MUST be able to answer each poll question only once per session; duplicate submissions MUST be rejected.
- **FR-012**: The poll creator MUST see a live vote-count dashboard per question, updating in real time as participant answers arrive.
- **FR-013**: All poll data — questions, answer options, and individual submissions — MUST be persisted in structured relational records.

**Survey Management**

- **FR-014**: An authenticated user MUST be able to create a survey with an arbitrary number of questions before publishing.
- **FR-015**: Each survey question MUST support the following answer types: multiple-choice (single selection), single-word text, multi-word text, and document upload (PDF files only).
- **FR-016**: Survey creators MUST be able to define conditional follow-up questions: a child question is displayed to the respondent only when their answer to a parent question matches a defined condition.
- **FR-017**: Every survey MUST be accessible via a unique URL derived from its system-assigned identifier.
- **FR-018**: Survey structure — all questions, their answer types, conditional linkages, and nested relationships — MUST be persisted in a flexible document-style storage model within the relational database to accommodate arbitrary nesting depth.
- **FR-019**: PDF file uploads within surveys MUST be validated at submission time; all non-PDF files MUST be rejected with a clear error message.
- **FR-020**: Each eligible respondent MUST be able to submit a survey only once; repeat submissions MUST be rejected.

**Access Control & Invitations**

- **FR-021**: A poll or survey marked as invite-only MUST NOT be viewable or answerable by users who have not been explicitly invited by the creator.
- **FR-022**: A poll or survey marked as public MUST appear in the home feed for all authenticated users who have public content discovery enabled in their profile settings.
- **FR-023**: Creators MUST be able to invite users by their exact username; the system MUST validate that the target username belongs to an existing user before recording the invitation.
- **FR-024**: Invited users MUST receive an in-app notification (visible via the notification bell) when they are invited to a poll or survey.
- **FR-025**: If the targeted user has disabled all invitations in their profile settings, the invitation MUST NOT be delivered, and the creator MUST receive a message indicating the user is not accepting invitations.

**Home Feed & Results**

- **FR-026**: The home page MUST present at least three tabs: a Polls feed, a Surveys feed, and a Results / My Content view.
- **FR-027**: The Polls and Surveys feeds MUST display all public and personally-invited items available to the current user, respecting the user's public discovery setting.
- **FR-028**: The Results tab MUST show all polls and surveys created by the current user along with their aggregated response data.

**Notifications**

- **FR-029**: A notification bell icon MUST be persistently visible in the application toolbar for all authenticated users, showing an unread count badge when new invitations are pending.
- **FR-030**: Opening the notification panel MUST mark the displayed invitations as read, reducing the unread count accordingly.

**Profile & Privacy**

- **FR-031**: The user profile MUST be accessible from the application toolbar via the user's avatar or initials.
- **FR-032**: Users MUST be able to enable or disable whether their public polls and surveys appear in other users' feeds.
- **FR-033**: Users MUST be able to configure invitation permissions with three options: accept invitations from all users, accept from nobody, or accept from a defined allowlist of specific usernames.
- **FR-034**: When adding a username to the invite allowlist, the system MUST validate that the entered username corresponds to an existing user before accepting it.

### Key Entities

- **User**: A registered account identified solely by a system-generated username and an authentication credential. Carries no PII attributes.
- **Poll**: A real-time question session with configurable visibility (public or invite-only); belongs to one creator; contains an ordered sequence of questions.
- **Poll Question**: A single multiple-choice question within a poll; has an ordered list of answer options; accumulates submission counts per option in real time.
- **Poll Submission**: A single participant's answer to a single poll question; uniqueness-constrained per user per question.
- **Survey**: A structured questionnaire with configurable visibility; belongs to one creator; contains a tree of questions with optional conditional branching.
- **Survey Question**: A node in the survey question tree; carries an answer type (multiple-choice, short text, long text, document upload) and optional conditional linkage to a parent question's answer.
- **Survey Response**: A completed survey submission from a single respondent; contains all answered questions and any uploaded PDF documents; uniqueness-constrained per user per survey.
- **Invitation**: A directed relationship from a creator to a specific user for a given poll or survey; triggers notification delivery.
- **Notification**: An in-app alert delivered to the target user upon invitation; carries read/unread state and a link to the relevant poll or survey.
- **UserPrivacySettings**: Per-user configuration for public content visibility in feeds and invitation permission rules (everyone / nobody / allowlist).

## Success Criteria *(mandatory)*

### Measurable Outcomes

- **SC-001**: A new user can complete account registration in under 60 seconds from first visiting the sign-up screen, with no PII collected at any point.
- **SC-002**: Live poll question updates reach all connected participants within 1 second of the creator publishing the question.
- **SC-003**: Live poll vote-count dashboard updates are reflected to the creator within 1 second of a participant submitting an answer.
- **SC-004**: All invited or eligible users can navigate to a poll or survey via its unique URL and begin answering without any additional setup steps.
- **SC-005**: The home feed loads and displays the user's available polls and surveys within 2 seconds on first visit after login.
- **SC-006**: A creator can build and publish a survey with at least five conditional-branching questions within 5 minutes.
- **SC-007**: Invitation notifications are delivered to the target user's notification bell within 5 seconds of the creator sending the invitation.
- **SC-008**: Users can configure all profile privacy settings (public feed discovery toggle, invite permission mode, allowlist management) within a single profile screen without navigating elsewhere.
- **SC-009**: Access control is correctly enforced — no uninvited, non-public poll or survey content is accessible to unauthorized users via any entry point (direct URL, feed, or underlying data access).
- **SC-010**: No personally identifiable information (email, name, phone number, or any similar attribute) is stored, displayed, or transmitted for any user at any point in the application lifecycle.

## Assumptions

- A "public" poll or survey is visible only to authenticated users; unauthenticated (anonymous) browsing or answering of content is not supported.
- Nested survey questions represent conditional/branching logic: a child question appears only when the respondent's answer to a parent question satisfies a creator-defined condition. Pure hierarchical grouping without conditional logic is considered the same mechanism with an always-true condition.
- A poll session has no automatic time limit; the creator controls the pace by pushing questions one at a time. There is no auto-advance or timer-based question progression.
- A participant who disconnects from a live poll and reconnects will see the current active question; previously submitted answers are preserved and cannot be re-answered.
- PDF upload size is limited to 10 MB per file as a reasonable default; this constraint may be adjusted by project configuration.
- When a user on another user's invite allowlist changes their username, the allowlist entry using the old username becomes invalid and is silently removed; the allowlist owner must re-add them using the new username.
- Survey respondents who abandon a survey mid-completion can return and resume from where they left off within a reasonable session window; uncompleted surveys are not counted as final submissions.
- Participants who were not connected when a poll question was first pushed cannot retroactively answer that question once a new question has been pushed; only the currently active question is answerable.
- The application is accessible to authenticated users only; there is no concept of guest participation or anonymous access.
- Username auto-generation at registration and upon change requests uses an in-application curated word list; the profanity filter covers common English-language profane words as a minimum standard.
