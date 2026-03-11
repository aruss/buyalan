# Milestone M22: Landing Page Accessibility and Conversion Polish

## Goal
- [ ] Remove broken/dead-end landing interactions and improve mobile discoverability while resolving all currently failing Lighthouse accessibility audits.

## Scope
- [ ] Landing page only (`BuyAlan.WebApp` landing route and landing components).
- [ ] CTA routing normalization to `/admin` for this milestone.
- [ ] Mobile navigation enhancement (hamburger dropdown).
- [ ] Accessibility fixes for current known failures:
  - [ ] missing `<main>` landmark
  - [ ] heading level order skips
  - [ ] insufficient text contrast

## Non-Goals (Out of Scope)
- [ ] Full WCAG AA hardening beyond currently failing Lighthouse checks.
- [ ] Backend/API/schema/database changes.
- [ ] New dependency introduction for landing navigation.
- [ ] Design-system-wide color token overhaul.

## User Decisions (Locked)
- [ ] Route all relevant landing CTAs to `/admin`.
- [ ] Add a mobile hamburger dropdown navigation.
- [ ] Put `Start Free Trial` in the mobile dropdown (remove duplicate top-bar mobile CTA outside menu).
- [ ] Keep the hero CTA as the primary visible mobile call to action.
- [ ] Fix all current Lighthouse failures in this iteration (not broader accessibility expansion).

## Findings from Repository Analysis
- [ ] Hero secondary CTA points to `#demo`, but no matching `id="demo"` section exists.
- [ ] Pricing card CTAs use `href="#"`, creating dead-end interactions.
- [ ] Layout currently has no `<main>` landmark around page content.
- [ ] Some sections skip heading levels (e.g., section-level heading to `h4`).
- [ ] Lighthouse reports multiple contrast violations in muted zinc text on light/dark backgrounds.
- [ ] Landing already includes a local Radix dropdown primitive wrapper (`src/components/landing/ui/dropdown-menu.tsx`) that can be reused.

## Architecture Decisions (Locked)
- [ ] Reuse existing landing dropdown primitive (`@radix-ui/react-dropdown-menu` wrapper); do not add new nav library dependencies.
- [ ] Keep desktop nav structure unchanged; only mobile nav behavior changes.
- [ ] Keep routing simple and deterministic for this pass: all affected landing CTAs resolve to `/admin`.

## Implementation Plan by Gate

## Gate A: Fix Broken and Dead-End CTA Destinations
- [ ] Update hero secondary CTA from `#demo` to `/admin`.
- [ ] Replace pricing CTA placeholders (`href="#"`) with `/admin`.
- [ ] Validate no landing CTA on this page resolves to a no-op hash placeholder after edits.

### Gate A Acceptance Criteria
- [ ] `View Live Demo` no longer points to a missing in-page anchor.
- [ ] `Deploy Free`, `Initialize Trial`, and `Contact Integrations` navigate to `/admin`.
- [ ] Manual click-through confirms no dead-end CTA behavior on landing.

## Gate B: Mobile Navigation Dropdown and CTA Placement
- [ ] Implement hamburger-triggered dropdown nav for mobile viewport.
- [ ] Include section links: `Features`, `Merchant Dashboard`, `Pricing`, `Trust`.
- [ ] Include `Start Free Trial` item linking to `/admin` inside dropdown.
- [ ] Remove standalone top-bar mobile `Start Free Trial` button outside dropdown.
- [ ] Preserve existing desktop nav links and desktop CTA behavior.

### Gate B Acceptance Criteria
- [ ] On mobile, nav links are reachable via dropdown menu.
- [ ] On mobile, top bar no longer duplicates CTA outside dropdown.
- [ ] Menu supports open/close by click and keyboard interaction (Enter/Escape/Tab flow).
- [ ] On desktop, existing nav remains visually and functionally unchanged.

