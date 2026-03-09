# Milestone M24: Agent Skills and Credential Management (API-First)

## Summary
Build an API-first skills system where:
- skills are enabled per agent,
- credentials are managed separately from skills and shared at subscription scope where applicable,
- `GET /agents/{agentId}/skills/enabled` returns runtime tool descriptors without raw secrets,
- Square credentials are consolidated behind a shared credential manager abstraction,
- v1 skills scope is a single system-managed skill: Google Maps address normalization,
- LLM tool execution remains out of scope.

## User Decisions (Locked)
- [x] Skill credentials and skill configuration are separated.
- [x] Existing Square token management and skills-related credentials are consolidated under one credential manager abstraction.
- [x] Milestone scope is API only (no UI in this milestone).
- [x] `GET /agents/{agentId}/skills` returns enabled skills only.
- [x] Add global skills catalog endpoint `GET /skills`.
- [x] `GET /skills` is available to authenticated users.
- [x] `PUT /agents/{agentId}/skills/{skillKey}` keeps future-proof body shape.
- [x] v1 Google Maps skill has no per-agent config behavior.
- [x] Enabling Google Maps skill requires system credential readiness (reject when not configured).
- [x] `GET /agents/{agentId}/skills/enabled` returns ready skills only.
- [x] v1 skill implementation scope is one system-managed skill only (Google Maps).
- [x] Square migration strategy is one-shot replace.

## Concrete v1 Use Cases (Reference)
- [x] Square access remains available through existing Square endpoint contracts while token lifecycle storage moves behind shared credential management.
- [x] Google Maps skill accepts an address string and returns a validated, normalized full address.
- [x] Google Maps credentials are environment/runtime managed and never user-editable via API.

## Public API and Contract Changes
- [ ] Add authenticated skills catalog endpoint:
  - [ ] `GET /skills` (list all supported skills + requirement metadata).
- [ ] Add agent skills endpoints (subscription member access):
  - [ ] `GET /agents/{agentId}/skills` (list enabled skills for agent).
  - [ ] `PUT /agents/{agentId}/skills/{skillKey}` (enable/upsert skill row; keep future-proof input shape).
  - [ ] `DELETE /agents/{agentId}/skills/{skillKey}` (disable/remove agent skill row).
  - [ ] `GET /agents/{agentId}/skills/enabled` (runtime-resolved descriptors for enabled and ready skills).
- [ ] Preserve existing Square endpoints and contracts:
  - [ ] `POST /subscriptions/{subscriptionId}/square/authorize`
  - [ ] `GET /subscriptions/square/callback`
  - [ ] `DELETE /subscriptions/{subscriptionId}/square/connection`
- [ ] Define canonical default account key (`default`) for single-account providers (Square in v1).
- [ ] Keep secret material out of all API responses and logs.
- [ ] Follow existing DTO naming and endpoint response shape conventions (`*Input`, `*Result`, concrete list result types).

## Gate A - Persistence Model and One-Shot Square Credential Migration
- [ ] Add `SubscriptionProviderCredential` persistence model:
  - [ ] Subscription-scoped credential ownership.
  - [ ] Provider + account identity metadata.
  - [ ] Credential source/type discriminator.
  - [ ] Encrypted secret payload at rest.
  - [ ] Lifecycle fields needed for OAuth token handling (Square path).
- [ ] Add `AgentSkill` persistence model:
  - [ ] Agent id + skill key + enabled flag.
  - [ ] Placeholder config payload for forward compatibility.
- [ ] Add constraints/indexes:
  - [ ] Unique skill row per `(agentId, skillKey)`.
  - [ ] Unique credential identity per `(subscriptionId, provider, accountKey)`.
- [ ] Add EF mappings in `MainDataContext` and relationship constraints.
- [ ] Implement one-shot data migration mapping existing `SubscriptionSquareConnection` token lifecycle data into `SubscriptionProviderCredential` rows.
- [ ] Stop and hand off for migration generation/run from `HeyAlan.Initializer` per repo rule.

### Gate A Acceptance Criteria
- [ ] Schema supports agent skill rows and provider/account credentials.
- [ ] DB-level constraints enforce one row per `(agentId, skillKey)` and one row per `(subscriptionId, provider, accountKey)`.
- [ ] Existing Square credential lifecycle data is migrated in one shot.
- [ ] No plaintext token/key storage is introduced.

## Gate B - Credential Service and Square Consolidation
- [ ] Introduce `IAccessCredentialService` as the shared credential boundary:
  - [ ] Upsert/get/remove provider credentials.
  - [ ] Resolve runtime credential for internal service usage.
  - [ ] Encrypt on write, decrypt only internally, return masked metadata when needed.
