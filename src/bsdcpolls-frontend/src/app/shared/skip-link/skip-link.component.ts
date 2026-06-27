import { Component } from '@angular/core';

/** Renders the "Skip to main content" link as the first focusable element on every page. */
@Component({
  selector: 'app-skip-link',
  standalone: true,
  template: `<a href="#main-content" class="skip-link">Skip to main content</a>`,
})
export class SkipLinkComponent {}
