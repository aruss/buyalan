namespace HeyAlan.Messaging;

public record IncomingMessage
{
    public Guid SubscriptionId { get; init; }

    public Guid AgentId { get; init; }

    public MessageChannel Channel { get; init; }

    public MessageRole Role { get; set; }

    public string Content { get; init; } = String.Empty;

    public string From { get; init; } = String.Empty;

    public string To { get; init; } = String.Empty;

    public DateTimeOffset ReceivedAt { get; init; }
}
