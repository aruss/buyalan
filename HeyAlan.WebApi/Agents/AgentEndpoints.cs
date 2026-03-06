namespace HeyAlan.WebApi.Agents;

using Microsoft.AspNetCore.Mvc;
using HeyAlan.Agents;
using System.Security.Claims;

public static class AgentEndpoints
{
    public static IEndpointRouteBuilder MapAgentEndpoints(this IEndpointRouteBuilder routeBuilder)
    {
        RouteGroupBuilder routeGroup = routeBuilder
            .MapGroup("/agents")
            .WithTags("Agents")
            .RequireAuthorization();

        routeGroup
            .MapGet(String.Empty, GetAgentsAsync)
            .Produces<GetAgentsResult>(StatusCodes.Status200OK)
            .Produces<AgentErrorResult>(StatusCodes.Status401Unauthorized)
            .Produces<AgentErrorResult>(StatusCodes.Status403Forbidden);

        routeGroup
            .MapPost(String.Empty, PostAgentsAsync)
            .Produces<AgentResult>(StatusCodes.Status200OK)
            .Produces<AgentErrorResult>(StatusCodes.Status401Unauthorized)
            .Produces<AgentErrorResult>(StatusCodes.Status403Forbidden);

        routeGroup
            .MapGet("{agentId:guid}", GetAgentAsync)
            .Produces<AgentResult>(StatusCodes.Status200OK)
            .Produces<AgentErrorResult>(StatusCodes.Status401Unauthorized)
            .Produces<AgentErrorResult>(StatusCodes.Status403Forbidden)
            .Produces<AgentErrorResult>(StatusCodes.Status404NotFound);

        routeGroup
            .MapPost("{agentId:guid}", PostAgentAsync)
            .Produces<AgentResult>(StatusCodes.Status200OK)
            .Produces<AgentErrorResult>(StatusCodes.Status400BadRequest)
            .Produces<AgentErrorResult>(StatusCodes.Status401Unauthorized)
            .Produces<AgentErrorResult>(StatusCodes.Status403Forbidden)
            .Produces<AgentErrorResult>(StatusCodes.Status404NotFound);

        routeGroup
            .MapDelete("{agentId:guid}", DeleteAgentAsync)
            .Produces<DeleteAgentResult>(StatusCodes.Status200OK)
            .Produces<AgentErrorResult>(StatusCodes.Status401Unauthorized)
            .Produces<AgentErrorResult>(StatusCodes.Status403Forbidden)
            .Produces<AgentErrorResult>(StatusCodes.Status404NotFound);

        return routeBuilder;
    }

    private static async Task<IResult> GetAgentsAsync(
        [AsParameters] GetAgentsInput input,
        ClaimsPrincipal user,
        ISubscriptionAgentService service,
        CancellationToken cancellationToken)
    {
        Guid? userId = user.GetUserId();
        if (userId is null)
        {
            return UnauthorizedError("unauthenticated");
        }

        HeyAlan.Agents.GetSubscriptionAgentsResult result = await service.GetAgentsAsync(
            new GetSubscriptionAgentsInput(input.Subscription, userId.Value),
            cancellationToken);

        if (result is HeyAlan.Agents.GetSubscriptionAgentsResult.Failure failure)
        {
            return MapError(failure.ErrorCode);
        }

        HeyAlan.Agents.GetSubscriptionAgentsResult.Success success =
            (HeyAlan.Agents.GetSubscriptionAgentsResult.Success)result;

        List<AgentItem> pagedAgents = success.Agents
            .Select(item => new AgentItem(
                item.AgentId,
                item.Name,
                item.Personality,
                item.IsOperationalReady,
                item.CreatedAt,
                item.UpdatedAt))
            .Skip(input.Skip)
            .Take(input.Take + 1)
            .ToList();

        return TypedResults.Ok(new GetAgentsResult(pagedAgents, input.Skip, input.Take));
    }

    private static async Task<IResult> PostAgentsAsync(
        [AsParameters] PostAgentsInput input,
        ClaimsPrincipal user,
        ISubscriptionAgentService service,
        CancellationToken cancellationToken)
    {
        Guid? userId = user.GetUserId();
        if (userId is null)
        {
            return UnauthorizedError("unauthenticated");
        }

        CreateSubscriptionAgentResult result = await service.CreateAgentAsync(
            new CreateSubscriptionAgentInput(input.Subscription, userId.Value),
            cancellationToken);

        if (result is CreateSubscriptionAgentResult.Failure failure)
        {
            return MapError(failure.ErrorCode);
        }

        CreateSubscriptionAgentResult.Success success = (CreateSubscriptionAgentResult.Success)result;
        return TypedResults.Ok(ToAgentResult(success.Agent));
    }

