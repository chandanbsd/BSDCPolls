# Contract: Navigation Layout

This contract defines the exact structural and behavioural specification for the adaptive navigation shell across all five breakpoints. It is the authoritative reference for implementing `AppComponent`, `BottomNavComponent`, `NavRailComponent`, and `NavDrawerComponent`.

---

## Navigation Item Catalogue

All navigation components render exactly these four items in this order:

| # | Label | Icon | Route | aria-label |
|---|---|---|---|---|
| 1 | Feed | `home` | `/feed` | "Feed" |
| 2 | Polls | `poll` | `/polls` | "Polls" |
| 3 | Surveys | `assignment` | `/surveys` | "Surveys" |
| 4 | Profile | `account_circle` | `/profile` | "Profile" |

---

## Breakpoint → Navigation Pattern Mapping

### XSmall + Small (320 px – 959 px): Bottom Navigation Bar

**Component**: `BottomNavComponent` (`shared/nav/bottom-nav/`)

**Structural contract**:
```html
<nav aria-label="Main navigation">
  <a mat-button routerLink="/feed" routerLinkActive="active" aria-current="page">
    <mat-icon>home</mat-icon>
    <span class="nav-label">Feed</span>
  </a>
  <!-- ... repeat for each nav item -->
</nav>
```

**Layout rules**:
- Fixed to the bottom of the viewport (`position: fixed; bottom: 0; left: 0; right: 0`)
- Height: 80 px (M3 bottom navigation bar specification)
- Background: `var(--mat-sys-surface-container)` 
- Icons: 24 px, colour `var(--mat-sys-on-surface-variant)` (inactive), `var(--mat-sys-on-secondary-container)` (active)
- Labels: `var(--mat-sys-label-medium)` type token, visible always (labels hidden is deprecated in M3)
- Touch target: minimum 44 × 44 px per item (achieved by `flex: 1; min-height: 44px`)
- Active indicator: `var(--mat-sys-secondary-container)` pill shape behind icon (M3 active indicator)
- `<main>` must have `padding-bottom: 80px` to prevent content occluded by nav bar

**Keyboard behaviour**:
- All nav items are in the natural Tab order
- Active item has `aria-current="page"`
- No arrow-key navigation required (each item is a standalone link, not a composite widget)

**ARIA**:
- `<nav aria-label="Main navigation">` wraps all items
- Active link: `aria-current="page"` (updated on route change)
- Icon: `aria-hidden="true"` (label is the accessible name)

---

### Medium (960 px – 1279 px): Navigation Rail

**Component**: `NavRailComponent` (`shared/nav/nav-rail/`)

**Structural contract**:
```html
<mat-sidenav-container>
  <mat-sidenav mode="side" opened>
    <nav aria-label="Main navigation" class="nav-rail">
      <a mat-button routerLink="/feed" routerLinkActive="active" aria-current="page">
        <mat-icon>home</mat-icon>
        <span class="nav-label">Feed</span>
      </a>
      <!-- ... -->
    </nav>
  </mat-sidenav>
  <mat-sidenav-content>
    <ng-content></ng-content>
  </mat-sidenav-content>
</mat-sidenav-container>
```

**Layout rules**:
- Rail width: 80 px (icon + label column, M3 nav rail specification)
- Rail background: `var(--mat-sys-surface-container)`
- Items: stacked vertically, `flex-direction: column`, `gap: $space-3`
- Icons: 24 px
- Labels: `var(--mat-sys-label-medium)`, visible beneath each icon
- Active indicator: pill shape 56 px wide × 32 px tall behind icon
- Content area: `margin-left: 80px`

**ARIA**: Same as bottom nav — `<nav aria-label="Main navigation">`, `aria-current="page"` on active.

---

### Large + XLarge (1280 px+): Navigation Drawer

**Component**: `NavDrawerComponent` (`shared/nav/nav-drawer/`)

**Structural contract**:
```html
<mat-sidenav-container>
  <mat-sidenav mode="side" opened class="nav-drawer">
    <div class="drawer-header">
      <span class="app-name">BSDCPolls</span>
    </div>
    <nav aria-label="Main navigation">
      <a mat-list-item routerLink="/feed" routerLinkActive="active" aria-current="page">
        <mat-icon matListItemIcon>home</mat-icon>
        <span matListItemTitle>Feed</span>
      </a>
      <!-- ... -->
    </nav>
  </mat-sidenav>
  <mat-sidenav-content>
    <ng-content></ng-content>
  </mat-sidenav-content>
</mat-sidenav-container>
```

**Layout rules**:
- Drawer width: 256 px (lg), 280 px (xl)
- Background: `var(--mat-sys-surface-container-low)`
- `mat-list-item` for each nav item (M3 nav drawer item specification)
- Active item: `var(--mat-sys-secondary-container)` background, `var(--mat-sys-on-secondary-container)` text
- Drawer header: App name in `var(--mat-sys-title-large)` type token
- Content area max-width at xl: `max-width: 1440px; margin-inline: auto`

**ARIA**: Same nav + aria-current pattern. Drawer is persistent (`mode="side" opened`), so no modal ARIA semantics apply.

---

## AppComponent Shell Contract

The `AppComponent` template orchestrates navigation and the skip link:

```html
<a href="#main-content" class="skip-link">Skip to main content</a>

@if (layoutStore.isXsOrSm()) {
  <app-bottom-nav />
  <main id="main-content" tabindex="-1" class="content content--bottom-nav">
    <router-outlet />
  </main>
}

@if (layoutStore.isMd()) {
  <app-nav-rail>
    <main id="main-content" tabindex="-1" class="content">
      <router-outlet />
    </main>
  </app-nav-rail>
}

@if (layoutStore.isLgOrXl()) {
  <app-nav-drawer>
    <main id="main-content" tabindex="-1" class="content content--xl-constrained">
      <router-outlet />
    </main>
  </app-nav-drawer>
}
```

**Skip link behaviour**:
- First focusable element on every page
- Visually hidden until focused (`position: absolute; top: -40px` → `:focus { top: 0 }`)
- Foreground: `var(--mat-sys-on-primary)`; Background: `var(--mat-sys-primary)`
- `z-index` above all other content

**`<main>` element**:
- `id="main-content"` — skip link target
- `tabindex="-1"` — accepts programmatic focus without entering tab order
- Angular Router must call `document.getElementById('main-content')?.focus()` after each navigation (via `NavigationEnd` listener in `AppComponent`)

---

## Route Change Focus Management

On every `NavigationEnd` event:
1. Update `<title>` via Angular `Title` service: `"{Screen Name} — BSDCPolls"`
2. Move focus to `<main id="main-content">` via `element.focus()`
3. Update `aria-current="page"` on the newly active nav item (handled automatically by `routerLinkActive`)

This ensures screen readers announce the new page title after route transitions.
