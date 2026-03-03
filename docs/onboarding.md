# Onboarding Use Cases (User Perspective)

## 1. New user starts onboarding
- After login, a user who is not onboarded is routed to onboarding.
- The first step is connecting Square.

## 2. User connects Square immediately
- User connects Square first.
- User continues through profile, channels, optional team invites, and finalize.
- On completion, onboarding is marked complete and user continues in Admin.

## 3. User skips Square for now
- In the same onboarding session, user can skip Square and continue.
- User can still complete profile and channels to prepare setup.

## 4. User pauses onboarding and resumes later from Admin
- User clicks **Complete onboarding** from the Admin reminder.
- If Square is still missing, onboarding starts again from Square.
- Previously entered profile/channel values are prefilled so user can review or edit.

## 5. Profile setup
- User sets agent name and personality.
- On later resume, profile is available with prefilled values and can be changed.

## 6. Channel setup
- User configures at least one channel (Telegram, SMS/phone, WhatsApp).
- Saved Telegram token is handled safely and not exposed as raw prefilled secret.

## 7. Invitations behavior
- Team invite step is available only when prerequisites are met (especially Square).
- If Square is missing, onboarding avoids blocked invitation/finalize calls and sends user back to Admin.

## 8. Finalize onboarding
- Finalize succeeds only when required steps are complete, including Square.
- If requirements are missing, user receives deterministic validation errors.

## 9. Already onboarded user
- If an onboarded user opens onboarding routes, they are redirected to Admin.
- Admin reminder to proceed onboarding is shown only while onboarding is incomplete.
