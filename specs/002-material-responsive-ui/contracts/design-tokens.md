# Contract: Design Tokens

This contract defines the complete set of design tokens authorised for use in BSDCPolls SCSS. All values in component SCSS must reference these tokens. Hardcoded primitives are PROHIBITED.

---

## Theming Architecture

### File: `src/styles.scss`

The global stylesheet is the sole location where `mat.theme()` is called. It establishes both light and dark themes.

```scss
@use '@angular/material' as mat;

// Light theme (default)
html {
  @include mat.theme((
    color: (
      primary: mat.$azure-palette,
      theme-type: light,
    ),
    typography: Roboto,
    density: 0,
  ));
}

// Dark theme — activated automatically via OS preference
@media (prefers-color-scheme: dark) {
  html {
    @include mat.theme((
      color: (
        primary: mat.$azure-palette,
        theme-type: dark,
      ),
      typography: Roboto,
      density: 0,
    ));
  }
}

// Global resets
html, body {
  height: 100%;
  margin: 0;
  font-family: Roboto, 'Helvetica Neue', sans-serif;
  background-color: var(--mat-sys-background);
  color: var(--mat-sys-on-background);
}

// Skip link
.skip-link {
  position: absolute;
  top: -40px;
  left: $space-4;
  z-index: 1000;
  padding: $space-2 $space-4;
  background: var(--mat-sys-primary);
  color: var(--mat-sys-on-primary);
  font: var(--mat-sys-label-large);
  border-radius: var(--mat-sys-corner-small);
  text-decoration: none;

  &:focus-visible {
    top: $space-2;
  }
}
```

### File: `src/styles/_tokens.scss`

```scss
// Spacing scale — 4px base grid
$space-1: 4px;
$space-2: 8px;
$space-3: 12px;
$space-4: 16px;
$space-5: 20px;
$space-6: 24px;
$space-8: 32px;
$space-10: 40px;
$space-12: 48px;
$space-16: 64px;

// Content width constraints
$content-max-width-xl: 1440px;
$nav-drawer-width-lg: 256px;
$nav-drawer-width-xl: 280px;
$nav-rail-width: 80px;
$bottom-nav-height: 80px;
```

### File: `src/styles/_breakpoints.scss`

```scss
@use '@angular/cdk' as cdk;

// Named breakpoint mixins that mirror Breakpoints.* constants
@mixin xs { @media (max-width: 599.98px) { @content; } }
@mixin sm { @media (min-width: 600px) and (max-width: 959.98px) { @content; } }
@mixin md { @media (min-width: 960px) and (max-width: 1279.98px) { @content; } }
@mixin lg { @media (min-width: 1280px) and (max-width: 1919.98px) { @content; } }
@mixin xl { @media (min-width: 1920px) { @content; } }

@mixin xs-sm { @media (max-width: 959.98px) { @content; } }
@mixin md-up { @media (min-width: 960px) { @content; } }
@mixin lg-up { @media (min-width: 1280px) { @content; } }
```

### File: `src/styles/_motion.scss`

```scss
// Apply inside any component SCSS that defines custom transitions/animations.
// Angular Material's own animations respect prefers-reduced-motion natively.
@mixin reduce-motion {
  @media (prefers-reduced-motion: reduce) {
    @content;
  }
}

// Convenience mixin: wraps a transition declaration and suppresses it on reduce.
@mixin motion-safe-transition($properties...) {
  transition: $properties;

  @include reduce-motion {
    transition: none;
  }
}
```

---

## Authorised CSS Custom Property Catalogue

All CSS custom properties below are emitted by `mat.theme()` and available for use in component SCSS via `var(--mat-sys-*)`. This is a non-exhaustive reference for the most common roles.

### Colour Roles

