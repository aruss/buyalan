namespace HeyAlan.Data.Entities;

using HeyAlan;

public class ConversationMessage : IEntityWithId, IEntityWithAudit
{
    public Guid Id { get; set; }

    public Guid ConversationId { get; set; }

    public Conversation Conversation { get; set; } = null!;

    public Guid AgentId { get; set; }

    public Agent Agent { get; set; } = null!;

    public MessageRole Role { get; set; }

    public string Content { get; set; } = String.Empty;

    public string From { get; set; } = String.Empty;

    public string To { get; set; } = String.Empty;

    public DateTimeOffset OccurredAt { get; set; }

    public bool IsRead { get; set; }

    public DateTimeOffset? ReadAt { get; set; }

    public DateTime CreatedAt { get; set; }

    public DateTime UpdatedAt { get; set; }
}
