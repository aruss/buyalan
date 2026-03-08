# Milestone M21: Square Service Consolidation

## Summary
Consolidate Square integration logic in `HeyAlan` and `HeyAlan.WebApi` into one domain service (`ISquareService` / `SquareService`) so outbound Square communication, token lifecycle, and connect/disconnect orchestration are owned by a single implementation.

This milestone explicitly excludes Microsoft Identity Square authorization provider behavior.

## Findings Baseline (Current State Inventory)
- [x] `HeyAlan.WebApi/SquareIntegration/SquareConnectionEndpoints.cs` does not call Square directly; it delegates to `ISubscriptionSquareConnectionService`.
- [x] Outbound Square communication for subscription connection flow currently happens only in:
  - [x] `HeyAlan/SquareIntegration/SquareOAuthClient.cs`
  - [x] `HeyAlan/SquareIntegration/SquareTokenService.cs`
  - [x] `HeyAlan/SquareIntegration/SubscriptionSquareConnectionService.cs` (authorize URL construction)
- [x] Current Square operations observed:
  - [x] OAuth authorize URL construction
  - [x] OAuth code exchange
  - [x] Token status fallback (scope resolution)
  - [x] Token refresh
  - [x] Token revoke
  - [x] Token persistence/encryption/decryption
- [x] Shared scope/base-url logic is duplicated today across Square services and onboarding checks.
- [x] `HeyAlan/Identity/IdentityBuilderExtensions.cs` contains Square OAuth provider code for Microsoft Identity login and is out of scope for this milestone.
- [x] No additional direct Square SDK/API usage found in `./HeyAlan` and `./HeyAlan.WebApi` outside the files above.

## User Decisions (Locked)
- [x] Consolidation mode: **Full Merge**.
- [x] Keep existing WebApi endpoint contracts unchanged.
- [x] Keep existing error code vocabulary unchanged.
- [x] Keep Identity provider Square auth code out of scope.

## Gate A - Single Square Service Contract
- [ ] Introduce `ISquareService` as the only Square integration interface used by WebApi handlers.
- [ ] Move connect/disconnect orchestration methods under `ISquareService`.
- [ ] Move token lifecycle operations (store, resolve, refresh) under `ISquareService`.
- [ ] Move outbound Square OAuth operations (exchange, status fallback, revoke, refresh) under `ISquareService`.
- [ ] Register only the consolidated service in DI for Square integration behavior.

### Gate A Acceptance Criteria
- [ ] `SquareConnectionEndpoints` depends on `ISquareService` only.
- [ ] No remaining runtime dependency on `ISquareOAuthClient`, `ISquareTokenService`, or `ISubscriptionSquareConnectionService`.
- [ ] One service boundary owns all Square API communication (excluding Identity provider auth flow).

## Gate B - Consolidate Shared Square Rules
- [ ] Centralize required Square scopes in one place and reuse in connect + onboarding completion checks.
- [ ] Centralize sandbox/production base URL resolution in one place.
- [ ] Centralize callback path usage for connect callback handling.
- [ ] Remove duplicated helper code for scope parsing/normalization and expiry parsing where possible.

### Gate B Acceptance Criteria
- [ ] Scope requirements used for connect completion and onboarding progression come from a single source.
- [ ] No duplicated Square base URL construction remains across consolidated flow.

## Gate C - WebApi Integration Refactor
- [ ] Refactor `SquareConnectionEndpoints` to call `ISquareService`.
- [ ] Keep all existing endpoint routes unchanged:
  - [ ] `POST /subscriptions/{subscriptionId}/square/authorize`
  - [ ] `GET /subscriptions/square/callback`
  - [ ] `DELETE /subscriptions/{subscriptionId}/square/connection`
- [ ] Keep existing DTO request/response payload shapes unchanged.
- [ ] Keep existing error-code-to-status-code mapping unchanged.

### Gate C Acceptance Criteria
- [ ] OpenAPI-facing shape for Square connection endpoints is unchanged.
- [ ] Existing frontend call expectations remain compatible.

## Gate D - Regression and Behavior Tests
- [ ] Add unit tests for consolidated service:
  - [ ] Start connect URL generation (sandbox/prod) and returnUrl validation.
  - [ ] Complete connect: invalid state, denied OAuth, missing code, missing scopes, exchange failure.
  - [ ] Token lifecycle: valid token reuse, decrypt failure, refresh success, refresh reconnect-required, refresh failure.
  - [ ] Disconnect: missing connection, revoke success, already revoked, revoke failure.
- [ ] Add/update endpoint-level tests for `SquareConnectionEndpoints` to verify status and payload mapping are unchanged.
- [ ] Keep onboarding recompute behavior validated after connect/disconnect.

### Gate D Acceptance Criteria
- [ ] Tests cover all critical success/failure branches above.
- [ ] No behavior regressions in Square connect/disconnect API flow.

## Gate E - Cleanup and Housekeeping
- [ ] Remove obsolete Square service interfaces/classes after call sites migrate.
- [ ] Remove unused named HTTP client registrations if not needed by consolidated service.
- [ ] Confirm no logs expose access tokens, refresh tokens, or PII.
- [ ] Confirm no changes are made to Identity Square auth provider flow.

### Gate E Acceptance Criteria
- [ ] Only consolidated Square service remains for subscription Square integration.
- [ ] Security posture is preserved (least privilege, no secret leakage in logs).

## Implementation Sequence (Handoff Order)
- [ ] 1) Introduce `ISquareService` contract with existing connect/disconnect method signatures first.
- [ ] 2) Implement `SquareService` by moving logic from:
  - [ ] `SubscriptionSquareConnectionService` (workflow/orchestration)
  - [ ] `SquareOAuthClient` (exchange/revoke/token-status)
  - [ ] `SquareTokenService` (store/resolve/refresh lifecycle)
- [ ] 3) Switch DI registrations and endpoint dependency from `ISubscriptionSquareConnectionService` to `ISquareService`.
- [ ] 4) Centralize shared constants/helpers (required scopes, base URL resolution, callback path, scope normalization).
- [ ] 5) Remove obsolete interfaces/classes only after all call sites compile and tests pass.
- [ ] 6) Run regression tests for endpoint mapping + service behavior + onboarding recompute interactions.

## Notes
- Database schema changes are not planned in this milestone.
- If any schema change is introduced unexpectedly, stop and hand off for migration creation per repository rule.
- Existing endpoint routes and DTO contracts must remain stable throughout this milestone.
