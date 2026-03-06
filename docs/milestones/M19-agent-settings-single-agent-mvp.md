# Milestone M19: Agent Settings MVP (Single-Agent UX, Multi-Agent Backend)

## Summary
Implement agent configuration in admin settings with working forms and onboarding-style validation, backed by new `/agents` CRUD endpoints and a shared agent context in WebApp.

MVP scope is one agent per subscription in UI flow:
- Load `GET /agents?subscription={subscriptionId}`
- Use the first agent as active settings target
- Wire `Personality` and `Channels` tabs only
- Keep `Skills` and `Inventory` routes as placeholders
- In settings, channels are optional; no channels means agent is not ready to operate

## User Decisions (Locked)
- [x] Use generated WebAPI client only in WebApp (no manual `fetch` methods).
- [x] `GET /agents/{agentId}` returns full agent info including channels and raw `telegramBotToken`.
- [x] MVP assumes onboarding already created at least one agent; settings does not auto-create fallback agent.
- [x] Keep current `Skills` and `Inventory` placeholder routes.
- [x] Personality tab includes optional `personalityPromptRaw`.
- [x] `personalityPromptSanitized` update is out of scope for this ticket.
- [x] Channels remain per-card save actions (Telegram/WhatsApp/SMS).
- [x] In settings, all channel fields are optional; no at-least-one rule in settings forms.
- [x] If all channels are empty, agent is considered not ready to operate.
- [x] Do not alter page design/look-and-feel; visual design and existing UI behavior patterns are out of scope for this feature.

## Findings from Repository Analysis

### Existing state in WebApp
- [x] Agent settings pages are currently static placeholder forms:
  - `HeyAlan.WebApp/src/app/admin/settings/agent/page.tsx`
  - `HeyAlan.WebApp/src/app/admin/settings/agent/channels/page.tsx`
- [x] Onboarding already defines desired validation behavior and form style using `react-hook-form` + `zodResolver`:
  - Required profile fields: name + personality
  - Channel validation in onboarding includes at-least-one-channel rule (must not be copied to settings)
  - File: `HeyAlan.WebApp/src/app/onboarding/page.tsx`
- [x] Generated API client is already the project convention:
  - SDK exports from `@/lib/api`
  - React Query helpers from `@/lib/api/@tanstack/react-query.gen.ts`
- [x] Generated client now includes `/agents` management CRUD endpoints after running `yarn openapi-ts`.

### Existing state in backend/domain
- [x] Data model already supports multi-agent per subscription and needed fields:
  - `Agent.Name`, `Agent.Personality`, `Agent.PersonalityPromptRaw`, `Agent.PersonalityPromptSanitized`
  - `Agent.TwilioPhoneNumber`, `Agent.TelegramBotToken`, `Agent.WhatsappNumber`
  - File: `HeyAlan/Data/Entities/Agent.cs`
- [x] Onboarding service already contains channel/domain rules; only selected parts should be reused for settings:
  - Telegram token uniqueness checks
  - Webhook registration on token change with rollback on failure
  - At-least-one-channel enforcement exists for onboarding and should remain onboarding-specific
  - File: `HeyAlan/Onboarding/SubscriptionOnboardingService.cs`
- [x] Current WebAPI does not expose `/agents` CRUD endpoints yet.

## API and Interface Changes
- [ ] Add authenticated WebAPI endpoints:
  - [x] `GET /agents?subscription={subscriptionId}`
  - [x] `POST /agents?subscription={subscriptionId}`
  - [x] `GET /agents/{agentId}`
  - [x] `POST /agents/{agentId}`
  - [x] `DELETE /agents/{agentId}`
- [x] `GET /agents/{agentId}` response includes complete agent settings data for personality and channels, including raw `telegramBotToken`.
- [x] Agent responses include `isOperationalReady` (computed true when at least one channel is configured; false otherwise).
- [x] `POST /agents/{agentId}` accepts profile and channel fields required by MVP:
  - [x] `name`
  - [x] `personality`
  - [x] `personalityPromptRaw` (optional)
  - [x] `twilioPhoneNumber` (optional)
  - [x] `whatsappNumber` (optional)
  - [x] `telegramBotToken` (optional)

## Gate A - Backend Agents Endpoint Surface
- [x] Introduce WebAPI route group for `/agents` with subscription membership authorization checks.
- [x] Define endpoint DTOs with operation-centric naming (`*Input` / `*Result`) per coding guideline.
- [x] Implement list/get/create/update/delete operations for agents.
- [x] Keep update behavior compatible with existing onboarding domain rules where applicable (normalization, per-field validity, token uniqueness, webhook registration failure handling).
- [x] Allow all channels to be empty in settings update (no `channels_at_least_one_required` for `/agents/{agentId}`).
- [x] Ensure errors return predictable machine-readable error codes for frontend handling.

### Gate A Acceptance Criteria
- [x] Authorized user can list agents for their subscription.
- [x] Authorized user can read full details for one agent.
- [x] Authorized user can update profile + channels in one endpoint call.
- [x] Update with all channels empty succeeds and returns `isOperationalReady = false`.
- [x] Unauthorized or non-member requests are blocked.

