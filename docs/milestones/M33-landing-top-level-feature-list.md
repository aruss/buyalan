# Milestone M33: Landing Top-Level Feature List Refresh

## Goal
Replace the current four-card `LandingFeatures` section with an eight-card, website-friendly feature grid that stays aligned with the existing landing page visual language while broadening the product story.

The section must remain:
- Square-led,
- merchant-outcome focused,
- roadmap-inclusive without becoming low-credibility marketing copy.

## Scope
- Update `BuyAlan.WebApp/src/components/landing/landing-features.tsx`.
- Keep the section in its current place on the landing page.
- Preserve the current section structure:
  - headline,
  - supporting sentence,
  - icon-led feature cards.
- Expand the grid from 4 cards to 8 cards.
- Refresh the feature copy so it reads as polished website content instead of an internal capability list.

## Non-Goals
- No redesign of the landing page outside the `LandingFeatures` section.
- No changes to hero, dashboard, pricing, navigation, or compliance sections.
- No new feature-flag behavior.
- No backend, API, or data-model work.

## User Decisions (Locked)
- [x] Copy scope is roadmap-inclusive.
- [x] The section should emphasize merchant outcomes.
- [x] The feature section should expand to 8 cards.
- [x] Square should remain front-and-center in the copy.
- [x] The section should remain structurally close to the current implementation rather than becoming a different layout type.

## Content Contract (Authoritative)

### Section Headline
- [ ] `Sell, support, and close orders in chat.`

### Section Subheadline
- [ ] `BuyAlan connects to your Square operations so customers can discover products, ask questions, complete checkout, and stay informed without leaving the conversation.`

### Feature Cards
- [ ] `Inventory-Aware Answers`
  - [ ] `Uses your Square catalog to answer product questions with context around what you sell, not generic chatbot guesses.`
- [ ] `Omnichannel Conversations`
  - [ ] `Engage customers across SMS, WhatsApp, and Telegram through one unified conversational sales workflow.`
- [ ] `Natural Upselling`
  - [ ] `Recommend relevant add-ons, alternatives, and higher-value options in the flow of the conversation.`
- [ ] `Checkout Inside Chat`
  - [ ] `Turn buying intent into a live order flow with payment-link support and a cleaner path from question to purchase.`
- [ ] `Order & Delivery Updates`
  - [ ] `Keep customers informed with status changes, payment progress, and fulfillment-related updates after checkout.`
- [ ] `Human Takeover`
  - [ ] `Step in when needed, continue the conversation manually, and return control to the agent without losing context.`
- [ ] `Merchant Control Center`
  - [ ] `Configure agent behavior, review conversations, manage settings, and monitor operations from the admin workspace.`
- [ ] `Trust-First Messaging`
  - [ ] `Respect opt-in boundaries, support opt-out behavior, and keep customer communication aligned with compliance expectations.`

## Implementation Plan by Gate

## Gate A: Section Copy Rewrite
- [ ] Replace the current feature-section headline with the approved new headline.
- [ ] Replace the current feature-section subheadline with the approved new subheadline.
- [ ] Remove the existing 4-card feature copy.
- [ ] Add the approved 8-card feature list with concise title + one-sentence description per card.

### Gate A Acceptance Criteria
- [ ] Copy reads like polished website content rather than milestone or roadmap language.
- [ ] Claims remain credible and do not overstate autonomous behavior.
- [ ] Square remains explicit in the section framing.

## Gate B: Grid Expansion and Layout Preservation
- [ ] Expand the feature grid from 4 cards to 8 cards.
- [ ] Keep the current section placement, card styling, spacing, and hover treatment recognizable.
- [ ] Preserve responsive behavior with:
  - [ ] `grid-cols-1` on small screens,
  - [ ] `md:grid-cols-2`,
  - [ ] `lg:grid-cols-4`.
- [ ] Ensure the 8 cards render as two rows on large screens without looking cramped.

### Gate B Acceptance Criteria
- [ ] The updated section still matches the existing landing page visual language.
- [ ] The section remains readable and balanced on mobile, tablet, and desktop.
- [ ] Card titles do not wrap awkwardly at common breakpoints.

## Gate C: Icon and Tone Alignment
- [ ] Keep one icon per feature card.
- [ ] Use icons that directly match the card meaning and stay within the current restrained visual style.
- [ ] Preserve the current neutral/premium tone used by `landing-hero.tsx`, `landing-dashboard.tsx`, and `landing-compliance.tsx`.
- [ ] Avoid jargon-heavy or inflated AI language.

### Gate C Acceptance Criteria
- [ ] Icon and copy pairings are easy to scan.
- [ ] The section tone matches the rest of the landing page.
- [ ] The new feature list feels broader and stronger than the current 4-card version without reading like a technical checklist.

## Gate D: Verification
- [ ] Verify the section remains visually balanced relative to hero and dashboard sections.
- [ ] Verify decorative icons remain `aria-hidden`.
- [ ] Verify no heading hierarchy regressions are introduced.
- [ ] Verify contrast and typography remain consistent with the current landing implementation.

## Implementation Notes
- This milestone is content-and-layout scoped to `LandingFeatures`.
- The request is to upgrade the landing-page feature narrative, not to redesign the whole landing page.
- “Website friendly” means concise, merchant-facing copy that is easy to scan.
- Roadmap-inclusive phrasing is allowed, but the implementation should avoid hard claims that imply every downstream capability is already fully shipped.

## Assumptions and Defaults Chosen
- [ ] The existing `LandingFeatures` component remains the source of truth.
- [ ] The component keeps the same overall section anatomy rather than being replaced with a new pattern.
- [ ] No new analytics, tracking, or CTA changes are required for this milestone.
- [ ] No backend or API changes are needed to support the landing copy refresh.
