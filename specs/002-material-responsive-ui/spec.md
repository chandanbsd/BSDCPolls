# Feature Specification: Consistent, Fully Responsive, Maximum Accessibility UI

**Feature Branch**: `002-material-responsive-ui`

**Created**: 2026-06-14

**Status**: Draft

**Input**: User description: "Implement a consistent, fully responsive, maximum accessibility, Angular Material and Sass user interface. Everything needs to be extensively responsive no exceptions."

## User Scenarios & Testing *(mandatory)*

### User Story 1 - Access any screen on a phone (Priority: P1)

A user opens BSDCPolls on a budget smartphone in portrait orientation. Every screen — home, poll list, poll detail, voting interface, results — must be fully usable with no horizontal scrolling, no clipped content, and no touch targets smaller than the minimum recommended size. Voting and navigation must be reachable with one thumb.

**Why this priority**: The majority of public polling traffic arrives on mobile devices. A broken mobile experience means the product is inaccessible to its primary audience.

**Independent Test**: Can be fully tested by loading the app on a 320 px wide viewport and navigating through the poll lifecycle (view list → open poll → cast vote → see results). All interactive elements must be reachable and functional without zooming or horizontal scrolling.

**Acceptance Scenarios**:

1. **Given** a phone viewport at 320 px width, **When** a user loads any page, **Then** all content is visible within the viewport with no horizontal overflow and no clipped text.
2. **Given** a phone viewport at 320 px width, **When** a user attempts to cast a vote, **Then** all vote option buttons are tappable with a minimum touch area, and the submit action is accessible without scrolling below the fold.
3. **Given** a phone in portrait orientation, **When** the user navigates between sections, **Then** navigation controls adapt to a mobile-appropriate layout (e.g., bottom nav bar or compact menu) that does not obscure page content.

---

### User Story 2 - Use the app with keyboard only (Priority: P1)

A user who cannot operate a mouse navigates the entire application using only a keyboard. Every interactive element — links, buttons, vote options, form fields, modal dialogs — must be reachable via Tab and operable via Enter or Space. The current focus must always be visually apparent.

**Why this priority**: Keyboard accessibility is foundational for users with motor disabilities and is required for WCAG 2.1 Level AA compliance, which is a non-negotiable project standard.

**Independent Test**: Can be fully tested by loading the app with no pointing device and pressing Tab through every interactive element on every screen. Every element must receive focus in a logical order and show a clearly visible focus ring.

**Acceptance Scenarios**:

1. **Given** a user who presses Tab from the top of any page, **When** they reach an interactive element, **Then** a high-contrast focus indicator is visible around that element.
2. **Given** a user navigating a poll's voting interface by keyboard, **When** they Tab through the vote options and press Enter on their choice, **Then** the vote is registered and a confirmation is announced without requiring mouse interaction.
3. **Given** a modal dialog is open, **When** the user presses Tab, **Then** focus is trapped within the dialog and does not escape to background content. Pressing Escape closes the dialog and returns focus to the trigger element.

---

### User Story 3 - Use the app with a screen reader (Priority: P1)

A user who is blind or has severely limited vision navigates BSDCPolls using a screen reader. Every page has a clear heading hierarchy, all interactive elements have meaningful labels, real-time vote count updates are announced automatically, and error messages are spoken when they appear.

**Why this priority**: Screen reader support is a WCAG 2.1 Level AA requirement and directly determines whether visually impaired users can participate in polls — a core product use case.

**Independent Test**: Can be fully tested by enabling VoiceOver (macOS) or NVDA (Windows) and navigating a complete poll lifecycle. The screen reader must announce all meaningful content, interactive elements, status changes, and error conditions.

**Acceptance Scenarios**:

1. **Given** a screen reader is active, **When** a user lands on any page, **Then** the page title is announced and a skip-to-main-content link is available as the first focusable element.
2. **Given** live vote results are displayed on screen, **When** vote counts update in real-time, **Then** the updated totals are announced by the screen reader without the user moving focus.
3. **Given** a form validation error occurs, **When** the user attempts to submit, **Then** the error message is announced by the screen reader and associated with the specific input field that caused it.
4. **Given** a user navigates the vote option list, **When** they move through the options, **Then** each option's label, current selection state (selected / not selected), and vote count are all announced.

