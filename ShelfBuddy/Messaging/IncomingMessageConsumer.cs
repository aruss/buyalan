namespace ShelfBuddy.Messaging;

using MassTransit;
using Microsoft.Extensions.Logging;

public class IncomingMessageConsumer : IConsumer<IncomingMessage>
{
    private const string PlaceholderReply = "Thanks, we got your message.";

    private readonly ILogger<IncomingMessageConsumer> logger;
    private readonly IPublishEndpoint publishEndpoint;
    private readonly IConversationStore conversationStore;

    public IncomingMessageConsumer(
        ILogger<IncomingMessageConsumer> logger,
        IPublishEndpoint publishEndpoint,
        IConversationStore conversationStore)
    {
        this.logger = logger ?? throw new ArgumentNullException(nameof(logger));
        this.publishEndpoint = publishEndpoint ?? throw new ArgumentNullException(nameof(publishEndpoint));
        this.conversationStore = conversationStore ?? throw new ArgumentNullException(nameof(conversationStore));
    }

    public async Task Consume(ConsumeContext<IncomingMessage> context)
    {
        IncomingMessage message = context.Message;

        this.logger.LogInformation(
            "Subscription {SubscriptionId} Agent {AgentId} received {Channel} message from {From}",
            message.SubscriptionId,
            message.AgentId,
            message.Channel,
            message.From);

        await this.conversationStore.UpsertIncomingMessageAsync(message, context.CancellationToken);

        if (message.Channel == MessageChannel.Telegram)
        {
            OutgoingTelegramMessage telegramMessage = new()
            {
                SubscriptionId = message.SubscriptionId,
                AgentId = message.AgentId,
                Content = PlaceholderReply,
                To = message.From
            };

            await this.publishEndpoint.Publish(telegramMessage, context.CancellationToken);
        }
    }
}
