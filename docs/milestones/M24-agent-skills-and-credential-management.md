# Milestone M24: Agent Skills and Credential Management (API-First)

## Summary
Build an API-first skills system where:
- skills are enabled/configured per agent,
- credentials are managed separately from skills and shared at subscription scope where applicable,
- `getEnabledSkills(agentId)` returns tool descriptors with resolved non-secret runtime config,
- Square and skills credentials are consolidated behind one credential manager abstraction,
- the credential model supports three classes:
  - system-managed runtime credentials (environment-backed, read-only),
  - subscription-managed API keys (BYOK by subscription owner),
  - subscription-managed OAuth credentials (owner consent + refresh token lifecycle),
- LLM tool execution remains out of scope.

## User Decisions (Locked)
- [x] Skill credentials and skill configuration are separated.
- [x] Credentials are shared at provider/account level and reusable across multiple skills.
- [x] Existing Square token management and skills-related credentials are consolidated under one credential manager abstraction.
- [x] `getEnabledSkills(agentId)` returns tool descriptors with resolved runtime config (no raw secrets).
- [x] Credential scope is subscription-level.
- [x] Credential create/update/delete permissions are subscription-owner only.
- [x] Credential read/list permissions are subscription-owner only.
- [x] OAuth credential subject is subscription owner only.
- [x] OAuth credential cardinality is single account per provider per subscription in v1 (`accountKey=default`).
- [x] System-managed credentials are configured via environment/runtime and are not user-editable at runtime.
- [x] UI for system-managed credentials is status-only (configured/not configured), no value exposure or edit actions.
- [x] Milestone scope is API only (no UI in this milestone).
- [x] v1 skill catalog must cover all three credential classes (system-managed, BYOK, OAuth-managed).

## Concrete v1 Use Cases (Reference)
- [x] Square access remains available through existing Square endpoint contracts while token lifecycle storage moves behind shared credential management.
- [x] Google Maps system-wide skill is enable-only at agent level and uses runtime environment credentials (status/readiness only; no user-editable secret).
- [x] Broader v1 architecture still supports BYOK and generic OAuth skill credentials without blocking the Square + system-managed Maps path.

## Public API and Contract Changes
- [ ] Add agent skills endpoints (subscription member access):
  - [ ] `GET /agents/{agentId}/skills` (list persisted agent skill state).
  - [ ] `PUT /agents/{agentId}/skills/{skillKey}` (upsert enable/config/reference).
  - [ ] `DELETE /agents/{agentId}/skills/{skillKey}` (disable/remove agent skill row).
  - [ ] `GET /agents/{agentId}/skills/enabled` (runtime-resolved tool descriptors).
- [ ] Add subscription credential endpoints (subscription owner access):
  - [ ] `GET /subscriptions/{subscriptionId}/credentials`
  - [ ] `PUT /subscriptions/{subscriptionId}/credentials/{provider}/{accountKey}`
  - [ ] `DELETE /subscriptions/{subscriptionId}/credentials/{provider}/{accountKey}`
- [ ] Add generic OAuth credential lifecycle endpoints (owner access) for non-Square providers:
  - [ ] `POST /subscriptions/{subscriptionId}/credentials/oauth/{provider}/authorize`
  - [ ] `GET /subscriptions/credentials/oauth/{provider}/callback`
  - [ ] `DELETE /subscriptions/{subscriptionId}/credentials/oauth/{provider}/connection`
- [ ] Define canonical default account key (`default`) when provider is single-account for a subscription.
- [ ] Keep secret material out of API responses. Return only masked credential metadata and references.
- [ ] Include credential source metadata in read contracts (`system_managed`, `subscription_api_key`, `subscription_oauth`) and readiness status.
- [ ] Do not expose create/update/delete API operations for system-managed credentials; expose readiness only.
- [ ] Follow existing DTO naming and endpoint response shape conventions (`*Input`, `*Result`, concrete list result types).

## Gate A - Persistence Model for Skills and Credentials
- [ ] Add `SubscriptionProviderCredential` persistence model:
  - [ ] Subscription-scoped credential ownership.
  - [ ] Provider + account identity metadata.
  - [ ] Credential source/type discriminator.
  - [ ] Encrypted secret payload at rest for BYOK and OAuth token material.
  - [ ] OAuth lifecycle fields (access token, refresh token, expiry, scopes, disconnected timestamp/status as required).
- [ ] Add `AgentSkill` persistence model:
  - [ ] Agent id + skill key + enabled flag.
  - [ ] Per-skill config payload.
  - [ ] Credential reference.
- [ ] Add constraints/indexes:
  - [ ] Unique skill row per `(agentId, skillKey)`.
  - [ ] Unique credential identity per `(subscriptionId, provider, accountKey)`.
- [ ] Ensure no plaintext token/key storage in DB fields.
- [ ] Add EF mappings in `MainDataContext` and relationship constraints.
- [ ] Stop and hand off for migration generation/run from `HeyAlan.Initializer` per repo rule.

### Gate A Acceptance Criteria
- [ ] Schema supports agent skill rows and subscription credentials with provider/account identity.
- [ ] DB-level constraints enforce one row per `(agentId, skillKey)` and one row per `(subscriptionId, provider, accountKey)`.
- [ ] Schema supports persisted OAuth lifecycle state (access/refresh/expiry/scopes) without plaintext storage.
- [ ] Gate ends at migration handoff; no service or API behavior required in this gate.

## Gate B - Credential Service and Credential APIs
- [ ] Introduce `IAccessCredentialService` as the shared credential boundary:
  - [ ] Upsert/get/remove credential metadata.
  - [ ] Resolve runtime credential across the three credential classes (system-managed, BYOK, OAuth-managed).
  - [ ] Decrypt runtime secret only for internal service usage.
  - [ ] Encrypt secret on write and return masked metadata on read.
  - [ ] Manage OAuth refresh lifecycle for OAuth-managed credentials.