---

### User Story 4 - View the app on a large desktop monitor (Priority: P2)

A user on a 1920 px or wider display experiences a layout that makes productive use of the available space. Content is not stretched to an unreadable line length; multi-column layouts reveal additional information where appropriate; typography remains proportional to the screen size.

**Why this priority**: Desktop users form a significant secondary audience (community managers, event organisers creating polls). A layout that degrades at large viewports signals a low-quality product.

**Independent Test**: Can be fully tested by loading the app at 1920 px and wider and verifying that content is arranged in a multi-column or appropriately constrained layout, and that no individual element stretches to fill the full viewport width inappropriately.

**Acceptance Scenarios**:

1. **Given** a viewport wider than 1920 px, **When** the poll list page loads, **Then** polls are displayed in a multi-column grid and readable line lengths are preserved (max ~80 characters per line for body text).
2. **Given** a large desktop viewport, **When** a user views live poll results, **Then** the results chart and supplementary metadata are arranged side-by-side rather than stacked vertically.

---

### User Story 5 - Switch between dark mode and light mode (Priority: P2)

A user's operating system or browser preference for dark mode is respected automatically. The entire application switches colour scheme to match the system preference. No screen has mixed light/dark content, unreadable text, or broken contrast ratios in either mode.

**Why this priority**: Dark mode is an accessibility requirement for users with photosensitivity and a standard UX expectation. Colour contrast must meet WCAG AA in both modes.

**Independent Test**: Can be fully tested by toggling the OS dark mode setting and reloading the app. Every screen must display a fully dark (or fully light) colour scheme with no hardcoded colours that break in the alternate mode.

**Acceptance Scenarios**:

1. **Given** the user's OS is set to dark mode, **When** the app loads, **Then** all backgrounds, text, icons, and interactive elements render in the dark theme with no hardcoded light colours appearing.
2. **Given** the user switches from dark to light mode at OS level, **When** the change is applied, **Then** the app updates immediately (or on next interaction) without requiring a manual reload.
3. **Given** either colour mode is active, **When** any text is displayed, **Then** the contrast ratio meets or exceeds 4.5:1 for body text and 3:1 for large text and UI components.

---

### User Story 6 - View and interact on a tablet (Priority: P2)

A user on a tablet in landscape or portrait orientation gets a layout that is neither a stretched phone layout nor a cramped desktop layout. Navigation is optimised for touch (larger tap targets, gesture-friendly controls) while making use of the additional screen real estate.

**Why this priority**: Tablets represent a distinct breakpoint where both phone and desktop layouts fail. A proper intermediate layout is required for a polished cross-device experience.

**Independent Test**: Can be fully tested by loading the app at 768–1024 px wide and verifying that the layout uses available space appropriately and all touch targets are adequately sized.

**Acceptance Scenarios**:

1. **Given** a tablet viewport at 768 px width, **When** the poll list loads, **Then** polls are displayed in a two-column grid rather than a single-column phone layout.
2. **Given** a tablet in portrait orientation, **When** the user navigates, **Then** navigation appears as a side rail or persistent nav panel rather than a collapsed mobile menu.

---

### User Story 7 - Use the app when colour is removed (Priority: P3)

A user with colour-blindness or who prints the page in greyscale can still distinguish all poll options, status indicators, results charts, and error states. No information is conveyed by colour alone.

**Why this priority**: Colour-alone communication is explicitly prohibited by WCAG 1.4.1 (Level A). All status must also be conveyed via shape, label, pattern, or text.

**Independent Test**: Can be tested by applying a greyscale CSS filter to the entire app and verifying that all meaningful distinctions are still communicated by means other than colour.

**Acceptance Scenarios**:

1. **Given** the app is viewed in greyscale, **When** a user views a poll results chart, **Then** each result bar or segment is distinguished by a label or pattern — not by colour alone.
2. **Given** the app is viewed in greyscale, **When** a form field has an error, **Then** the error is indicated by an icon or text label in addition to any colour change on the border.

