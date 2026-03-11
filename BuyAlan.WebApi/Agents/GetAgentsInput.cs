namespace BuyAlan.WebApi.Agents;

using BuyAlan;
using Microsoft.AspNetCore.Mvc;
using System.ComponentModel.DataAnnotations;

public sealed record GetAgentsInput(
    [FromQuery(Name = "subscription")] Guid Subscription,
    [property: FromQuery]
    [property: Range(Constants.SkipMin, Constants.SkipMax)]
    int Skip = Constants.SkipDefault,
    [property: FromQuery]
    [property: Range(Constants.TakeMin, Constants.TakeMax)]
    int Take = Constants.TakeDefault);
