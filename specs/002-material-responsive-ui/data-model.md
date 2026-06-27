# Data Model: Consistent, Fully Responsive, Maximum Accessibility UI

This feature introduces no new database entities. The "model" for this feature is the **design token taxonomy**, the **breakpoint system**, and the **layout state model** that govern all visual and structural decisions across every screen.

---

## 1. Breakpoint System

### `Breakpoint` Enum

The single source of truth for viewport classification. Defined in `BreakpointService` and surfaced via `LayoutStore`.

```typescript
export enum Breakpoint {
  XSmall = 'XSmall',   // 320 px – 599 px   (phone portrait)
  Small  = 'Small',    // 600 px – 959 px   (phone landscape / small tablet)
  Medium = 'Medium',   // 960 px – 1279 px  (tablet)
  Large  = 'Large',    // 1280 px – 1919 px (desktop)
  XLarge = 'XLarge',  // 1920 px +          (large desktop)
}
```

**CDK `Breakpoints` mapping**:

| Enum Value | CDK Constant | `@media` Query |
|---|---|---|
| `XSmall` | `Breakpoints.XSmall` | `(max-width: 599.98px)` |
| `Small` | `Breakpoints.Small` | `(min-width: 600px) and (max-width: 959.98px)` |
| `Medium` | `Breakpoints.Medium` | `(min-width: 960px) and (max-width: 1279.98px)` |
| `Large` | `Breakpoints.Large` | `(min-width: 1280px) and (max-width: 1919.98px)` |
| `XLarge` | `Breakpoints.XLarge` | `(min-width: 1920px)` |

### `LayoutStore` State Shape

Managed by NgRX Signal Store in `core/layout/layout.store.ts`.

```typescript
interface LayoutState {
  breakpoint: Breakpoint;  // current viewport class; default XSmall
}

// Computed signals exposed by the store:
// isXsOrSm: Signal<boolean>  — bottom nav visible
// isMd: Signal<boolean>      — nav rail visible
// isLgOrXl: Signal<boolean>  — nav drawer visible
// isPhone: Signal<boolean>   — same as isXsOrSm (alias for template clarity)
// isDesktop: Signal<boolean> — same as isLgOrXl (alias)
```

---

## 2. Design Token Taxonomy

All visual values must reference M3 tokens — no hardcoded primitives. The tokens below are Angular Material's M3 CSS custom properties, resolved automatically for light and dark mode.

### 2a. Colour Roles

| Token (CSS var) | Role | Usage |
|---|---|---|
| `--mat-sys-primary` | Primary brand colour | Buttons, active nav items, focus rings |
| `--mat-sys-on-primary` | Text/icon on primary | Button labels, icon on filled button |
| `--mat-sys-primary-container` | Tinted primary surface | Active nav item chip, selected state |
| `--mat-sys-on-primary-container` | Text on primary container | Label in active nav chip |
| `--mat-sys-secondary` | Secondary accent | Secondary actions, chips |
| `--mat-sys-surface` | Default surface | Card background, page background |
| `--mat-sys-on-surface` | Text on surface | Body text, icon on surface |
| `--mat-sys-surface-variant` | Lower-emphasis surface | Input fill, nav rail background |
| `--mat-sys-on-surface-variant` | Text on surface variant | Placeholder, subtitle, inactive nav label |
| `--mat-sys-error` | Error state | Error text, error border, error icon |
| `--mat-sys-on-error` | Text on error | Button label on error-filled button |
| `--mat-sys-error-container` | Error tint surface | Error banner background |
| `--mat-sys-outline` | Border / divider | Input border, card outline |
| `--mat-sys-outline-variant` | Subtle border | Divider lines, low-emphasis outlines |

**Rule**: Every `color`, `background-color`, `border-color`, `fill`, and `stroke` value in component SCSS **must** reference one of these CSS vars. Hexadecimal values are PROHIBITED.

### 2b. Typography Scale

| Token (CSS var) | M3 Role | Usage |
|---|---|---|
| `--mat-sys-display-large` | Display Large | Hero headings |
| `--mat-sys-display-medium` | Display Medium | Page title on xl |
| `--mat-sys-headline-large` | Headline Large | Feature screen h1 |
| `--mat-sys-headline-medium` | Headline Medium | Section heading h2 |
| `--mat-sys-headline-small` | Headline Small | Card heading h3 |
| `--mat-sys-title-large` | Title Large | Dialog title, toolbar title |
| `--mat-sys-title-medium` | Title Medium | List item primary text |
| `--mat-sys-title-small` | Title Small | Nav label, tag |
| `--mat-sys-body-large` | Body Large | Primary body text |
| `--mat-sys-body-medium` | Body Medium | Secondary body, form hint |
| `--mat-sys-body-small` | Body Small | Caption, metadata |
| `--mat-sys-label-large` | Label Large | Button label |
| `--mat-sys-label-medium` | Label Medium | Input label, chip text |
| `--mat-sys-label-small` | Label Small | Badge text, helper text |

