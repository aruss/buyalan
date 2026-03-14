# Milestone M22: Landing Page Accessibility and Conversion Polish

## Goal
- [x] Remove dead-end landing interactions, improve mobile navigation discoverability, and clear the currently known Lighthouse accessibility failures on the landing page.

## Scope
- [x] Landing route only in `BuyAlan.WebApp`.
- [x] CTA routing normalization for affected landing CTAs to `/admin`.
- [x] Mobile navigation enhancement using a hamburger dropdown.
- [x] Accessibility fixes limited to the currently known landing Lighthouse failures:
  - [x] missing `<main>` landmark
  - [x] heading level order skips
  - [x] insufficient text contrast
- [x] Refresh automated Playwright coverage for the landing page to match current behavior.

## Non-Goals (Out of Scope)
- [x] Full WCAG AA hardening beyond the currently known Lighthouse failures.
- [x] Backend, API, schema, database, or auth flow changes.
- [x] New dependency introduction for landing navigation.
- [x] Design-system-wide color token overhaul.
- [x] Expanding milestone scope for unrelated accessibility issues newly discovered during verification unless introduced by this work.

## User Decisions (Locked)
- [x] Route affected landing conversion CTAs to `/admin`.
- [x] Add a mobile hamburger dropdown navigation.
- [x] Put `Start Free Trial` inside the mobile dropdown and remove the duplicate standalone mobile CTA.
- [x] Keep the hero CTA as the primary visible mobile conversion action.
- [x] Fix the currently known Lighthouse failures in this iteration only.
- [x] Update automated Playwright coverage as part of this milestone.

## Findings from Repository Analysis
- [x] Hero secondary CTA previously pointed to `#demo`, but no matching section id existed.
- [x] Pricing card CTAs previously used `href="#"`, creating dead-end interactions.
- [x] The landing route previously rendered navigation, page sections, and footer without a `<main>` landmark around primary content.
- [x] Landing dashboard cards previously used `h4` elements directly under a section `h2`, creating heading-order skips.
- [x] Landing footer column labels previously used heading tags that contributed to heading-order cleanup scope.
- [x] Lighthouse risk areas were concentrated in muted hero, dashboard, compliance, and footer text.
- [x] The repo already included a local Radix dropdown wrapper in `src/components/landing/ui/dropdown-menu.tsx`.
- [x] Existing `BuyAlan.WebApp/tests/landing-page.spec.ts` was stale and did not match the current landing page content or navigation behavior.

## Architecture Decisions (Locked)
- [x] Reuse the existing landing dropdown primitive; do not add a new navigation library.
- [x] Keep desktop navigation structure and section-link behavior unchanged.
- [x] Keep routing deterministic for this milestone: affected landing conversion CTAs resolve to `/admin`.
- [x] Keep accessibility fixes narrowly targeted to the known failures and the elements directly involved in those failures.
- [x] Treat Playwright updates as regression coverage for current landing behavior, not as broad end-to-end expansion.

## Implementation Plan by Gate

## Gate A: Fix Broken and Dead-End CTA Destinations
- [x] Update hero secondary CTA from `#demo` to `/admin`.
- [x] Replace pricing CTA placeholders with `/admin`.
- [x] Verify no landing conversion CTA remains a no-op hash placeholder unless it points to a real section id.

### Gate A Acceptance Criteria
- [x] `View Live Demo` no longer points to a missing in-page anchor.
- [x] `Deploy Free`, `Initialize Trial`, and `Contact Integrations` navigate to `/admin`.
- [x] Landing conversion CTAs no longer produce dead-end behavior.

## Gate B: Mobile Navigation Dropdown and CTA Placement
- [x] Implement a hamburger-triggered mobile dropdown menu using the existing Radix wrapper.
- [x] Include section links for `Features`, `Merchant Dashboard`, `Pricing`, and `Trust`.
- [x] Include `Start Free Trial` in the dropdown linking to `/admin`.
- [x] Remove the standalone mobile top-bar CTA outside the dropdown.
- [x] Preserve existing desktop nav links and desktop CTA behavior.
- [x] Ensure menu selections close predictably after activation.

### Gate B Acceptance Criteria
- [x] On mobile, section links are reachable through the dropdown.
- [x] On mobile, `Start Free Trial` appears only in the dropdown.
- [x] Trigger and menu support keyboard interaction, including open, traversal, Escape close, and focus return to the trigger.
- [x] Selecting a section link closes the menu and navigates to the target section.
- [x] On desktop, existing nav remains visually and functionally unchanged.

## Gate C: Landmark and Heading Semantics
- [x] Wrap landing primary content in a single `<main>` landmark between navigation and footer.
- [x] Resolve heading hierarchy skips in landing sections, especially dashboard feature titles and footer section labels.
- [x] Preserve existing visual style when changing semantic elements.