    private static async Task<IResult> GetAgentAsync(
        [FromRoute] Guid agentId,
        ClaimsPrincipal user,
        ISubscriptionAgentService service,
        CancellationToken cancellationToken)
    {
        Guid? userId = user.GetUserId();
        if (userId is null)
        {
            return UnauthorizedError("unauthenticated");
        }

        HeyAlan.Agents.GetAgentResult result = await service.GetAgentAsync(
            new GetAgentInput(agentId, userId.Value),
            cancellationToken);

        if (result is HeyAlan.Agents.GetAgentResult.Failure failure)
        {
            return MapError(failure.ErrorCode);
        }

        HeyAlan.Agents.GetAgentResult.Success success = (HeyAlan.Agents.GetAgentResult.Success)result;
        return TypedResults.Ok(ToAgentResult(success.Agent));
    }

    private static async Task<IResult> PostAgentAsync(
        [FromRoute] Guid agentId,
        [FromBody] PostAgentInput input,
        ClaimsPrincipal user,
        ISubscriptionAgentService service,
        CancellationToken cancellationToken)
    {
        Guid? userId = user.GetUserId();
        if (userId is null)
        {
            return UnauthorizedError("unauthenticated");
        }

        UpdateAgentResult result = await service.UpdateAgentAsync(
            new UpdateAgentInput(
                agentId,
                userId.Value,
                input.Name,
                input.Personality,
                input.PersonalityPromptRaw,
                input.TwilioPhoneNumber,
                input.TelegramBotToken,
                input.WhatsappNumber),
            cancellationToken);

        if (result is UpdateAgentResult.Failure failure)
        {
            return MapError(failure.ErrorCode);
        }

        UpdateAgentResult.Success success = (UpdateAgentResult.Success)result;
        return TypedResults.Ok(ToAgentResult(success.Agent));
    }

    private static async Task<IResult> DeleteAgentAsync(
        [FromRoute] Guid agentId,
        ClaimsPrincipal user,
        ISubscriptionAgentService service,
        CancellationToken cancellationToken)
    {
        Guid? userId = user.GetUserId();
        if (userId is null)
        {
            return UnauthorizedError("unauthenticated");
        }

        HeyAlan.Agents.DeleteAgentResult result = await service.DeleteAgentAsync(
            new DeleteAgentInput(agentId, userId.Value),
            cancellationToken);

        if (result is HeyAlan.Agents.DeleteAgentResult.Failure failure)
        {
            return MapError(failure.ErrorCode);
        }

        return TypedResults.Ok(new DeleteAgentResult(true));
    }

    private static AgentResult ToAgentResult(AgentDetailsResult agent)
    {
        return new AgentResult(
            agent.AgentId,
            agent.SubscriptionId,
            agent.Name,
            agent.Personality,
            agent.PersonalityPromptRaw,
            agent.TwilioPhoneNumber,
            agent.WhatsappNumber,
            agent.TelegramBotToken,
            agent.IsOperationalReady,
            agent.CreatedAt,
            agent.UpdatedAt);
    }

    private static IResult UnauthorizedError(string errorCode)
    {
        AgentErrorResult payload = new(errorCode, "Authentication is required.");
        return TypedResults.Json(payload, statusCode: StatusCodes.Status401Unauthorized);
    }

    private static IResult MapError(string errorCode)
    {
        AgentErrorResult payload = new(errorCode, ResolveErrorMessage(errorCode));

        int statusCode = errorCode switch
        {
            "subscription_member_required" => StatusCodes.Status403Forbidden,
            "agent_not_found" => StatusCodes.Status404NotFound,
            _ => StatusCodes.Status400BadRequest
        };

        return TypedResults.Json(payload, statusCode: statusCode);
    }

    private static string ResolveErrorMessage(string errorCode)
    {
        return errorCode switch
        {
            "subscription_member_required" => "You must be a member of the subscription.",
            "agent_not_found" => "The requested agent was not found.",
            "agent_name_required" => "Agent name is required.",
            "agent_personality_required" => "Agent personality is required.",
            "telegram_bot_token_already_in_use" => "This Telegram bot token is already connected to another agent. Use a different token.",
            "telegram_webhook_registration_failed" => "Telegram webhook registration failed. Verify the bot token, webhook URL reachability, and BotFather webhook settings, then try again.",
            "telegram_bot_token_invalid" => "Telegram rejected the bot token. Verify the token from BotFather and try again.",
            _ => "Agent request failed."
        };
    }
}