- [ ] Implement owner-only authorization in credential endpoints:
  - [ ] Owner can list credentials.
  - [ ] Owner can upsert credential by `(provider, accountKey)`.
  - [ ] Owner can delete credential by `(provider, accountKey)`.
- [ ] Expose system-managed credential readiness as read-only metadata.
- [ ] Keep existing secret-logging protections; verify no new token/key exposure in endpoint paths.

### Gate B Acceptance Criteria
- [ ] Credential API contracts are functional and independently testable without skills or Square refactor.
- [ ] Owner-only authz and masked response behavior are enforced.
- [ ] OAuth-managed credentials support authorize/callback/connect/disconnect and refresh path behavior.
- [ ] System-managed credential entries are read-only and represented via readiness metadata only.
- [ ] No secret values appear in logs or transport payloads.

## Gate C - Square Integration Consolidation onto Credential Service
- [ ] Refactor `ISquareService` internals to read/write Square credentials via `IAccessCredentialService`.
- [ ] Preserve Square WebAPI endpoint routes and contracts:
  - [ ] `POST /subscriptions/{subscriptionId}/square/authorize`
  - [ ] `GET /subscriptions/square/callback`
  - [ ] `DELETE /subscriptions/{subscriptionId}/square/connection`
- [ ] Preserve existing Square error codes and status mapping.
- [ ] Keep onboarding recompute behavior unchanged after connect/disconnect.

### Gate C Acceptance Criteria
- [ ] Square connect/disconnect API behavior is externally unchanged.
- [ ] Square token lifecycle is persisted/resolved through credential service.
- [ ] No secrets are exposed in logs or API payloads.

## Gate D - Skills Domain Services and Registry
- [ ] Add `ISkillDefinitionRegistry` for canonical skill metadata and validation.
- [ ] Add `IAgentSkillService` for skill lifecycle and resolution:
  - [ ] List/upsert/disable skills per agent.
  - [ ] Resolve enabled skills into tool descriptors for runtime.
- [ ] Define skill-level credential policy (`none/system_managed`, `subscription_api_key`, `subscription_oauth`) in registry.
- [ ] Implement v1 skills to cover all credential classes:
  - [ ] one system-managed credential skill (environment-backed),
  - [ ] one subscription-managed API key (BYOK) skill,
  - [ ] one subscription-managed OAuth skill (Google Calendar or Gmail style).
- [ ] Define and validate runtime-safe config per skill.
- [ ] Validate required credential readiness before enabling skill.

### Gate D Acceptance Criteria
- [ ] Invalid or missing credential/config blocks enable with deterministic error codes.
- [ ] Service-level enabled-skill resolution returns only enabled + valid skills.
- [ ] Returned descriptor model is sufficient for downstream LLM tool wiring without exposing secrets.

## Gate E - Agent Skills Endpoints and Runtime Descriptor API
- [ ] Implement agent skill endpoints in `HeyAlan.WebApi`.
- [ ] Enforce authz boundaries:
  - [ ] Agent skills operations require subscription membership.
- [ ] Maintain deterministic error-code-to-status mapping consistent with existing API style.
- [ ] Wire `GET /agents/{agentId}/skills/enabled` to service-level runtime descriptor resolution.
- [ ] Keep secret material out of endpoint responses.

### Gate E Acceptance Criteria
- [ ] Members can manage skills for agents they can access.
- [ ] `GET /agents/{agentId}/skills/enabled` returns descriptor payloads for enabled/valid skills only.
- [ ] Descriptor output contains no raw API key.
- [ ] Endpoint responses follow established contract patterns.

## Gate F - Tests and Regression Coverage
- [ ] Unit tests:
  - [ ] Credential encryption/decryption and masking.
  - [ ] System-managed credential readiness resolution behavior.
  - [ ] OAuth lifecycle behavior (connect state, refresh, reconnect-required path).
  - [ ] Skill validation (missing credential, invalid config, disabled skill exclusion).
  - [ ] Authorization guards for owner/member constraints.
- [ ] Endpoint/integration tests:
  - [ ] Credentials CRUD behavior.
  - [ ] OAuth authorize/callback/disconnect behavior for generic OAuth credential flow.
  - [ ] Agent skill list/upsert/disable.
  - [ ] Enabled-skills descriptor shape and filtering.
- [ ] Regression tests:
  - [ ] Existing Square endpoints and behavior remain unchanged externally.
  - [ ] Existing agents/onboarding flows remain unchanged.

### Gate F Acceptance Criteria
- [ ] New skill/credential flows are covered by tests.
- [ ] No behavioral regression in existing Square integration contracts.

## Implementation Sequence (Context-Window Friendly)
- [ ] 1) Gate A: persistence-only and migration handoff.
- [ ] 2) Gate B: credential service + credential APIs.
- [ ] 3) Gate C: migrate Square internals onto credential service.
- [ ] 4) Gate D: skills domain services + registry + v1 three-class skill catalog.
- [ ] 5) Gate E: agent skills endpoints and enabled-descriptor API.
- [ ] 6) Gate F: tests and regression pass.

## Handoff and Operational Notes
- [ ] This milestone requires schema changes; after Gate A schema edits, stop and hand off for migration generation/run from `HeyAlan.Initializer` per repo rule.
- [ ] After WebApi interface changes are finalized, hand off for WebApp API client generation (`yarn openapi-ts`).
- [ ] UI work is intentionally out of scope for this milestone.
- [ ] LLM tool call execution is intentionally out of scope; this milestone provides enablement/config/descriptor contracts only.
