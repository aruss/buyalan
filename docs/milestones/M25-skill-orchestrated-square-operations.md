# Milestone M25: Skill-Orchestrated Square Operations (API-First)

## Summary
Build the skill-system layer that routes Square operational behavior through skills/tools while keeping LLM chat orchestration out of scope.

This milestone extends M24 and aligns to M23 catalog-cache rules:
- all product lookup tools read from local catalog cache only,
- state-changing Square actions require a two-step `prepare -> confirm` flow,
- write tools require idempotency and auditable execution envelopes,
- checkout/session completeness orchestration remains outside the skill system.

## Dependencies and Preconditions
- [ ] M23 runtime catalog read path is available (`ISubscriptionCatalogReadService`) with agent and zip filtering semantics.
- [ ] M24 core skill mechanics and credential consolidation baseline are available.
- [ ] Existing Square token lifecycle remains provided by `ISquareService`.

## User Decisions (Locked)
- [x] This is a new follow-up milestone (not an M24 expansion).
- [x] Checkout/session completion state machine is outside this milestone's skill-system scope.
- [x] State-changing Square tools use mandatory two-step confirmation (`prepare -> confirm`).
- [x] v1 tool set includes:
  - [x] `catalog_search`
  - [x] `checkout_validate`
  - [x] `order_prepare`
  - [x] `order_confirm_create`
  - [x] `payment_link_create`
  - [x] `order_status_get`
- [x] Catalog lookup source policy is cache-only (no direct Square catalog calls in skill execution path).
- [x] Catalog lookup tool returns freshness metadata.

## Public API and Contract Changes
- [ ] Extend skill descriptor contract returned by existing skill endpoints to include execution policy metadata:
  - [ ] `accessMode` (`read_only` | `state_changing`)
  - [ ] `requiresConfirmation`
  - [ ] `requiresIdempotency`
  - [ ] dependency/readiness metadata (`catalog_cache_required`, `square_connection_required`)
- [ ] Define stable skill input/output schema contracts for all v1 tools.
- [ ] Keep deterministic error code surfaces for tool-level validation and execution failures.
- [ ] Keep external chat endpoints unchanged in this milestone.
- [ ] Optional (read-only) diagnostics API may be added for recent skill invocation audits if needed for operations.

## Gate A - Skill Execution Policy and Contract Foundation
- [ ] Extend `ISkillDefinitionRegistry` metadata model for execution safety policy:
  - [ ] access mode (`read_only`, `state_changing`)
  - [ ] confirmation requirement
  - [ ] idempotency requirement
  - [ ] dependency requirement metadata
- [ ] Add canonical schema contracts for tool input/output and validation hooks.
- [ ] Define deterministic skill error codes:
  - [ ] validation errors (input/schema/dependency not ready)
  - [ ] policy errors (confirmation/idempotency missing or invalid)
  - [ ] provider errors (Square operation failure categories)

### Gate A Acceptance Criteria
- [ ] Every v1 tool has a machine-readable policy and I/O contract.
- [ ] Deterministic error code mapping exists for validation, policy, and provider failure classes.

## Gate B - Internal Skill Execution Boundary and Safety Guards
- [ ] Introduce `IAgentSkillExecutionService` (internal) with execution envelope:
  - [ ] `subscriptionId`, `agentId`, `checkoutContextId`, `correlationId`
  - [ ] `idempotencyKey` (required for writes)
  - [ ] `confirmationToken` (required for confirm step)
  - [ ] typed `toolInput`
- [ ] Introduce confirmation guard service:
  - [ ] issue confirmation token during `order_prepare`
  - [ ] verify token during confirm execution
  - [ ] enforce expiry, scope binding, and anti-replay
- [ ] Persist skill invocation audit records with redaction:
  - [ ] request/response metadata
  - [ ] outcome/error code
  - [ ] timing and correlation identifiers
- [ ] Ensure no secret/token material is persisted in audit payloads.

### Gate B Acceptance Criteria
- [ ] Writes are blocked unless confirmation and idempotency policies are satisfied.
- [ ] Confirm tokens are one-time use, scoped, and replay-safe.
- [ ] Invocation audit records are queryable and secret-safe.

