# Milestone M16: OAuth Provider Callback URL Canonicalization

## Goal
Ensure Google and Square external auth in `BuyAlan/Identity/IdentityBuilderExtensions.cs` always send the correct externally reachable callback URL to providers when WebApi is proxied by WebApp and when deployed behind ingress.

Provider-facing callback URL MUST be built from:

- `PUBLIC_BASE_URL`
- proxy prefix `/api`
- provider callback path `/auth/providers/{provider}/callback`

So provider redirect URI is:

- `<PUBLIC_BASE_URL>/api/auth/providers/google/callback`
- `<PUBLIC_BASE_URL>/api/auth/providers/square/callback`

## Implementation
- [x] Keep internal middleware callback paths unchanged (`/auth/providers/google/callback`, `/auth/providers/square/callback`).
- [x] Add shared helper to build absolute auth callback URLs from `AppOptions.PublicBaseUrl` + `/api` + callback path.
- [x] Rewrite outbound Google authorization redirect to force canonical `redirect_uri`.
- [x] Rewrite outbound Square authorization redirect to force canonical `redirect_uri` while preserving production `session=false`.
- [x] Keep `/auth/external-callback` completion flow unchanged.

## Tests
- [x] Add unit tests for callback URL builder with trailing slash and base-path variants.
- [x] Add unit test verifying query parameter replacement rewrites `redirect_uri` without losing other parameters.
- [ ] Run `BuyAlan.Tests` and confirm no regressions.

## Docs
- [x] Update setup docs so provider console callback config uses `/api/auth/providers/.../callback`.

## Acceptance Criteria
- [ ] Google auth provider receives `redirect_uri=<PUBLIC_BASE_URL>/api/auth/providers/google/callback`.
- [ ] Square auth provider receives `redirect_uri=<PUBLIC_BASE_URL>/api/auth/providers/square/callback`.
- [ ] Existing callback handlers and post-login behavior remain unchanged.
