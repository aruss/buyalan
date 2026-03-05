# M17 - Newsletter Double Opt-In via SendGrid + In-App Confirmation

## Summary
Implement a public newsletter DOI flow from the landing footer:
- WebApp footer validates email and submits to WebAPI.
- WebAPI enqueues a newsletter subscription message to RabbitMQ via Wolverine.
- Consumer sends a confirmation email using a SendGrid transactional template with a signed token link.
- User opens `/newsletter/confirm?token=...`; the confirm page calls API, API validates token, then upserts contact to the real SendGrid newsletter list.
- Only confirmed users are added to the newsletter audience.

## Scope
- Keep anonymous newsletter subscribe endpoint (`POST /newsletter/subscriptions`).
- Add signed stateless confirmation token service.
- Add SendGrid transactional confirmation email send capability.
- Add newsletter confirmation endpoint (`POST /newsletter/confirmations`).
- Add landing confirmation page with thank-you message + redirect to `/`.

## Non-Goals
- Creating a pending/unconfirmed SendGrid list.
- Adding persistent newsletter subscription state to the project database.
- Implementing unsubscribe management in-app.
- Editing generated OpenAPI client files directly.

## Findings
### Repo Findings
- Existing queue and consumer routing via Wolverine can be reused as-is.
- Existing `AppOptions.PublicBaseUrl` is available for confirmation-link generation.
- Existing footer subscription UX and cookie behavior can be reused.
- Existing Data Protection setup is already configured in WebAPI.

### External Findings (SendGrid)
- Contacts upsert remains `PUT /v3/marketing/contacts`.
- Confirmation email is implemented via `POST /v3/mail/send` with a dynamic template.
- No single SendGrid API flag exists for `double_opt_in=true`; DOI is workflow/design based.

## User Decisions (Locked)
- [x] DOI mode: app-sent transactional template email.
- [x] Token mode: signed stateless token (no DB state).
- [x] Pending list: not used.
- [x] Invalid/expired token UX: same generic thank-you experience.

## Planned API and Contracts
- [x] Existing endpoint kept:
  - `POST /newsletter/subscriptions`
- [x] New endpoint added:
  - `POST /newsletter/confirmations`
- [x] New request DTO:
  - `ConfirmNewsletterSubscriptionInput` (`token`)
- [x] New response DTO:
  - `ConfirmNewsletterSubscriptionResult` (`accepted`)

## Configuration Additions
- [x] Required env vars:
  - `SENDGRID_API_KEY`
  - `SENDGRID_NEWSLETTER_LIST_ID`
  - `SENDGRID_NEWSLETTER_CONFIRM_TEMPLATE_ID`
  - `SENDGRID_NEWSLETTER_FROM_EMAIL`
- [x] Optional env var:
  - `NEWSLETTER_CONFIRM_TOKEN_TTL_MINUTES` (default `1440`)
- [x] Startup fails fast on missing/invalid values.

## Gate A - Subscription Intake
- [x] Keep existing anonymous subscribe endpoint and email validation.
- [x] Keep queue publish behavior for newsletter requests.
- [x] Keep generic response (no account/contact existence disclosure).

### Gate A Acceptance Criteria
- [ ] Valid email requests enqueue successfully and return accepted response.
- [ ] Invalid email requests fail validation with 400 semantics.

## Gate B - Confirmation Email Dispatch
- [x] Add token generation service (Data Protection + TTL validation).
- [x] Consumer generates signed confirmation token and URL based on `PUBLIC_BASE_URL`.
- [x] Consumer sends SendGrid transactional email with dynamic `confirmation_url`.
- [x] Avoid logging raw email/PII in logs.

### Gate B Acceptance Criteria
- [ ] Queue receives newsletter requests.
- [ ] Consumer dispatches confirmation email with tokenized link.
- [ ] SendGrid failures remain retriable through consumer failure behavior.

## Gate C - Confirmation API
- [x] Add anonymous `POST /newsletter/confirmations`.
- [x] Validate token and extract email.
- [x] On valid token, upsert contact to real newsletter list.
- [x] On invalid/expired token, return generic success without upsert.

### Gate C Acceptance Criteria
- [ ] Valid token confirms subscription into the real list.
- [ ] Invalid/expired token does not disclose state.

## Gate D - WebApp Confirmation UX
- [x] Add `/newsletter/confirm` page.
- [x] Page calls confirmation API internally when `token` exists.
- [x] Show thank-you message and redirect to landing page.
- [x] Preserve footer confirmation cookie behavior.

### Gate D Acceptance Criteria
- [ ] Confirmation page shows thank-you state for valid and invalid tokens.
- [ ] Redirect to `/` occurs after confirmation attempt.

## Gate E - Verification
- [x] Unit tests: SendGrid options validation (new keys + token TTL).
- [x] Unit tests: SendGrid client payload for upsert and confirmation email.
- [x] Unit tests: newsletter consumer behavior (DOI email dispatch).
- [x] Unit tests: token service validity/expiry behavior.
- [x] Frontend test: confirmation page posts token and redirects.
- [ ] Manual E2E: submit -> email link -> confirm endpoint -> SendGrid list upsert.

### Gate E Acceptance Criteria
- [ ] End-to-end DOI path works from footer submit to confirmed list upsert.
- [ ] No raw email addresses are emitted in logs.

## Handoffs and Repo Rules
- [ ] After WebAPI interface change updates OpenAPI, hand off for developer-run client generation:
  - `yarn openapi-ts`
- [x] Do not edit `swagger.json` or `.gen.ts` files manually.

## Risks and Notes
- SendGrid template must contain `{{confirmation_url}}`.
- Generic success on invalid/expired token prioritizes privacy over explicit user error feedback.
- Public subscribe endpoint remains abuse-prone; rate limiting can be added as follow-up.
