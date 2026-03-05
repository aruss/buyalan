# SendGrid Setup

## What is `SENDGRID_NEWSLETTER_LIST_ID`
It is the SendGrid Marketing Contacts list ID used for confirmed newsletter subscribers.

Set:
- `SENDGRID_API_KEY`
- `SENDGRID_NEWSLETTER_LIST_ID`
- `SENDGRID_NEWSLETTER_CONFIRM_TEMPLATE_ID`
- `SENDGRID_NEWSLETTER_FROM_EMAIL`
- `NEWSLETTER_CONFIRM_TOKEN_TTL_MINUTES` (optional, defaults to `1440`)

## Create a List in the UI
1. Open SendGrid.
2. Go to `Marketing` -> `Contacts`.
3. Create a new list (example: `HeyAlan Newsletter`).
4. Save.

## Create a List via API
```bash
curl -X POST "https://api.sendgrid.com/v3/marketing/lists" \
  -H "Authorization: Bearer $SENDGRID_API_KEY" \
  -H "Content-Type: application/json" \
  -d "{\"name\":\"HeyAlan Newsletter\"}"
```

The response contains the new list `id`.

## Get List ID
```bash
curl -X GET "https://api.sendgrid.com/v3/marketing/lists" \
  -H "Authorization: Bearer $SENDGRID_API_KEY"
```

Find your list by `name` and use the matching `id` as `SENDGRID_NEWSLETTER_LIST_ID`.

## Configure Confirmation Template
Create a dynamic transactional template in SendGrid with a variable named `confirmation_url`.

Use the template ID (`d-...`) as `SENDGRID_NEWSLETTER_CONFIRM_TEMPLATE_ID`.

Example template body link:
```html
<a href="{{confirmation_url}}">Confirm newsletter subscription</a>
```

## Multi-App Setup
If one SendGrid account is shared by multiple apps, create one list per app (for example `HeyAlan Newsletter`, `Foo Newsletter`) and set each app to its own list ID.
