namespace BuyAlan.WebApi.Core;

using BuyAlan;
using BuyAlan.Extensions;
public record ConversationListItem(
    Guid ConversationId,
    string ParticipantExternalId,
    MessageChannel Channel,
    string? LastMessagePreview,
    DateTimeOffset? LastMessageAt,
    MessageRole? LastMessageRole,
    int UnreadCount,
    bool HasUnread);

public record GetAgentConversationsResult : CursorList<ConversationListItem>
{
    public GetAgentConversationsResult(IReadOnlyCollection<ConversationListItem> items, int skip, int take)
        : base(items, skip, take)
    {
    }

    public GetAgentConversationsResult()
    {
    }
}