### Gate C Acceptance Criteria
- [x] Lighthouse no longer reports the missing main landmark failure.
- [x] Lighthouse no longer reports heading-order failures.
- [x] Screen-reader landmark navigation exposes one clear main content region.

## Gate D: Contrast Remediation
- [x] Adjust low-contrast text classes only in currently failing landing areas:
  - [x] hero supported-channels label
  - [x] dashboard subtle labels and metadata text
  - [x] compliance descriptive copy
  - [x] footer descriptive, link, and meta text
- [x] Preserve the current visual direction while meeting minimum contrast for the failing cases.

### Gate D Acceptance Criteria
- [x] Lighthouse no longer reports color-contrast failures for the known landing issues.
- [x] No obvious visual regressions are introduced in updated sections.
- [x] Updated text remains legible in the default rendered theme.

## Gate E: Landing Regression Coverage Refresh
- [x] Rewrite stale landing Playwright coverage to reflect the current landing page.
- [x] Add automated coverage for CTA routing and mobile dropdown behavior.
- [x] Add keyboard-path verification for the mobile menu interaction.

### Gate E Acceptance Criteria
- [x] Automated landing tests assert current hero content instead of removed legacy hardware content.
- [x] Automated tests verify affected CTAs route correctly.
- [x] Automated tests verify mobile menu open/close behavior and menu item visibility.
- [x] Automated tests cover at least one keyboard dismissal/navigation path for the mobile menu.

## Test Cases and Scenarios (Authoritative)
1. **CTA routing**
- [x] Click hero and pricing conversion CTAs and verify navigation to `/admin`.

2. **Mobile navigation behavior**
- [x] Emulate a mobile viewport and open the hamburger menu.
- [x] Verify `Features`, `Merchant Dashboard`, `Pricing`, and `Trust` are reachable through the menu.
- [x] Verify `Start Free Trial` appears in the menu and routes to `/admin`.
- [x] Verify the menu closes after selection and via Escape.

3. **Desktop regression**
- [x] Verify desktop nav links still point to in-page sections and scroll correctly.
- [x] Verify desktop CTA presence and placement remain unchanged.

4. **Accessibility verification**
- [x] Run Lighthouse on the landing page in desktop mode and confirm the currently known failures are cleared.
- [x] Run Lighthouse on the landing page in mobile mode and confirm the currently known failures are cleared.
- [x] Perform a quick keyboard-only pass through nav, menu, and landing CTAs.

5. **Automated regression coverage**
- [x] Run the landing Playwright spec and confirm it passes with the current landing content and mobile nav behavior.

6. **Runtime sanity**
- [ ] Confirm no console errors on landing load and interaction.
- [ ] Confirm no failing network requests are introduced by nav or CTA changes.

## Public Interfaces / Contracts
- [x] No backend API contract changes.
- [x] No DTO, schema, migration, or generated client changes.
- [x] No `.gen.ts` or `swagger.json` changes expected.

## Risks and Mitigations
- [x] Risk: mobile dropdown introduces keyboard or focus regressions.
- [x] Mitigation: rely on the existing Radix primitive and verify trigger/menu keyboard behavior explicitly.
- [x] Risk: stronger text colors visibly shift branding tone.
- [x] Mitigation: limit contrast changes to currently failing text only.
- [x] Risk: semantic heading changes alter spacing defaults.
- [x] Mitigation: preserve existing classes and verify rendered spacing after tag changes.
- [x] Risk: stale automated tests hide regressions if not updated.
- [x] Mitigation: treat Playwright refresh as part of the milestone, not optional follow-up.

## Handoff Notes for New Context Window
- [x] Start in `BuyAlan.WebApp`; this milestone is frontend-only.
- [x] Implement in gate order `A -> B -> C -> D -> E`.
- [x] Reuse `src/components/landing/ui/dropdown-menu.tsx`; do not add a new navigation dependency.
- [x] Keep the CTA routing rule deterministic: affected conversion CTAs route to `/admin`.
- [x] Keep desktop navigation behavior unchanged.
- [x] Do not change backend contracts, `.gen.ts`, or generated artifacts.
- [x] Verify completion with Lighthouse desktop and mobile plus updated Playwright landing coverage.
- [x] If new unrelated accessibility issues are found during implementation, record them as follow-up work rather than expanding M22 automatically.

## Assumptions and Defaults
- [x] The existing `(landing)` app segment remains the landing route host.
- [x] `/admin` remains the correct login/onboarding destination for landing conversion intent.
- [x] Existing styles and utilities are sufficient; no token-system refactor is required.
- [x] Existing Radix dropdown behavior is sufficient for the mobile menu interaction pattern.
- [x] No localization or content rewrite is required in this milestone.
