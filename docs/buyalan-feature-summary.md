# BuyAlan Feature Summary

This document compiles the feature set described in `docs/milestones/*.md`.

Status labels in the milestone matrix are derived from checklist state in each milestone file:
- `Complete`: all checklist items checked.
- `In progress`: mix of checked and unchecked items.
- `Planned`: checklist items exist but none are checked.
- `Spec only`: milestone defines scope but does not use checklist tracking.

## 1. Marketing-Style Feature List

### What BuyAlan Does
BuyAlan is an autonomous commerce agent that helps businesses engage customers over chat, understand what they want, guide them through checkout, process payment, and keep them updated afterward.

### Core Product Features
- Secure account access with local sign-in plus external providers.
- Guided onboarding for new subscriptions, including commerce connection, agent setup, channels, and team setup.
- Admin workspace with dashboard, navigation, user profile context, and onboarding reminders.
- Customer messaging over Telegram, with durable ingestion, outbound delivery, and conversation history.
- Conversation inbox with message persistence and read-state APIs.
- Agent configuration for personality, channels, and settings.
- Subscription-scoped Square connection for commerce operations without requiring a live user session.
- Local Square catalog cache so customer-facing workflows can use synced product data instead of live provider lookups.
- Agent-level product access controls and regional zip-code allowlists.
- Agent skills system with shared credential management and runtime-ready skill descriptors.
- Google Maps address normalization as an initial system-managed skill.
- Tool-driven commerce actions for catalog lookup, checkout validation, order preparation, order creation, payment-link creation, and order-status retrieval.
- Structured conversation state and customer identity resolution across turns.
- LLM orchestration layer to decide whether the agent should answer, ask follow-up questions, call tools, update state, or defer to a human.
- Checkout completion flow that turns collected conversation state into orders and payment requests.
- Post-order status tracking and proactive customer updates.
- Human handoff so operators can take over a conversation, edit state, reply as the agent, and resume automation later.
- Transactional email pipeline through SendGrid.
- Newsletter signup with double opt-in confirmation.
- Feature-flag support for frontend rollout control.

### Supporting Platform Capabilities
- Identity and subscription membership foundations.
- OAuth callback handling for proxied and public deployments.
- Telegram token/webhook orchestration.
- Square service and credential consolidation.
- Messaging infrastructure migration toward Wolverine.
- Internal refactors to improve maintainability and feature isolation.

## 2. Milestone Matrix

| Milestone | Feature Area | Status |
| --- | --- | --- |
| M1 | Identity integration, secure auth, local login/logout, email sender stub, Google auth foundation | In progress |
| M2 | Admin dashboard shell with top bar, sidebar, and basic navigation | Complete |
| M3 | Admin breadcrumbs with route-map fallback and page-level overrides | Planned |
| M4 | Telegram webhook registration service | Planned |
| M5 | Telegram incoming/outgoing delivery pipeline | In progress |
| M6 | Conversation persistence, read state, and inbox REST APIs | Spec only |
| M7 | Google external auth via custom `/auth` flow | In progress |
| M8 | Square auth, subscription-scoped Square connection, and backend-driven onboarding | In progress |
| M9 | Authenticated user profile in admin sidebar via session context | In progress |
| M10 | Automatic subscription membership handling for new external-auth users | In progress |
| M11 | Internal code organization and feature-folder refactors | In progress |
| M12 | Buffered per-conversation incoming message processing with cancel/restart semantics | In progress |
| M13 | Multi-bot Telegram channel refactor with reliable token routing and onboarding-critical webhook setup | In progress |
| M14 | Onboarding guard plus sidebar reminder for incomplete setup | In progress |
| M14.1 | Onboarding resume/skip behavior refactor with Square still required for activation | In progress |
| M15 | Messaging migration from MassTransit to WolverineFx | In progress |
| M16 | Canonical public OAuth callback URL handling | In progress |
| M17 | Newsletter subscription with SendGrid double opt-in and in-app confirmation | In progress |
| M18 | Single Square app callback broker | In progress |
| M19 | Agent settings MVP with single-agent UX on multi-agent backend | In progress |
| M20 | Consolidated Telegram token orchestration | In progress |
| M21 | Consolidated Square service ownership for token lifecycle and connect/disconnect flows | In progress |
| M22 | Landing page accessibility, mobile navigation, and conversion polish | In progress |
| M23 | Subscription-scoped Square catalog sync cache, sync controls, product access rules, and zip allowlists | In progress |
| M24 | API-first agent skills and credential management | In progress |
| M25 | Skill-orchestrated Square operations with confirmation and idempotency guards | In progress |
| M26 | Conversation state core and customer identity resolution | In progress |
| M27 | LLM orchestration and skill arbitration runtime | In progress |
| M28 | Checkout completion, order creation, and payment flow | In progress |
| M29 | Order-status tracking and proactive customer updates | In progress |
| M30 | Human handoff and manual conversation-state control | In progress |
| M31 | Unified queued email sender through SendGrid | In progress |
| M32 | Team member invitations across onboarding, settings, redemption, and queued email | In progress |
| M32.1 | Frontend feature flags for SSR and client-side code | In progress |

## 3. High-Level Product Capability Map

### Identity, onboarding, and admin
- M1, M7, M8, M9, M10, M14, M14.1, M16, M18, M19, M32

### Messaging, inbox, and channel operations
- M4, M5, M6, M12, M13, M15, M20

### Commerce, catalog, and checkout
- M8, M18, M21, M23, M25, M28, M29

### Agent intelligence and runtime control
- M19, M24, M25, M26, M27, M30

### Marketing, email, and frontend delivery
- M17, M22, M31, M32.1

### Internal platform evolution
- M11, M15, M20, M21

## 4. Notes

- This is a roadmap-derived feature inventory, not a claim that every feature is fully shipped.
- Several milestone files mix completed work, remaining work, and follow-up items, so `In progress` should be interpreted as partially implemented.
- `M19-agent-settings-single-agent-mvp-scratchpad.md` was intentionally excluded from the matrix because it is a scratchpad, not a milestone definition.