| CSS Custom Property | Light Value (Azure palette) | Dark Value (Azure palette) | Role |
|---|---|---|---|
| `--mat-sys-primary` | `#005cbb` | `#aac7ff` | Brand primary |
| `--mat-sys-on-primary` | `#ffffff` | `#002e69` | Text/icon on primary |
| `--mat-sys-primary-container` | `#d8e2ff` | `#004492` | Tinted primary surface |
| `--mat-sys-on-primary-container` | `#001945` | `#d8e2ff` | Text on primary container |
| `--mat-sys-secondary` | `#545e71` | `#bbc6dc` | Secondary actions |
| `--mat-sys-on-secondary` | `#ffffff` | `#253140` | Text on secondary |
| `--mat-sys-secondary-container` | `#d8e2f9` | `#3b4657` | Active nav indicator |
| `--mat-sys-on-secondary-container` | `#111c2b` | `#d8e2f9` | Active nav label/icon |
| `--mat-sys-surface` | `#f9f9ff` | `#111318` | Default surface / page bg |
| `--mat-sys-on-surface` | `#191c20` | `#e2e2e9` | Body text |
| `--mat-sys-surface-variant` | `#e1e2ec` | `#43474e` | Input fill, nav bg |
| `--mat-sys-on-surface-variant` | `#44474f` | `#c4c6d0` | Placeholder, subtitle |
| `--mat-sys-surface-container` | `#e5e5ef` | `#1d2024` | Bottom nav / rail bg |
| `--mat-sys-surface-container-low` | `#efeffa` | `#191c20` | Drawer bg |
| `--mat-sys-background` | `#f9f9ff` | `#111318` | Page background |
| `--mat-sys-on-background` | `#191c20` | `#e2e2e9` | Default text |
| `--mat-sys-error` | `#ba1a1a` | `#ffb4ab` | Error state |
| `--mat-sys-on-error` | `#ffffff` | `#690005` | Text on error |
| `--mat-sys-error-container` | `#ffdad6` | `#93000a` | Error banner background |
| `--mat-sys-on-error-container` | `#410002` | `#ffdad6` | Text on error container |
| `--mat-sys-outline` | `#74777f` | `#8e9099` | Input border, card outline |
| `--mat-sys-outline-variant` | `#c4c6d0` | `#43474e` | Dividers |

> Note: Exact values depend on the M3 tonal palette generated for the `azure-palette`. The CSS vars are the authoritative source at runtime — these values are illustrative only.

### Typography Tokens

All typography tokens include font, size, weight, and line-height together. Use the shorthand `font: var(--mat-sys-body-large)` pattern in SCSS.

| CSS Custom Property | Approx Size | Weight | Use |
|---|---|---|---|
| `--mat-sys-display-large` | 57 sp | 400 | Hero display |
| `--mat-sys-display-medium` | 45 sp | 400 | Large page titles |
| `--mat-sys-display-small` | 36 sp | 400 | |
| `--mat-sys-headline-large` | 32 sp | 400 | h1 |
| `--mat-sys-headline-medium` | 28 sp | 400 | h2 |
| `--mat-sys-headline-small` | 24 sp | 400 | h3 |
| `--mat-sys-title-large` | 22 sp | 400 | Toolbar title, dialog title |
| `--mat-sys-title-medium` | 16 sp | 500 | List primary text |
| `--mat-sys-title-small` | 14 sp | 500 | Chip label, nav label |
| `--mat-sys-body-large` | 16 sp | 400 | Primary body text |
| `--mat-sys-body-medium` | 14 sp | 400 | Secondary body |
| `--mat-sys-body-small` | 12 sp | 400 | Caption |
| `--mat-sys-label-large` | 14 sp | 500 | Button label |
| `--mat-sys-label-medium` | 12 sp | 500 | Input label |
| `--mat-sys-label-small` | 11 sp | 500 | Badge, helper text |

### Shape Tokens

| CSS Custom Property | Value | Usage |
|---|---|---|
| `--mat-sys-corner-none` | `0px` | Square surfaces |
| `--mat-sys-corner-extra-small` | `4px` | Chips, dense elements |
| `--mat-sys-corner-small` | `8px` | Menu items, text fields |
| `--mat-sys-corner-medium` | `12px` | Cards |
| `--mat-sys-corner-large` | `16px` | Dialogs, bottom sheets |
| `--mat-sys-corner-extra-large` | `28px` | Floating elements |
| `--mat-sys-corner-full` | `50%` | FABs, avatar |

### Elevation Tokens (Tonal)

Angular Material M3 uses tonal elevation (surface colour overlay) rather than box shadows. The following vars control the overlay opacity:

| CSS Custom Property | Elevation Level | Usage |
|---|---|---|
| `--mat-sys-level0` | 0 dp — no overlay | Base surface |
| `--mat-sys-level1` | 1 dp | Cards (resting) |
| `--mat-sys-level2` | 3 dp | Menus, dropdowns |
| `--mat-sys-level3` | 6 dp | Dialogs, drawers |
| `--mat-sys-level4` | 8 dp | Navigation bar |
| `--mat-sys-level5` | 12 dp | Bottom sheets |

---

## Prohibited Values

The following patterns are **build failures** (linting enforced):

```scss
// PROHIBITED — hardcoded colour
color: #005cbb;
background: rgb(0, 92, 187);

// PROHIBITED — hardcoded font size
font-size: 16px;
font-weight: bold;

// PROHIBITED — hardcoded border radius
border-radius: 12px;

// PROHIBITED — hardcoded spacing
margin: 16px;
padding: 24px 16px;

// PROHIBITED — hardcoded box shadow
box-shadow: 0 2px 4px rgba(0,0,0,0.2);

// CORRECT equivalents
color: var(--mat-sys-primary);
font: var(--mat-sys-body-large);
border-radius: var(--mat-sys-corner-medium);
margin: $space-4;
padding: $space-6 $space-4;
// box-shadow: use mat.elevation() or rely on M3 tonal elevation
```