**Rule**: Every `font`, `font-size`, `font-weight`, `line-height`, and `letter-spacing` in component SCSS **must** reference one of these CSS vars via `font: var(--mat-sys-body-large)` or equivalent. Hardcoded pixel sizes are PROHIBITED.

### 2c. Spacing Scale

Angular Material M3 does not expose a spacing scale as CSS vars. The approved approach is to use multiples of the base 4 px grid as SCSS variables defined in `_tokens.scss`:

```scss
// _tokens.scss  — SCSS spacing variables (not CSS custom properties)
// These are SCSS compile-time constants, not runtime tokens.
$space-1: 4px;    // base unit
$space-2: 8px;
$space-3: 12px;
$space-4: 16px;   // default gap / standard padding
$space-5: 20px;
$space-6: 24px;   // card padding
$space-8: 32px;
$space-10: 40px;
$space-12: 48px;
$space-16: 64px;
```

**Rule**: All `margin`, `padding`, and `gap` values in component SCSS **must** use `$space-N` variables. Arbitrary pixel values are PROHIBITED.

### 2d. Elevation / Shape Tokens

| Token | Usage |
|---|---|
| `--mat-sys-level0` through `--mat-sys-level5` | Card elevation (box-shadow via M3 tonal elevation) |
| `--mat-sys-corner-extra-small` through `--mat-sys-corner-extra-large` | `border-radius` on surfaces, cards, dialogs |

**Rule**: `box-shadow` and `border-radius` values must reference these tokens. Hardcoded values are PROHIBITED.

---

## 3. Navigation Layout Model

The navigation shell adapts its pattern based on the `Breakpoint` signal. This is the structural model each navigation component implements:

### `NavigationItem` (shared type)

```typescript
interface NavigationItem {
  readonly label: string;          // display label (also aria-label for icon-only rail)
  readonly icon: string;           // Material icon name
  readonly route: string;          // router link target
  readonly ariaLabel?: string;     // override if label is ambiguous
}
```

**Navigation items** (ordered by tab sequence):
1. Feed — `home` icon — `/feed`
2. Polls — `poll` icon — `/polls`
3. Surveys — `assignment` icon — `/surveys`
4. Profile — `account_circle` icon — `/profile`

### Layout State per Navigation Pattern

| Breakpoint | Pattern | Nav width | Content margin-left | Max content width |
|---|---|---|---|---|
| XSmall / Small | Bottom nav bar | n/a (bottom) | 0 | 100% |
| Medium | Nav rail | 80 px | 80 px | 100% |
| Large | Nav drawer (persistent) | 256 px | 256 px | 100% |
| XLarge | Nav drawer (persistent) | 280 px | 280 px | 1440 px (centred) |

---

## 4. Screen Layout Model

Each screen has a defined layout behaviour at each breakpoint. This table governs implementation decisions.

| Screen | XSmall / Small | Medium | Large / XLarge |
|---|---|---|---|
| Login / Register | Full-width centred card, single column | 480 px centred card | 480 px centred card |
| Feed (poll/survey list) | 1-column stacked cards | 2-column grid | 3-column grid |
| Create Poll | Full-width form, stacked sections | Two-column form layout | Two-column form, max 800 px |
| Poll Session (voting) | Stacked: options above, live counts below | Side-by-side: options left, counts right | Side-by-side, max 1000 px |
| Survey Builder | Full-width, accordion sections | Split: question list left, editor right | Split with wider editor |
| Survey Respondent | Full-width, single-column questions | Single-column, 640 px centred | Single-column, 720 px centred |
| Survey Results | Stacked: chart then metadata | Chart left, metadata right | Chart left, metadata right, max 1200 px |
| Profile | Single-column | Single-column, 640 px centred | Two-column: avatar+name left, details right |

---

## 5. Accessibility Model

### Focus Indicator Contract

| State | Visual | Minimum contrast |
|---|---|---|
| Default focus (`:focus-visible`) | 2 px solid `--mat-sys-primary`, 2 px offset | 3:1 against adjacent background |
| Focus within dark mode | Same token — resolved automatically | 3:1 (verified by M3 palette) |

Angular Material 17+ applies M3's focus indicator styles automatically. Custom components must add `&:focus-visible { outline: 2px solid var(--mat-sys-primary); outline-offset: 2px; }`.

### Live Region Model

| Region | `aria-live` | `aria-atomic` | Content |
|---|---|---|---|
| Vote count per option | `polite` | `true` | `"{N} votes"` |
| Poll status (open / closed) | `assertive` | `true` | `"Poll is now closed"` |
| Form submission success | `polite` | `true` | `"Vote submitted successfully"` |
| Form validation errors | (not live — use `aria-describedby` on field) | n/a | Error message string |

### Heading Hierarchy Contract

Each screen must implement the following heading hierarchy (no skipped levels):

```
h1: Page title (unique per route, matches <title> element)
  h2: Section heading
    h3: Subsection / card heading
```

The `<title>` element must be updated on every route change via Angular's `Title` service.
