namespace HeyAlan.WebApi.Agents;

using Microsoft.AspNetCore.Mvc;

public sealed record PostAgentsInput(
    [FromQuery(Name = "subscription")] Guid Subscription);