---

### Edge Cases

- What happens when a user's browser does not support certain CSS features (e.g., CSS Grid in very old browsers)? The layout must still be functional with graceful degradation.
- How does the responsive layout behave when the user zooms the browser to 200% or higher (WCAG 1.4.4 Resize Text)? All content must remain accessible and no horizontal scrolling should appear.
- What happens on a foldable device or split-screen mode where the available viewport changes dynamically? Layouts must adapt without requiring a page reload.
- How does the system behave when an RTL (right-to-left) locale is applied? Layout must mirror correctly.
- What happens when a user has the "prefers-reduced-motion" system preference set? All animations and transitions must be suppressed or minimised to prevent vestibular disruption.
- What happens on ultra-wide monitors (2560 px+) where content would stretch to an unreadable line length? A maximum content width constraint must be applied.

## Requirements *(mandatory)*

### Functional Requirements

- **FR-001**: Every screen MUST render correctly and be fully usable at all five standard breakpoints: 320 px (xs), 600 px (sm), 960 px (md), 1280 px (lg), and 1920 px (xl) widths.
- **FR-002**: All interactive elements MUST be fully operable using keyboard alone, with no mouse required for any user-facing action.
- **FR-003**: A clearly visible focus indicator MUST appear on every interactive element when it receives keyboard focus. The focus indicator MUST have a minimum contrast ratio of 3:1 against adjacent colours.
- **FR-004**: All non-decorative images and icons MUST have a descriptive text alternative accessible to screen readers.
- **FR-005**: Every form field MUST have a visible label that remains visible when the field is focused or contains a value; placeholder text alone is NOT an acceptable label.
- **FR-006**: Form validation error messages MUST be programmatically associated with the relevant field and announced automatically by screen readers when they appear.
- **FR-007**: All text content MUST meet WCAG 2.1 Level AA colour contrast ratios: 4.5:1 for normal text, 3:1 for large text (18 pt / 14 pt bold) and UI component boundaries.
- **FR-008**: Colour MUST NOT be used as the sole means to convey information, indicate status, or prompt user action. Every colour distinction MUST be accompanied by a non-colour indicator (text, icon, pattern, or shape).
- **FR-009**: Real-time vote count updates and poll status changes MUST be announced to screen readers via live regions without requiring the user to move focus.
- **FR-010**: The application MUST respect the user's operating-system dark mode preference and render a fully coherent dark theme with correct contrast ratios.
- **FR-011**: All animations and transitions MUST be suppressed or reduced to a single cross-fade when the user has set the "prefers-reduced-motion" system preference.
- **FR-012**: Focus MUST be trapped within modal dialogs while they are open and returned to the triggering element when closed.
- **FR-013**: A "skip to main content" link MUST be the first focusable element on every page, allowing keyboard users to bypass repeated navigation.
- **FR-014**: Touch targets (buttons, links, form controls) MUST have a minimum interactive area of 44 × 44 CSS pixels on all touch-enabled viewports.
- **FR-015**: Page content MUST remain accessible and avoid horizontal overflow when the browser's text size is set to 200% of the default.
- **FR-016**: The heading hierarchy on every page MUST be logical and sequential (h1 → h2 → h3) with no skipped levels.
- **FR-017**: All pages MUST have a unique, descriptive `<title>` element that is announced by screen readers on navigation.
- **FR-018**: The UI colour scheme MUST use only semantic colour tokens from the design system's M3 palette — no hardcoded hex or RGB colour values are permitted in any stylesheet.
- **FR-019**: Typography MUST use semantic scale tokens exclusively; no hardcoded font-size or font-weight values are permitted.
- **FR-020**: Layout spacing (margin, padding, gap) MUST use design-system spacing tokens; no hardcoded pixel values for spacing are permitted.
- **FR-021**: Every page layout MUST be validated at all five breakpoints before the feature is considered complete.
- **FR-022**: All interactive controls that are disabled MUST communicate their disabled state to screen readers via appropriate ARIA attributes.
- **FR-023**: Error states, loading states, and empty states MUST be visually and programmatically communicated on every screen that can enter those states.
- **FR-024**: The navigation structure MUST adapt at each breakpoint: collapsing to a compact mobile menu at xs/sm, presenting a side rail at md, and a full navigation panel at lg/xl.

