# Contract: Accessibility

This contract defines the mandatory ARIA, keyboard, focus management, and screen reader specifications for every screen in BSDCPolls. Compliance with this contract satisfies FR-002 through FR-017 and SC-001 through SC-010 in the feature spec.

---

## 1. Global Shell Requirements

### 1a. Skip Link

**Location**: First child element in `AppComponent` template.

**Markup**:
```html
<a href="#main-content" class="skip-link">Skip to main content</a>
```

**CSS class** `.skip-link` defined in global `styles.scss` (see [design-tokens.md](design-tokens.md)).

**Behaviour**:
- Visually hidden at `position: absolute; top: -40px`
- Revealed on `:focus-visible` at `top: 0` (or `$space-2`)
- Must be keyboard-operable via Enter
- Must move focus to `<main id="main-content">`

**Verification**: Tab once from a fresh page load → skip link appears → press Enter → focus lands in main content area → Tab again skips the navigation.

### 1b. Page Title Updates

Every route change must update `document.title` via Angular `Title` service:

| Route | Title |
|---|---|
| `/feed` | `Feed — BSDCPolls` |
| `/polls` | `Polls — BSDCPolls` |
| `/polls/create` | `Create Poll — BSDCPolls` |
| `/polls/:id` | `[Poll Name] — BSDCPolls` |
| `/surveys` | `Surveys — BSDCPolls` |
| `/surveys/builder` | `Survey Builder — BSDCPolls` |
| `/surveys/:id` | `[Survey Name] — BSDCPolls` |
| `/surveys/:id/results` | `[Survey Name] Results — BSDCPolls` |
| `/profile` | `Profile — BSDCPolls` |
| `/login` | `Sign In — BSDCPolls` |
| `/register` | `Create Account — BSDCPolls` |

The `Title` service call must happen in the component's `ngOnInit` or via a `ResolveFn` / route `title` property (Angular 15+ route title feature preferred).

### 1c. Focus Management on Route Change

On every `NavigationEnd` event in `AppComponent`:
1. `document.getElementById('main-content')?.focus()`
2. This announces the new page `<h1>` to screen reader users navigating by virtual cursor

---

## 2. Heading Hierarchy

Every route component must implement `<h1>` through `<h3>` with no skipped levels.

### Required heading structure per screen

**Feed**:
```html
<h1>Feed</h1>
  <h2>Active Polls</h2>       <!-- section within feed -->
  <h2>Active Surveys</h2>
    <h3>[Poll / Survey title]</h3>  <!-- per card -->
```

**Poll Session**:
```html
<h1>[Poll Name]</h1>
  <h2>Vote Options</h2>
  <h2>Live Results</h2>
    <h3>[Option name]</h3>  <!-- per result row -->
```

**Survey Builder**:
```html
<h1>Survey Builder</h1>
  <h2>Questions</h2>
    <h3>[Question N title]</h3>
  <h2>Preview</h2>
```

**Auth (Login / Register)**: Single `<h1>` only — `<h1>Sign In</h1>` / `<h1>Create Account</h1>`.

---

## 3. Form Accessibility

### 3a. Label Association

Every `mat-form-field` must wrap a `mat-label`. Placeholder text alone is PROHIBITED.

```html
<!-- CORRECT -->
<mat-form-field>
  <mat-label>Email address</mat-label>
  <input matInput type="email" formControlName="email" autocomplete="email" />
  <mat-error>Enter a valid email address</mat-error>
</mat-form-field>

<!-- PROHIBITED — placeholder only -->
<input matInput placeholder="Email address" />
```

`MatFormField` with `mat-label` automatically wires `<label for>` and `aria-labelledby`. `<mat-error>` is automatically announced via `aria-live="polite"` by Angular Material.

### 3b. Error State Announcement

`<mat-error>` is displayed and announced when the control is `touched && invalid`. No additional `aria-describedby` wiring is needed for `mat-form-field` errors — Angular Material handles this.

For custom error displays outside `mat-form-field`:
```html
<div role="alert" aria-live="assertive">
  {{ errorMessage }}
</div>
```

### 3c. Required Fields

```html
<mat-form-field>
  <mat-label>Poll Title <span aria-hidden="true">*</span></mat-label>
  <input matInput required formControlName="title" aria-required="true" />
  <mat-hint>Required</mat-hint>
</mat-form-field>
```

