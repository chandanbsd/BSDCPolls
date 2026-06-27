# Quickstart Validation Guide: Consistent, Fully Responsive, Maximum Accessibility UI

This guide covers how to validate that the feature is working correctly. It is a manual verification checklist — no automated test suite exists (Principle XV). Run through every section before marking the feature complete.

---

## Prerequisites

1. Local Aspire stack running: `dotnet run --project BSDCPolls.AppHost`
2. Angular dev server running: `cd src/bsdcpolls-frontend && ng serve`
3. App accessible at `http://localhost:4200`
4. Browser DevTools available (Chrome recommended for axe DevTools extension)
5. [axe DevTools browser extension](https://www.deque.com/axe/devtools/) installed

---

## Setup Commands

```bash
# Start the backend (from repo root)
dotnet run --project BSDCPolls.AppHost

# Start the Angular dev server (from repo root)
cd src/bsdcpolls-frontend && ng serve

# Run linting (must pass before validation)
ng lint --max-warnings 0
prettier --check .
```

---

## SC-001: WCAG 2.1 AA — Zero Automated Violations

**For each of the 5 breakpoints and each screen**, run axe DevTools:

1. Open Chrome DevTools → axe DevTools panel
2. Resize browser to the target viewport width using DevTools responsive mode
3. Navigate to the screen
4. Click "Analyse" in axe DevTools
5. **Expected**: Zero violations (Critical, Serious, Moderate, Minor all show 0)

**Breakpoint widths to test**: 320, 600, 960, 1280, 1920 px

**Screens to test**: Login, Register, Feed, Create Poll, Poll Session (with live data), Survey Builder, Survey Respondent, Survey Results, Profile

---

## SC-002: Keyboard-Only Navigation (under 5 minutes)

**Setup**: Disconnect or ignore your mouse. Use only Tab, Shift+Tab, Enter, Space, and Arrow keys.

### Login flow
1. Load `http://localhost:4200` → redirects to `/login`
2. Press Tab once → **verify** skip link appears (high-contrast overlay at top of page)
3. Press Enter on skip link → **verify** focus jumps past navigation to main content
4. Tab to "Email address" field → type test email → Tab to "Password" field → type password → Tab to "Sign In" button → press Enter
5. **Expected**: Logged in, redirected to Feed, page title announced by screen reader (if using one)

### Vote casting flow
1. From Feed, Tab to a poll card → press Enter → navigate to Poll Session
2. Tab through vote options → press Enter or Space on one → Tab to "Submit" → Enter
3. **Expected**: Vote submitted confirmation visible/announced

### Survey flow
1. Navigate to a survey via keyboard → complete all questions → submit
2. **Expected**: Completion confirmation visible

**Pass criteria**: Entire login → vote → result journey completes in under 5 minutes with no mouse.

---

## SC-003: Screen Reader Journey

**Setup**: Enable VoiceOver (macOS: `Cmd + F5`) or NVDA (Windows).

### Full poll participation journey
1. Load the app → **verify**: page title "Sign In — BSDCPolls" announced
2. Navigate to login fields → enter credentials → submit → **verify**: "Feed — BSDCPolls" announced after redirect
3. Navigate to a poll card → **verify**: poll title and status announced (Active / Closed icon + text)
4. Open poll → **verify**: all vote options announced with their current vote counts and selection state
5. Select an option → **verify**: selection state ("selected") announced immediately
6. Submit vote → **verify**: "Vote submitted successfully" announced via live region
7. Observe live results updating → **verify**: updated vote counts announced without focus movement
8. If poll closes during session → **verify**: "This poll is now closed" announced assertively

**Pass criteria**: All state changes announced correctly; no unlabelled interactive elements encountered; no inaccessible dialogs.

---

## SC-004: 200% Text Zoom

1. Load app in Chrome at 100% zoom
2. Press `Ctrl/Cmd +` four times to reach 200% zoom
3. Navigate through: Feed, Poll Session, Profile
4. **Expected**: All content visible without horizontal scrollbar; all interactive elements reachable; text readable (not clipped or overflowing)

---

## SC-005: Zero Hardcoded Values in SCSS

Run this command from the Angular project root:

```bash
# Search for hardcoded hex colours in SCSS (should return no matches in src/)
grep -rn '#[0-9a-fA-F]\{3,6\}' src/app/**/*.scss src/styles.scss src/styles/

# Search for hardcoded px font-sizes (should return no matches)
grep -rn 'font-size:\s*[0-9]' src/app/**/*.scss src/styles/

# Search for hardcoded font-weight (should return no matches)  
grep -rn 'font-weight:\s*[0-9]' src/app/**/*.scss src/styles/

# Search for hardcoded border-radius (should return no matches)
grep -rn 'border-radius:\s*[0-9]' src/app/**/*.scss src/styles/

# Search for prohibited ::ng-deep
grep -rn '::ng-deep' src/
```

**Expected**: All greps return empty output (no matches in application source files).

---

## SC-006: Manual Breakpoint Verification (all 5 × all screens)

Use Chrome DevTools responsive mode. For each breakpoint width, verify the following for every screen:

| Breakpoint | Width | Nav pattern | Expected layout |
|---|---|---|---|
| xs | 320 px | Bottom nav bar | See [navigation-layout.md](contracts/navigation-layout.md) |
| sm | 600 px | Bottom nav bar | Same as xs |
| md | 960 px | Nav rail (80 px left) | See navigation-layout.md |
| lg | 1280 px | Nav drawer (256 px left) | See navigation-layout.md |
| xl | 1920 px | Nav drawer (280 px left, content centred 1440 px max) | See navigation-layout.md |

**Per screen checklist** (repeat for all 9 screens):
- [ ] No horizontal scrollbar
- [ ] No clipped text or overflowing elements
- [ ] Navigation pattern matches breakpoint spec
- [ ] All interactive elements visible and reachable
- [ ] Heading hierarchy correct (h1 → h2 → h3, no skips)
- [ ] Layout matches the Screen Layout Model in [data-model.md](data-model.md)

---

## SC-007: Dark Mode Contrast

1. Set OS to Dark Mode (macOS: System Settings → Appearance → Dark)
2. Reload app
3. For each screen, run axe DevTools → **verify**: zero colour contrast violations
4. Visually inspect: no light-coloured text on light backgrounds; no dark text on dark backgrounds

**Key risk areas to check manually**:
- Poll results chart colours (custom palette may not inherit dark mode automatically)
- Status badges (Active / Closed)
- Any custom coloured elements outside Angular Material components

---

## SC-008: Reduced Motion

1. Enable "Reduce Motion" in OS settings (macOS: System Settings → Accessibility → Display → Reduce Motion)
2. Reload app
3. Navigate between pages, open/close dialogs, submit forms
4. **Expected**: No slide animations, no fade transitions, no expanding animations. Page transitions are instant (or a single cross-fade at most).

---

## SC-009: Touch Target Sizes (44 × 44 px)

Using Chrome DevTools at 320 px (xs viewport):

1. Inspect each interactive element with DevTools
2. Check computed width and height (or use DevTools' element size indicator)
3. **Verify**: width ≥ 44 px AND height ≥ 44 px for:
   - All bottom nav bar items
   - All buttons
   - All form controls (radio buttons, checkboxes, selects)
   - All links
   - Notification bell icon button
   - Profile avatar icon button

---

## SC-010: Screen Reader — State Announcements

With VoiceOver or NVDA active:

### Live vote count updates
1. Open a poll session with another browser tab also connected
2. Cast a vote from the second tab
3. **In the first tab**: verify vote count announces without user moving focus

### Error announcements
1. Submit the login form with an empty email field
2. **Verify**: "Enter a valid email address" (or equivalent) is announced; screen reader associates it with the email field

### Poll status change
1. Have a poll session open; close the poll from another tab / admin action
2. **Verify**: "This poll is now closed" is announced assertively (interrupts current reading)

---

## Linting Sign-Off Checklist

Run these before marking the feature complete:

```bash
# TypeScript / Angular ESLint
ng lint --max-warnings 0

# Prettier check
prettier --check "src/**/*.{ts,html,scss,json}"

# TypeScript compilation
ng build --configuration production
```

All must exit with code 0.

---

## Navigation Contract Reference

See [contracts/navigation-layout.md](contracts/navigation-layout.md) for the full structural specification of each navigation component.

## Design Token Reference

See [contracts/design-tokens.md](contracts/design-tokens.md) for the complete authorised token catalogue.

## Accessibility Contract Reference

See [contracts/accessibility.md](contracts/accessibility.md) for ARIA specifications, live region markup, focus management, and contrast requirements.