- [ ] Refactor `ISquareService` internals to use `IAccessCredentialService` for token lifecycle operations.
- [ ] Preserve Square endpoint behavior, status mapping, and error codes.
- [ ] Keep onboarding recompute behavior unchanged after connect/disconnect.
- [ ] Verify no token/key exposure in logs or API payloads.

### Gate B Acceptance Criteria
- [ ] Square connect/disconnect API behavior is externally unchanged.
- [ ] Square token lifecycle is persisted/resolved through credential service.
- [ ] No secrets are exposed in logs or transport payloads.

## Gate C - Skills Domain Services and Agent Skills APIs
- [ ] Add `ISkillDefinitionRegistry` for canonical skill metadata and validation.
- [ ] Add `IAgentSkillService` for skill lifecycle and resolution:
  - [ ] List enabled skills per agent.
  - [ ] Upsert/enable skill per agent.
  - [ ] Disable skill per agent.
  - [ ] Resolve enabled skills into runtime descriptors.
- [ ] Implement skills endpoints in `HeyAlan.WebApi`:
  - [ ] `GET /skills` for authenticated users.
  - [ ] `GET /agents/{agentId}/skills`
  - [ ] `PUT /agents/{agentId}/skills/{skillKey}`
  - [ ] `DELETE /agents/{agentId}/skills/{skillKey}`
  - [ ] `GET /agents/{agentId}/skills/enabled`
- [ ] Enforce authz boundaries:
  - [ ] Agent skill operations require subscription membership.
  - [ ] Skills catalog requires authentication.
- [ ] Maintain deterministic error-code-to-status mapping consistent with existing API style.

### Gate C Acceptance Criteria
- [ ] Members can manage skills for agents they can access.
- [ ] `GET /skills` returns catalog metadata and requirement metadata without secrets.
- [ ] `GET /agents/{agentId}/skills` returns enabled skills only.
- [ ] Endpoint responses follow established contract patterns.

## Gate D - Google Maps Skill (System-Managed)
- [ ] Register `google_maps_address_normalizer` in `ISkillDefinitionRegistry`.
- [ ] Define runtime contract:
  - [ ] Input: address string.
  - [ ] Output: validated, normalized full address.
- [ ] Define credential policy as system-managed (environment/runtime backed, read-only from API perspective).
- [ ] Implement readiness checks for Google Maps system credential.
- [ ] Enforce enable-time readiness:
  - [ ] `PUT /agents/{agentId}/skills/{skillKey}` fails with deterministic error when not configured.
- [ ] Wire runtime descriptor resolution:
  - [ ] `GET /agents/{agentId}/skills/enabled` returns Google Maps descriptor only when skill is enabled and ready.
  - [ ] Do not include not-ready skills in enabled descriptor output.

### Gate D Acceptance Criteria
- [ ] Google Maps skill can be enabled only when system credential is ready.
- [ ] Enabled descriptor output includes Google Maps only when enabled + ready.
- [ ] Descriptor and API payloads contain no secret material.
- [ ] Error behavior is deterministic for not-ready and invalid enable paths.

## Gate E - Tests and Regression Coverage
- [ ] Unit tests:
  - [ ] Credential encryption/decryption and masking behavior.
  - [ ] Google Maps readiness resolution behavior.
  - [ ] Skill enable validation and disabled-skill exclusion.
  - [ ] Authorization guards for member/owner constraints.
- [ ] Endpoint/integration tests:
  - [ ] `/skills` catalog behavior (authenticated access only).
  - [ ] Agent skill list/upsert/disable behavior.
  - [ ] Google Maps enable rejection when system credential not ready.
  - [ ] Enabled-skills descriptor filtering (enabled + ready only).
- [ ] Regression tests:
  - [ ] Existing Square endpoints and behavior remain unchanged externally.
  - [ ] Existing agents/onboarding flows remain unchanged.

### Gate E Acceptance Criteria
- [ ] New skill/credential flows are covered by tests.
- [ ] No behavioral regression in existing Square integration contracts.

## Implementation Sequence (Context-Window Friendly)
- [ ] 1) Gate A: persistence model + one-shot migration and migration handoff.
- [ ] 2) Gate B: credential service + Square consolidation.
- [ ] 3) Gate C: skills core services + skills APIs.
- [ ] 4) Gate D: Google Maps skill implementation.
- [ ] 5) Gate E: tests and regression pass.

## Handoff and Operational Notes
- [ ] This milestone requires schema changes; after Gate A schema edits, stop and hand off for migration generation/run from `HeyAlan.Initializer` per repo rule.
- [ ] After WebApi interface changes are finalized, hand off for WebApp API client generation (`yarn openapi-ts`).
- [ ] UI work is intentionally out of scope for this milestone.
- [ ] LLM tool call execution is intentionally out of scope; this milestone provides enablement/config/descriptor contracts only.
