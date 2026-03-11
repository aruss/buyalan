# Milestone M18: Single Square App via Callback Broker

## Summary
Refactor Square OAuth so both external authentication and subscription connection use one Square app (`SQUARE_CLIENT_ID` / `SQUARE_CLIENT_SECRET`) and one registered redirect URL.

Canonical provider redirect URL:
- `<PUBLIC_BASE_URL>/api/subscriptions/square/callback`

Design:
- Keep Identity middleware callback path `/auth/providers/square/callback` unchanged internally.
- Use `/subscriptions/square/callback` as a broker callback endpoint:
  - If `state` is a valid Square connection state payload, complete subscription connect flow.
  - Otherwise, forward callback query to `/auth/providers/square/callback` so ASP.NET OAuth handler completes external login.

## User Decisions (Locked)
- [x] Use a single Square credential pair for both flows.
- [x] Canonical callback path is `/subscriptions/square/callback`.
- [x] Immediate removal of legacy callback behavior (no temporary aliases).
- [x] Hard switch config model: do not use `AUTH_SQUARE_CLIENT_ID` / `AUTH_SQUARE_CLIENT_SECRET`.

## API and Interface Changes
- [ ] Keep `POST /subscriptions/{subscriptionId}/square/authorize` unchanged for connect start.
- [ ] Keep `GET /subscriptions/square/callback` as canonical callback and implement broker behavior.
- [ ] Keep Identity callback path `/auth/providers/square/callback` registered in OAuth handler.
- [ ] No new public DTO contracts required.
- [ ] OpenAPI surface remains stable for auth endpoints; callback internals change only.

## Gate A - Credential and Configuration Unification
- [ ] Update identity Square provider setup to use `SQUARE_CLIENT_ID` / `SQUARE_CLIENT_SECRET`.
- [ ] Remove runtime dependency on `AUTH_SQUARE_CLIENT_ID` / `AUTH_SQUARE_CLIENT_SECRET`.
- [ ] Update `AppOptions` validation/documentation to reflect single credential model.
- [ ] Add startup validation that fails fast when required `SQUARE_*` values are missing.

### Gate A Acceptance Criteria
- [ ] Auth Square provider initializes from `SQUARE_*` only.
- [ ] Subscription connect flow still initializes from same `SQUARE_*`.
- [ ] No code path reads `AUTH_SQUARE_*`.

## Gate B - Callback Broker Routing
- [ ] Extend `/subscriptions/square/callback` handler to branch by state shape:
  - connect-state -> complete connect flow (existing behavior),
  - non-connect-state -> 302 forward to `/auth/providers/square/callback` with original query intact.
- [ ] Keep broker forward target fixed/internal to avoid open redirect risk.
- [ ] Ensure connect-state parse failure no longer forces onboarding error for auth callbacks.

### Gate B Acceptance Criteria
- [ ] Subscription connect callback still stores tokens and redirects to internal return URL.
- [ ] External login callback reaches Identity handler through broker forward.
- [ ] Query parameters (`code`, `state`, `error`, etc.) survive broker forwarding.

## Gate C - Identity Provider Redirect URI Canonicalization
- [ ] In Square auth provider `OnRedirectToAuthorizationEndpoint`, force `redirect_uri` to canonical broker callback (`/api/subscriptions/square/callback`).
- [ ] Keep existing production `session=false` behavior.
- [ ] Keep existing claim mapping and ticket creation behavior.

### Gate C Acceptance Criteria
- [ ] Outbound Square auth challenge uses canonical broker callback URI.
- [ ] Correlation/state validation in Identity handler remains functional post-forward.
- [ ] External login success/failure paths remain unchanged for user experience.

## Gate D - Tests
- [ ] Add tests for broker branching:
  - valid connect-state -> connect completion,
  - opaque/non-connect OAuth state -> forward to Identity callback.
- [ ] Add tests verifying forwarded callback preserves full query payload.
- [ ] Add tests verifying Square auth provider uses canonical broker `redirect_uri`.
- [ ] Keep/update existing connect flow tests for required internal return URL behavior.
- [ ] Run `BuyAlan.Tests` and verify no regressions.

### Gate D Acceptance Criteria
- [ ] Broker callback tests pass for both branches.
- [ ] External auth and connect flow tests both pass with single app config.
- [ ] No regression in ownership/security checks for connect flow.

## Gate E - Documentation and Handoff
- [ ] Update Square setup docs to a single Square app for auth + connect.
- [ ] Remove references to dual app requirement for Square.
- [ ] Document canonical callback URL and broker behavior for operators.
- [ ] If WebAPI OpenAPI changes impact WebApp client, hand off for `yarn openapi-ts`.

### Gate E Acceptance Criteria
- [ ] Docs match actual runtime configuration and callback behavior.
- [ ] Developer setup requires only one Square app for BuyAlan.

## Risks and Notes
- ASP.NET OAuth correlation/state checks are bound to Identity callback handling; broker MUST forward raw query unmodified.
- Broker MUST never forward to user-provided URLs.
- Immediate legacy removal requires Square dashboard redirect URI update before deploy cutover.
