namespace SquareBuddy.Consumers;

using MassTransit;
using MassTransit.Transports;
using Microsoft.Extensions.Logging;
using OpenTelemetry;
using SquareBuddy.TelegramIntegration;

public record IncomingMessage
{
    public Guid SubscribtionId { get; init; }

    public MessageChannel Channel { get; init; }

    public MessageRole Role { get; set; }

    public string Content { get; init; } = string.Empty;

    public string From { get; init; } = string.Empty;

    public string To { get; init; } = string.Empty;

    public DateTimeOffset ReceivedAt { get; init; }
}

public class IncomingMessageConsumer : IConsumer<IncomingMessage>
{
    private readonly ILogger<IncomingMessageConsumer> logger;
    private readonly IPublishEndpoint publishEndpoint;

    public IncomingMessageConsumer(
        ILogger<IncomingMessageConsumer> logger,
        IPublishEndpoint publishEndpoint)
    {
        this.logger = logger;
        this.publishEndpoint = publishEndpoint; 
    }

    public async Task Consume(ConsumeContext<IncomingMessage> context)
    {
        var message = context.Message;

        this.logger.LogInformation(
            "Subscribtion {SubscribtionId} received {Channel} message from {From}",
            message.SubscribtionId, message.Channel, message.From);

        // Processes the message here ...

        // Pass the cancelation token here 

        if (message.Channel == MessageChannel.Telegram)
        {
            var telegramMessage = new OutgoingTelegramMessage
            {

            };

            await publishEndpoint.Publish(message);
        }
    }
}

public record OutgoingTelegramMessage
{

}


public class OutgoingTelegramMessageConsumer : IConsumer<OutgoingTelegramMessage>
{
    private readonly ILogger<OutgoingTelegramMessageConsumer> logger;
    private readonly IPublishEndpoint publishEndpoint;
    private readonly ITelegramService telegramService; 

    public OutgoingTelegramMessageConsumer(
        ILogger<OutgoingTelegramMessageConsumer> logger,
        ITelegramService telegramService,
        IPublishEndpoint publishEndpoint)
    {
        this.logger = logger;
        this.telegramService = telegramService;
        this.publishEndpoint = publishEndpoint; 
    }

    public async Task Consume(ConsumeContext<OutgoingTelegramMessage> context)
    {

    }
}