## Gate C - Catalog and Validation Read Skills (M23-Aligned)
- [ ] Implement `catalog_search` skill:
  - [ ] reads via `ISubscriptionCatalogReadService` only
  - [ ] applies agent product-access and zip-allowlist semantics
  - [ ] returns freshness metadata (`lastSyncAtUtc`, `syncStatus`, staleness indicator)
- [ ] Implement `checkout_validate` skill:
  - [ ] validates required checkout/order fields from provided context payload
  - [ ] returns deterministic missing-field and invalid-field results
  - [ ] does not persist or own checkout state machine
- [ ] Add dependency readiness validation:
  - [ ] fail `catalog_search` with deterministic code when catalog cache is unavailable
  - [ ] fail Square-dependent skills when Square connection is unavailable

### Gate C Acceptance Criteria
- [ ] No direct Square catalog API calls occur in skill execution path.
- [ ] Catalog reads honor M23 filtering semantics.
- [ ] Validation output is deterministic and machine-consumable.

## Gate D - Square Write and Status Skills
- [ ] Implement `order_prepare` skill:
  - [ ] performs write preflight and returns draft summary plus confirmation token
  - [ ] does not create order
- [ ] Implement `order_confirm_create` skill:
  - [ ] requires valid confirmation token
  - [ ] requires idempotency key
  - [ ] creates Square order and returns stable order result contract
- [ ] Implement `payment_link_create` skill:
  - [ ] requires write policy checks (confirmation + idempotency)
  - [ ] returns payment link contract and identifiers
- [ ] Implement `order_status_get` skill:
  - [ ] read-only status retrieval contract with deterministic not-found/error mapping
- [ ] Route operational Square calls through dedicated operation service(s), while token lifecycle continues through `ISquareService`.

### Gate D Acceptance Criteria
- [ ] State-changing operations are only executable through confirm path.
- [ ] Idempotent retries do not duplicate Square side effects.
- [ ] Status retrieval is read-only and contract-stable.

## Gate E - Observability, Security, and Regression Coverage
- [ ] Unit tests:
  - [ ] policy enforcement for read/write/confirm/idempotency rules
  - [ ] token issuance/verification/expiry/replay behavior
  - [ ] contract validation and error mapping behavior
- [ ] Integration tests:
  - [ ] `catalog_search` cache-only behavior and freshness metadata output
  - [ ] `order_prepare -> order_confirm_create` success and failure paths
  - [ ] `payment_link_create` safety path
  - [ ] `order_status_get` success/not-found paths
- [ ] Regression tests:
  - [ ] existing Square connect/disconnect/token lifecycle behavior unchanged
  - [ ] M23 catalog sync/read behavior unchanged
- [ ] Security checks:
  - [ ] no secret/token leakage in logs
  - [ ] no secret/token leakage in audit payload storage

### Gate E Acceptance Criteria
- [ ] Critical safety and execution branches are covered by tests.
- [ ] No behavioral regression in existing Square integration and catalog cache behavior.

## Implementation Sequence (Context-Window Friendly)
- [ ] 1) Gate A: policy + contract model foundation.
- [ ] 2) Gate B: execution boundary, confirmation guard, and auditing.
- [ ] 3) Gate C: cache-backed read/validation skills.
- [ ] 4) Gate D: write/status Square skills with safety controls.
- [ ] 5) Gate E: tests, security checks, and regression verification.

## Handoff and Operational Notes
- [ ] If schema changes are required for audit or policy persistence, stop and hand off for migration generation/run from `HeyAlan.Initializer` per repo rule.
- [ ] If WebAPI interface changes impact generated client contracts, hand off for `yarn openapi-ts`.
- [ ] UI/chat-flow implementation is intentionally out of scope for this milestone.

## Out of Scope
- [ ] LLM prompt strategy, conversation policy, and response generation logic.
- [ ] Checkout/session state machine ownership and persistence.
- [ ] Frontend UX for checkout or order-state interactions.