The `*` is marked `aria-hidden="true"` because `aria-required="true"` conveys the required state to screen readers without needing the asterisk character.

### 3d. Disabled Controls

All disabled form controls must have `disabled` attribute (or `[disabled]` binding). Angular Material sets `aria-disabled="true"` automatically on Material components. Verify disabled state is conveyed for any custom controls.

---

## 4. Interactive Elements

### 4a. Keyboard Operability

All interactive elements must be reachable via Tab and operable via Enter or Space.

| Element type | Expected keyboard behaviour |
|---|---|
| `mat-button` / `button` | Enter or Space to activate |
| `mat-icon-button` | Enter or Space to activate; must have `aria-label` |
| `a[mat-button]` / `<a>` | Enter to follow link |
| `mat-radio-button` | Arrow keys to move between options; Enter/Space to select |
| `mat-checkbox` | Space to toggle |
| `mat-select` | Enter/Space to open; Arrow keys to navigate options; Enter to select |
| `mat-slide-toggle` | Space to toggle |
| `mat-dialog` | Escape to close; Tab/Shift+Tab within dialog only |

### 4b. Focus Indicator

The global focus indicator is managed by Angular Material's M3 theme (`:focus-visible` outline in `--mat-sys-primary`). No override needed for Material components.

For custom components and anchor tags that bypass Material theming, add explicitly in component SCSS:
```scss
&:focus-visible {
  // Overriding browser default: Material M3 focus indicator is more visible
  outline: 2px solid var(--mat-sys-primary);
  outline-offset: 2px;
  border-radius: var(--mat-sys-corner-extra-small);
}
```

### 4c. Touch Targets

Minimum 44 × 44 CSS px interactive area. Verified requirements:

| Component | Visual size | Touch area mechanism |
|---|---|---|
| `mat-icon-button` | 40 px | 48 px transparent padding (M3 default at density 0) |
| Bottom nav item | Full column width | `flex: 1; min-height: 80px` |
| `mat-radio-button` | 20 px radio + label | `min-height: 44px` on radio group item |
| `mat-checkbox` | 18 px box | M3 touch target padding applied automatically |
| Nav rail item | 80 px wide × 56 px | `min-height: 56px` on nav item |

---

## 5. Live Regions

### 5a. Vote Count Display (Poll Session Screen)

```html
<!-- Rendered once in the DOM unconditionally (before any data loads) -->
<div
  role="status"
  aria-live="polite"
  aria-atomic="true"
  aria-label="Live vote results">
  @for (option of pollOptions(); track option.uid) {
    <p>{{ option.text }}: {{ option.voteCount }} votes</p>
  }
</div>
```

**Rules**:
- The `<div role="status" aria-live="polite">` element must be in the initial DOM render — not inside an `@if`. Empty on initial render is acceptable.
- `aria-atomic="true"` ensures the full content is announced on any change (not just the changed text node).
- Poll options that update via SignalR push the new `voteCount` through the store → the component's signal updates → Angular re-renders the live region content → screen reader announces.

### 5b. Poll Status Changes

```html
<div
  role="alert"
  aria-live="assertive"
  aria-atomic="true"
  [class.visually-hidden]="!pollStatusMessage()">
  {{ pollStatusMessage() }}
</div>
```

`pollStatusMessage()` is a computed signal that returns e.g. `"This poll is now closed"` when status changes to Closed, and `null` otherwise. `aria-live="assertive"` interrupts the screen reader immediately.

### 5c. Form Submission Confirmation

```html
<div
  role="status"
  aria-live="polite"
  aria-atomic="true"
  class="visually-hidden">
  {{ submissionConfirmation() }}
</div>
```

Set `submissionConfirmation` signal to `"Vote submitted successfully"` on success, then clear after 3 seconds.

### 5d. Visually Hidden Content

```scss
// In _tokens.scss or styles.scss — utility class for screen-reader-only content
.visually-hidden {
  position: absolute;
  width: 1px;
  height: 1px;
  padding: 0;
  margin: -1px;
  overflow: hidden;
  clip: rect(0, 0, 0, 0);
  white-space: nowrap;
  border: 0;
}
```

---

## 6. Modal Dialogs (MatDialog)

All dialogs must be opened via `MatDialog.open()`. Custom overlay constructions are PROHIBITED.

### 6a. Required Dialog Config

