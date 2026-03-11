namespace BuyAlan.WebApi.Core;

using BuyAlan;
using BuyAlan.Extensions;

public record ConversationMessageItem(
    Guid MessageId,
    MessageRole Role,
    string Content,
    string From,
    string To,
    DateTimeOffset OccurredAt,
    bool IsRead,
    DateTimeOffset? ReadAt);

public record GetConversationMessagesResult : CursorList<ConversationMessageItem>
{
    public GetConversationMessagesResult(IReadOnlyCollection<ConversationMessageItem> items, int skip, int take)
        : base(items, skip, take)
    {
    }

    public GetConversationMessagesResult()
    {
    }
}
