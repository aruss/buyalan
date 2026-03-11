namespace BuyAlan.WebApi.Agents;

using BuyAlan.Data.Entities;
using BuyAlan.Extensions;

public sealed record AgentItem(
    Guid AgentId,
    string Name,
    AgentPersonality? Personality,
    bool IsOperationalReady,
    DateTime CreatedAt,
    DateTime UpdatedAt);

public sealed record GetAgentsResult : CursorList<AgentItem>
{
    public GetAgentsResult(IReadOnlyCollection<AgentItem> items, int skip, int take)
        : base(items, skip, take)
    {
    }

    public GetAgentsResult()
    {
    }
}