### Key Entities

- **Screen / Route**: A distinct addressable view within the application (e.g., Home, Poll List, Poll Detail, Voting Interface, Results). Each screen has independent responsive layout requirements.
- **Breakpoint**: A named viewport-width threshold (xs, sm, md, lg, xl) at which layouts must be verified as functional and correct.
- **Colour Token**: A semantic reference to a colour role in the design system (e.g., `primary`, `surface`, `on-primary`). Tokens are resolved at runtime for both light and dark modes.
- **Typography Token**: A semantic reference to a type style in the design system scale (e.g., `headline-medium`, `body-large`). Tokens define size, weight, and line-height together.
- **Touch Target**: The interactive area of a control on touch devices; must meet minimum dimensions regardless of the control's visual size.
- **Live Region**: A designated area of the page where dynamic content changes are automatically announced by screen readers without focus movement.
- **Focus Indicator**: The visual outline or highlight that marks the currently keyboard-focused interactive element.

## Success Criteria *(mandatory)*

### Measurable Outcomes

- **SC-001**: Every page achieves a WCAG 2.1 Level AA score of zero automated violations in an accessibility audit tool across all five named breakpoints.
- **SC-002**: All interactive flows (login, browse polls, cast vote, view results) can be completed in under 5 minutes by a first-time user using only a keyboard, with no reliance on a pointing device.
- **SC-003**: A screen reader user can complete the full poll participation journey (land on page → find a poll → cast a vote → confirm the result) without encountering any unlabelled controls, missing announcements, or inaccessible dialogs.
- **SC-004**: All page content is fully readable and functional at browser text-size settings from 100% to 200% with no horizontal scrolling.
- **SC-005**: Zero hardcoded colour, font-size, or spacing values appear in any stylesheet; all values reference design system tokens, verified by automated linting.
- **SC-006**: All five breakpoints (xs → xl) are manually verified for each screen before the feature is marked complete; no screen has layout overflow, clipped content, or inaccessible controls at any breakpoint.
- **SC-007**: The dark mode colour scheme passes WCAG AA contrast checks on all screens, independently of the light mode validation.
- **SC-008**: All animations are suppressed when the user's "prefers-reduced-motion" preference is active, verified by toggling the OS setting and checking that no motion occurs.
- **SC-009**: All touch targets on interactive elements measure at or above 44 × 44 CSS pixels on xs and sm viewports.
- **SC-010**: Screen reader manual testing with VoiceOver or NVDA completes the poll participation journey with all state changes (loading, errors, vote confirmation, live count updates) announced correctly.

## Assumptions

- All screens referenced in this spec are the screens defined in the current polls/surveys platform feature (spec `001-polls-surveys-platform`). This UI spec governs the visual and accessibility layer for those existing screens.
- Users access the application from a wide range of devices including budget Android phones (320 px wide, low-pixel-density screens), iOS devices, tablets, and desktop browsers.
- The application supports the two most recent versions of Chrome, Safari, Firefox, and Edge. No legacy browser support (e.g., IE11) is required.
- RTL language support is a desirable goal but is not in scope for this feature; layouts should not actively prevent future RTL support (i.e., avoid absolute left/right positioning where logical properties can be used).
- The design system's colour token set, typography scale, and spacing scale are already defined (or will be co-created with this feature) and govern all visual decisions.
- Automated accessibility testing tools (e.g., axe-core via browser extension or CI integration) are available for initial triage; manual screen reader testing is required for final sign-off.
- The "prefers-reduced-motion" media query is the sole mechanism for motion suppression; no in-app motion preference toggle is required for this feature.
- Content is authored in English for this feature; internationalisation (i18n) of text strings is not in scope.
