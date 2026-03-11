# M19 Scratchpad: Agent Settings MVP Handoff

## Objective Snapshot
Deliver a production-usable agent settings flow in admin for MVP using one effective agent per subscription in UI, while preserving backend multi-agent design.

## Locked Product/UX Decisions
- Use first agent from `GET /agents?subscription=...` for MVP.
- Do not add empty-state auto-create in settings; onboarding guarantees existing agent.
- Keep `Skills` and `Inventory` tabs/pages unchanged as placeholders.
- Personality supports optional `personalityPromptRaw`.
- `personalityPromptSanitized` is not part of this implementation.
- Channels tab keeps per-card save interactions.
- `GET /agents/{agentId}` returns raw telegram token.
- WebApp must use generated OpenAPI client and generated react-query options/mutations only.

## Repository Truth Collected

### Frontend
- Agent settings pages are static placeholders:
  - `BuyAlan.WebApp/src/app/admin/settings/agent/page.tsx`
  - `BuyAlan.WebApp/src/app/admin/settings/agent/channels/page.tsx`
- Onboarding page contains target validation and form architecture:
  - `react-hook-form` + `zodResolver`
  - profile required fields + channels cross-field rule
  - `BuyAlan.WebApp/src/app/onboarding/page.tsx`
- Session context exists and exposes `currentUser.activeSubscriptionId`:
  - `BuyAlan.WebApp/src/lib/session-context.tsx`
- Generated API usage pattern already established:
  - `BuyAlan.WebApp/src/lib/api/index.ts`
  - `BuyAlan.WebApp/src/lib/api/@tanstack/react-query.gen.ts`

### Backend
- WebAPI currently has onboarding + conversation endpoints, but no general `/agents` CRUD endpoints.
- Agent entity already contains all needed fields:
  - `Name`, `Personality`, `PersonalityPromptRaw`, `PersonalityPromptSanitized`
  - `TwilioPhoneNumber`, `WhatsappNumber`, `TelegramBotToken`
  - `BuyAlan/Data/Entities/Agent.cs`
- Onboarding domain service already encodes important channel behavior:
  - trim/normalize optional channels
  - at least one channel required
  - telegram token uniqueness checks
  - telegram webhook registration on token change with rollback on error
  - `BuyAlan/Onboarding/SubscriptionOnboardingService.cs`

## Contract/Integration Notes
- Adding `/agents` endpoints changes WebAPI interface and OpenAPI output.
- Per AGENTS.md: after WebAPI interface changes, hand off for developer-run `yarn openapi-ts`.
- Do not edit `swagger.json` or `.gen.ts` files manually.

## Security/Privacy Notes
- Raw telegram token exposure is intentionally accepted for this milestone.
- Must avoid token value leakage in logs/UI telemetry.
- Maintain strict subscription membership checks on every `/agents` operation.

## Suggested Execution Order
1. Backend `/agents` endpoints + DTOs + authorization + domain-consistent update behavior.
2. Update OpenAPI and hand off for WebApp client regeneration.
3. Implement WebApp agent settings provider in agent layout scope.
4. Implement Personality form (onboarding-style validation).
5. Implement Channels form with per-card save and shared cross-field validation.
6. Run/expand tests for endpoint behavior and settings flows.