```typescript
this.dialog.open(MyDialogComponent, {
  ariaLabel: 'Dialog title that describes the dialog purpose',
  restoreFocus: true,  // Angular Material default — restore focus to trigger on close
  autoFocus: 'first-tabbable',  // focus first interactive element inside dialog
});
```

### 6b. Dialog Template Requirements

```html
<h2 mat-dialog-title>Dialog Heading</h2>
<mat-dialog-content>
  <!-- dialog body -->
</mat-dialog-content>
<mat-dialog-actions align="end">
  <button mat-button mat-dialog-close>Cancel</button>
  <button mat-button [mat-dialog-close]="true" cdkFocusInitial>Confirm</button>
</mat-dialog-actions>
```

**Behaviour verified by Angular Material CDK**:
- Focus trapped within dialog while open
- Escape closes dialog and restores focus to trigger
- `role="dialog"` and `aria-modal="true"` applied automatically by `MatDialogContainer`

---

## 7. Images and Icons

### 7a. Non-decorative Icons

All `<mat-icon>` used as standalone information (not accompanying visible text) must have an `aria-label`:

```html
<!-- Icon with no adjacent label — aria-label required -->
<button mat-icon-button aria-label="Open notifications">
  <mat-icon aria-hidden="true">notifications</mat-icon>
</button>

<!-- Icon accompanying visible text — icon is decorative -->
<button mat-button>
  <mat-icon aria-hidden="true">add</mat-icon>
  Create Poll
</button>
```

**Rule**: When a label is present and visible, `aria-hidden="true"` on the icon prevents duplicate announcements.

### 7b. Status-Only Colour

Poll status indicators (e.g., "Active" / "Closed" / "Draft") must not rely on colour alone:

```html
<span class="status-badge">
  <mat-icon aria-hidden="true">check_circle</mat-icon>
  Active
</span>
```

Icon + text: WCAG 1.4.1 compliant (no colour-only information).

---

## 8. Colour Contrast Requirements

| Text category | Minimum contrast ratio | M3 role pairs (light) | M3 role pairs (dark) |
|---|---|---|---|
| Normal body text | 4.5:1 | `on-surface` / `surface` | `on-surface` / `surface` |
| Large text (18pt+ or 14pt bold) | 3:1 | Same roles | Same roles |
| UI component boundaries (input border) | 3:1 | `outline` / `surface` | `outline` / `surface` |
| Focus indicator | 3:1 against adjacent colour | `primary` / background | `primary` / background |
| Disabled text | Exempt (WCAG 1.4.3 exception) | n/a | n/a |

M3's azure palette is designed to satisfy WCAG AA for all of the above role pairs. Poll results chart colours are the only area requiring manual contrast verification — see [quickstart.md](../quickstart.md) for the verification checklist.

---

## 9. Colour Independence (WCAG 1.4.1)

Every status distinction must have a non-colour indicator:

| Status | Colour | Non-colour indicator |
|---|---|---|
| Poll Active | Green-tinted | `check_circle` icon + "Active" text |
| Poll Closed | Neutral | `cancel` icon + "Closed" text |
| Poll Draft | Amber-tinted | `edit` icon + "Draft" text |
| Form error | Error red | `error` icon + error text below field |
| Required field | n/a | `aria-required` + "(required)" hint or asterisk |
| Chart segment A vs B | Distinct colours | Pattern fill + legend label |

---

## 10. Browser Zoom (200%)

At 200% browser text zoom (`Ctrl/Cmd +`), all content must remain accessible without horizontal scrolling. Implementation requirements:

- Use `rem`-based spacing where fixed pixel values would cause overflow at 200% (exception: touch target minimums use px minimums). Note: with the M3 token approach, Angular Material already uses `rem` internally.
- Avoid `overflow: hidden` on the page level that would clip zoomed content.
- Test by zooming to 200% and checking that all text is readable and no horizontal scrollbar appears.

---

## 11. RTL Preparation

Do not use `left`/`right` directional CSS properties where logical properties are available. Use:

| Physical property | Replace with |
|---|---|
| `margin-left` | `margin-inline-start` |
| `margin-right` | `margin-inline-end` |
| `padding-left` | `padding-inline-start` |
| `text-align: left` | `text-align: start` |

Angular Material M3 already uses logical properties internally. SCSS overrides must follow the same convention.