## Gate C: Landmark and Heading Semantics
- [ ] Wrap landing main content in a `<main>` landmark between navigation and footer.
- [ ] Resolve heading hierarchy skips in landing sections (notably dashboard and footer section labels).
- [ ] Keep visual style unchanged unless required by semantic element change.

### Gate C Acceptance Criteria
- [ ] Lighthouse no longer reports `landmark-one-main`.
- [ ] Lighthouse no longer reports `heading-order`.
- [ ] Screen-reader landmark navigation exposes a single clear main content region.

## Gate D: Contrast Remediation
- [ ] Adjust low-contrast text classes in flagged landing areas:
  - [ ] hero channel label text
  - [ ] dashboard panel subtle labels/meta text
  - [ ] footer descriptive/link/meta text
- [ ] Ensure updates preserve the existing visual language while meeting minimum contrast.

### Gate D Acceptance Criteria
- [ ] Lighthouse no longer reports `color-contrast`.
- [ ] No newly introduced contrast regressions in updated elements.
- [ ] Footer and dashboard remain legible in default theme without visual breakage.

## Test Cases and Scenarios (Authoritative)
1. **CTA routing**
- [ ] Click each hero/pricing CTA and verify navigation to `/admin`.

2. **Mobile navigation behavior**
- [ ] Emulate mobile viewport; open hamburger menu and navigate via each section link.
- [ ] Confirm `Start Free Trial` appears in menu and routes to `/admin`.
- [ ] Verify menu closes predictably after selection and via Escape.

3. **Desktop regression**
- [ ] Verify desktop nav links still point to in-page sections and scroll correctly.
- [ ] Verify desktop CTA presence/placement remains unchanged.

4. **Accessibility verification**
- [ ] Run Lighthouse (desktop) and confirm previous failures are cleared.
- [ ] Run Lighthouse (mobile) and confirm previous failures are cleared.
- [ ] Perform quick keyboard-only tab pass through nav/menu/CTAs.

5. **Runtime sanity**
- [ ] Confirm no console errors on landing load and interaction.
- [ ] Confirm no failing network requests introduced by nav/menu updates.

## Public Interfaces / Contracts
- [ ] No backend API contract changes.
- [ ] No DTO/type/schema/migration changes.
- [ ] No generated client (`.gen.ts`) changes expected.

## Risks and Mitigations
- [ ] Risk: mobile dropdown introduces focus trap or keyboard regression.
  - [ ] Mitigation: use existing Radix primitive behavior and validate keyboard flow explicitly.
- [ ] Risk: stronger contrast tokens alter intended branding.
  - [ ] Mitigation: restrict color changes to failing elements only and keep typography/layout intact.
- [ ] Risk: semantic heading changes alter spacing defaults.
  - [ ] Mitigation: preserve classes and validate rendered spacing after tag updates.

## Handoff Notes for New Context Window (Implementation-Ready)
- [ ] Start in `BuyAlan.WebApp` landing components; this milestone is frontend-only.
- [ ] Implement in gate order `A -> B -> C -> D`; each gate is independently verifiable and reduces rework risk.
- [ ] Reuse existing landing dropdown primitive in `src/components/landing/ui/dropdown-menu.tsx`; do not introduce new nav dependencies.
- [ ] Keep routing rule deterministic for this milestone: affected landing CTAs resolve to `/admin`.
- [ ] Do not change `.gen.ts`, `swagger.json`, or backend contracts.
- [ ] Verify completion with Lighthouse desktop + mobile and include before/after scores in handoff output.
- [ ] If any new issue is discovered during implementation (e.g., unrelated accessibility regression), record it under follow-ups without expanding milestone scope unless explicitly requested.

## Assumptions and Defaults
- [ ] Landing route remains served from the existing `(landing)` app segment.
- [ ] `/admin` remains valid onboarding/login destination for trial intent.
- [ ] Existing styles/utilities are sufficient; no token system refactor is required.
- [ ] No localization/content rewrite required in this milestone.
