# M5 - Telegram Outgoing Delivery Pipeline

## Summary
Implement end-to-end Telegram message delivery across webhook ingestion, message contracts, queue routing, and outbound dispatch.

Incoming Telegram webhooks must resolve the owning `Agent` by bot token, enrich `IncomingMessage` with `AgentId` and `SubscribtionId`, emit a placeholder business reply, and dispatch through `OutgoingTelegramMessageConsumer` using `ITelegramService`.

## Gate A - Message Contract and Telegram Service API
- [ ] Extend `IncomingMessage` with required `Guid AgentId`.
- [ ] Define `OutgoingTelegramMessage` with required fields: `SubscribtionId`, `AgentId`, `Content`, `To`.
- [ ] Add `SendMessageAsync(string botToken, long chatId, string text, CancellationToken ct = default)` to `ITelegramService`.
- [ ] Implement `SendMessageAsync` in `TelegramService` using `TelegramClientFactory.GetClient(botToken)` and Telegram.Bot send API.

## Gate B - Telegram Webhook Ingestion Enrichment
- [ ] Update `SquareBuddy.WebApi/TelegramIntegration/TelegramWebhookEndpoints.cs` to inject `MainDataContext`.
- [ ] Resolve `Agent` by exact `TelegramBotToken == botToken`.
- [ ] Return `NotFound` when no matching agent exists.
- [ ] Publish `IncomingMessage` with populated `SubscribtionId` and `AgentId` from resolved agent.
- [ ] Preserve current behavior for non-text updates (`Ok` without publish).

## Gate C - Incoming Routing and Placeholder Business Reply
- [ ] In `IncomingMessageConsumer`, keep inbound logging and include `AgentId` in logs.
- [ ] Add deterministic placeholder reply text for current mock business logic.
- [ ] For `MessageChannel.Telegram`, publish `OutgoingTelegramMessage` (not `IncomingMessage`).
- [ ] Set outgoing recipient to incoming Telegram sender id (`message.From`).

## Gate D - Outgoing Telegram Consumer Delivery
- [ ] Complete `OutgoingTelegramMessageConsumer` consume logic.
- [ ] Inject and use `MainDataContext` to load `Agent` by `AgentId`.
- [ ] Validate agent existence and non-empty `TelegramBotToken`; fault when invalid.
- [ ] Parse outgoing `To` into `long chatId`; fault on parse errors.
- [ ] Send message via `ITelegramService.SendMessageAsync` using consume cancellation token.

## Gate E - MassTransit Topology and Registrations
- [ ] Register `OutgoingTelegramMessageConsumer` in `SquareBuddy.WebApi/Infrastructure/MassTransitBuilderExtensions.cs`.
- [ ] Register `OutgoingTelegramMessageConsumer` in `SquareBuddy.Initializer/Program.cs` topology deployment section.
- [ ] Keep endpoint auto-configuration via `cfg.ConfigureEndpoints(context)`.

## Gate F - Channel Compatibility and Current Placeholders
- [ ] Update `TwilioWebhookEndpoints` to set required `AgentId` placeholder (same temporary style as `SubscribtionId`).
- [ ] Keep current Twilio behavior otherwise unchanged.

## Gate G - Verification
- [ ] Test: Telegram webhook publishes enriched `IncomingMessage` with resolved `AgentId` and `SubscribtionId`.
- [ ] Test: Telegram webhook returns `404` for unknown bot token.
- [ ] Test: Telegram webhook non-text updates return `200` without publish.
- [ ] Test: `IncomingMessageConsumer` emits one `OutgoingTelegramMessage` with fixed placeholder content for Telegram channel.
- [ ] Test: `OutgoingTelegramMessageConsumer` resolves agent token and calls `ITelegramService.SendMessageAsync`.
- [ ] Test: missing agent/token and invalid chat id paths fault as expected.
- [ ] Test: `TelegramService.SendMessageAsync` sends expected payload and propagates Telegram API failures.

## Assumptions and Defaults
- `SubscribtionId` spelling remains unchanged in this milestone for compatibility.
- `AgentId` is required and non-null on `IncomingMessage`.
- Outgoing bus messages do not carry Telegram bot token secrets.
- Unknown Telegram bot token returns HTTP `404`.
- Placeholder business response is a single fixed deterministic string.