## Gate B - OpenAPI and WebApp Client Contract
- [x] Expose new `/agents` operations in OpenAPI from WebAPI annotations/signatures.
- [x] Hand off for client generation (`yarn openapi-ts`) after WebAPI interface changes.
- [x] Confirm generated SDK/react-query helpers include all new `/agents` operations.
- [ ] WebApp implementation uses generated client methods only; no direct `fetch`.

### Gate B Acceptance Criteria
- [x] `HeyAlan.WebApp/src/lib/api/*.gen.ts` contains `/agents` operations.
- [ ] Agent settings provider and tabs call generated methods only.

## Gate C - WebApp Agent Settings Provider
- [ ] Add an agent settings context provider under agent settings layout scope.
- [ ] Resolve active subscription id from session context.
- [ ] Load `GET /agents?subscription={subscriptionId}` and select first agent.
- [ ] Fetch selected agent details via `GET /agents/{agentId}`.
- [ ] Expose shared state and mutations to tabs:
  - [ ] `agent`
  - [ ] `isLoading`
  - [ ] `errorMessage`
  - [ ] `refresh()`
  - [ ] `updateProfile(...)`
  - [ ] `updateChannels(...)`
- [ ] Keep cache/state sync after successful updates.
- [ ] Expose and cache `isOperationalReady` for settings status UI.

### Gate C Acceptance Criteria
- [ ] Personality and Channels tabs consume same in-memory agent instance from provider.
- [ ] Navigation between tabs does not lose unsaved loaded state.

## Gate D - Personality Tab (Working Form + Validation)
- [ ] Replace static personality form with `react-hook-form` + `zodResolver`.
- [ ] Implement field validation aligned with onboarding style:
  - [ ] `agentName` required
  - [ ] `agentPersonality` required enum (`casual|balanced|business` mapped to API enum)
  - [ ] `personalityPromptRaw` optional
- [ ] Submit updates through generated `POST /agents/{agentId}` client call.
- [ ] Display save success/error feedback.

### Gate D Acceptance Criteria
- [ ] Invalid inputs show form-level/field-level errors.
- [ ] Valid save persists and refreshes shared agent state.

## Gate E - Channels Tab (Per-Card Save + Cross-Field Rules)
- [ ] Keep per-card saves for Telegram/WhatsApp/SMS UX.
- [ ] Use one shared channels form model with settings-specific validation:
  - [ ] E.164-like validation for phone/whatsapp fields
  - [ ] No cross-field requirement that at least one channel must be configured
- [ ] Each card save submits merged channel state via generated `POST /agents/{agentId}` call.
- [ ] Preserve existing value on partial card edits; do not accidentally clear other channels unless explicitly changed.
- [ ] Show warning/notice when `isOperationalReady` is false.

### Gate E Acceptance Criteria
- [ ] Per-card save works when all channels are empty or partially configured.
- [ ] API errors (including token conflicts/telegram registration failures) are surfaced clearly.
- [ ] UI clearly indicates non-ready agent when no channels are configured.

## Gate F - Testing and Verification
- [ ] Backend tests:
  - [ ] Endpoint auth/membership checks for list/get/update/delete.
  - [ ] Update validation for required profile fields and optional channels in settings endpoint.
  - [ ] `isOperationalReady` is false when all channels are empty and true when any channel is configured.
  - [ ] Onboarding endpoint still enforces channels-at-least-one rule (no regression).
  - [ ] Telegram token uniqueness + rollback behavior.
- [ ] Frontend tests (or verification pass if tests not yet in place):
  - [ ] Provider loads first agent from list and shares across tabs.
  - [ ] Personality validation and save path.
  - [ ] Channels per-card save path with optional-channel behavior.
  - [ ] Non-ready warning shown when all channels are empty.
  - [ ] Generated-client-only usage in settings implementation.

### Gate F Acceptance Criteria
- [ ] New behavior is covered and no regression in existing onboarding/channel semantics.

## Risks / Notes
- [ ] Returning raw Telegram token is sensitive; must not be logged in server/client logs.
- [ ] Endpoint update logic should avoid divergence from onboarding service semantics except for intentional optional-channel behavior in settings.
- [ ] OpenAPI/client generation is a required handoff dependency for WebApp integration.

## Handoff Notes for Next Context Window
- [x] Gate A and Gate B backend/OpenAPI baseline is complete, including regenerated WebApp client with `/agents` operations.
- [ ] Next window scope: implement Gate C (shared agent settings provider) and Gate E (channels tab per-card save + optional-channel behavior + `isOperationalReady` warning).
- [ ] Gate D remains open and intentionally deferred unless pulled into next window scope.
- [ ] Suggested execution order in fresh context: Gate C first (provider contract), then Gate E (consumer integration), then Gate F verification for C/E paths.
- [ ] Do not edit `.gen.ts` files manually.
- [ ] Keep `Skills`/`Inventory` untouched aside from navigation continuity.
- [ ] Keep manual `fetch` out of agent settings work; use generated SDK/react-query helpers exclusively.
