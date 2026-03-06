namespace HeyAlan.SquareIntegration;

using Microsoft.AspNetCore.WebUtilities;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using HeyAlan.Configuration;
using HeyAlan.Data;
using HeyAlan.Data.Entities;
using HeyAlan.Onboarding;

public sealed class SubscriptionSquareConnectionService : ISubscriptionSquareConnectionService
{
    private static readonly string[] RequiredFullScopes =
    [
        "ITEMS_READ",
        "CUSTOMERS_READ",
        "CUSTOMERS_WRITE",
        "ORDERS_READ",
        "ORDERS_WRITE",
        "PAYMENTS_WRITE"
    ];

    private readonly MainDataContext dbContext;
    private readonly AppOptions appOptions;
    private readonly IOAuthStateProtector stateProtector;
    private readonly ISquareOAuthClient squareOAuthClient;
    private readonly ISquareTokenService squareTokenService;
    private readonly ISubscriptionOnboardingService subscriptionOnboardingService;
    private readonly ILogger<SubscriptionSquareConnectionService> logger;

    public SubscriptionSquareConnectionService(
        MainDataContext dbContext,
        AppOptions appOptions,
        IOAuthStateProtector stateProtector,
        ISquareOAuthClient squareOAuthClient,
        ISquareTokenService squareTokenService,
        ISubscriptionOnboardingService subscriptionOnboardingService,
        ILogger<SubscriptionSquareConnectionService> logger)
    {
        this.dbContext = dbContext ?? throw new ArgumentNullException(nameof(dbContext));
        this.appOptions = appOptions ?? throw new ArgumentNullException(nameof(appOptions));
        this.stateProtector = stateProtector ?? throw new ArgumentNullException(nameof(stateProtector));
        this.squareOAuthClient = squareOAuthClient ?? throw new ArgumentNullException(nameof(squareOAuthClient));
        this.squareTokenService = squareTokenService ?? throw new ArgumentNullException(nameof(squareTokenService));
        this.subscriptionOnboardingService = subscriptionOnboardingService ?? throw new ArgumentNullException(nameof(subscriptionOnboardingService));
        this.logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    public async Task<StartSquareConnectResult> StartConnectAsync(
        StartSquareConnectInput input,
        CancellationToken cancellationToken = default)
    {
        if (!await this.IsSubscriptionOwnerAsync(input.SubscriptionId, input.UserId, cancellationToken))
        {
            return new StartSquareConnectResult.Failure("subscription_owner_required");
        }

        if (String.IsNullOrWhiteSpace(this.appOptions.SquareClientId) || String.IsNullOrWhiteSpace(this.appOptions.SquareClientSecret))
        {
            return new StartSquareConnectResult.Failure("square_not_configured");
        }

        if (!TryNormalizeReturnUrl(input.ReturnUrl, out string safeReturnUrl))
        {
            return new StartSquareConnectResult.Failure("return_url_required");
        }

        string callbackUrl = BuildAbsoluteCallbackUrl(
            this.appOptions.PublicBaseUrl, "/api/subscriptions/square/callback");

        SquareConnectStatePayload payload = new(
            input.SubscriptionId,
            input.UserId,
            safeReturnUrl,
            DateTime.UtcNow);

        string protectedState = this.stateProtector.Protect(payload);
        string oauthBaseUrl = this.appOptions.SquareClientId.StartsWith("sandbox-", StringComparison.OrdinalIgnoreCase)
            ? "https://connect.squareupsandbox.com/oauth2/authorize"
            : "https://connect.squareup.com/oauth2/authorize";

        Dictionary<string, string?> parameters = new()
        {
            ["client_id"] = this.appOptions.SquareClientId,
            ["scope"] = String.Join(' ', RequiredFullScopes),
            ["state"] = protectedState,
            ["redirect_uri"] = callbackUrl,
            ["response_type"] = "code"
        };

        if (!this.appOptions.SquareClientId.StartsWith("sandbox-", StringComparison.OrdinalIgnoreCase))
        {
            parameters["session"] = "false";
        }

        string authorizeUrl = QueryHelpers.AddQueryString(oauthBaseUrl, parameters);
        return new StartSquareConnectResult.Success(authorizeUrl);
    }

    public async Task<CompleteSquareConnectResult> CompleteConnectAsync(
        CompleteSquareConnectInput input,
        CancellationToken cancellationToken = default)
    {
        if (String.IsNullOrWhiteSpace(input.State) || !this.stateProtector.TryUnprotect(input.State, out SquareConnectStatePayload? state))
        {
            string redirectUrl = AddQuery("/onboarding", "squareConnectError", "square_oauth_state_invalid");
            return new CompleteSquareConnectResult.Failure(redirectUrl, "square_oauth_state_invalid");
        }

        if (!await this.IsSubscriptionOwnerAsync(state.SubscriptionId, state.UserId, cancellationToken))
        {
            string redirectUrl = AddQuery(state.ReturnUrl, "squareConnectError", "subscription_owner_required");
            return new CompleteSquareConnectResult.Failure(redirectUrl, "subscription_owner_required");
        }

        if (String.IsNullOrWhiteSpace(input.AuthorizationCode))
        {
            string? oauthError = NormalizeOAuthError(input.OAuthError);
            if (!String.IsNullOrWhiteSpace(oauthError))
            {
                string errorCode = ResolveOAuthErrorCode(oauthError);
                string oauthErrorRedirectUrl = AddQuery(state.ReturnUrl, "squareConnectError", errorCode);
                return new CompleteSquareConnectResult.Failure(oauthErrorRedirectUrl, errorCode);
            }
        }

        if (String.IsNullOrWhiteSpace(input.AuthorizationCode))
        {
            string redirectUrl = AddQuery(
                state.ReturnUrl, 
                "squareConnectError", 
                "square_oauth_code_missing");

            return new CompleteSquareConnectResult.Failure(
                redirectUrl, 
                "square_oauth_code_missing");
        }

        string callbackUrl = BuildAbsoluteCallbackUrl(
            this.appOptions.PublicBaseUrl, 
            "/api/subscriptions/square/callback");

        SquareTokenExchangeResult tokenExchange = await this.squareOAuthClient.ExchangeAuthorizationCodeAsync(
            input.AuthorizationCode.Trim(),
            callbackUrl,
            cancellationToken);

        if (tokenExchange is SquareTokenExchangeResult.Failure tokenFailure)
        {
            string redirectUrl = AddQuery(state.ReturnUrl, "squareConnectError", tokenFailure.ErrorCode);
            return new CompleteSquareConnectResult.Failure(redirectUrl, tokenFailure.ErrorCode);
        }

        SquareTokenExchangeResult.Success tokenSuccess = (SquareTokenExchangeResult.Success)tokenExchange;
        if (!HasRequiredScopes(tokenSuccess.Payload.Scopes))
        {
            string redirectUrl = AddQuery(state.ReturnUrl, "squareConnectError", "square_required_scopes_missing");
            return new CompleteSquareConnectResult.Failure(redirectUrl, "square_required_scopes_missing");
        }

        try
        {
            await this.squareTokenService.StoreConnectionAsync(new SquareTokenStoreInput(
                state.SubscriptionId,
                state.UserId,
                tokenSuccess.Payload.MerchantId,
                tokenSuccess.Payload.AccessToken,
                tokenSuccess.Payload.RefreshToken,
                tokenSuccess.Payload.AccessTokenExpiresAtUtc,
                tokenSuccess.Payload.Scopes),
                cancellationToken);
            await this.subscriptionOnboardingService.RecomputeStateAsync(state.SubscriptionId, cancellationToken);

            string successRedirectUrl = AddQuery(state.ReturnUrl, "squareConnect", "success");
            return new CompleteSquareConnectResult.Success(successRedirectUrl);
        }
        catch (Exception exception)
        {
            this.logger.LogWarning(
                exception,
                "Failed storing square token connection for subscription {SubscriptionId}.",
                state.SubscriptionId);
            string redirectUrl = AddQuery(state.ReturnUrl, "squareConnectError", "square_connection_persist_failed");
            return new CompleteSquareConnectResult.Failure(redirectUrl, "square_connection_persist_failed");
        }
    }

    public async Task<DisconnectSquareConnectionResult> DisconnectAsync(
        DisconnectSquareConnectionInput input,
        CancellationToken cancellationToken = default)
    {
        if (!await this.IsSubscriptionOwnerAsync(input.SubscriptionId, input.UserId, cancellationToken))
        {
            return new DisconnectSquareConnectionResult.Failure("subscription_owner_required");
        }

        SubscriptionSquareConnection? connection = await this.dbContext.SubscriptionSquareConnections
            .SingleOrDefaultAsync(item => item.SubscriptionId == input.SubscriptionId, cancellationToken);
        if (connection is null)
        {
            return new DisconnectSquareConnectionResult.Failure("connection_not_found");
        }

        SquareTokenResolution tokenResolution = await this.squareTokenService.GetValidAccessTokenAsync(
            input.SubscriptionId,
            cancellationToken);

        if (tokenResolution is SquareTokenResolution.RefreshFailed)
        {
            return new DisconnectSquareConnectionResult.Failure("square_revoke_failed");
        }

        if (tokenResolution is SquareTokenResolution.Success success)
        {
            SquareRevokeResult revokeResult = await this.squareOAuthClient.RevokeAccessTokenAsync(
                success.AccessToken,
                cancellationToken);

            if (revokeResult is SquareRevokeResult.Failure revokeFailure)
            {
                return new DisconnectSquareConnectionResult.Failure(revokeFailure.ErrorCode);
            }
        }

        this.dbContext.SubscriptionSquareConnections.Remove(connection);
        await this.dbContext.SaveChangesAsync(cancellationToken);
        await this.subscriptionOnboardingService.RecomputeStateAsync(input.SubscriptionId, cancellationToken);
        return new DisconnectSquareConnectionResult.Success();
    }

    private async Task<bool> IsSubscriptionOwnerAsync(Guid subscriptionId, Guid userId, CancellationToken cancellationToken)
    {
        if (subscriptionId == Guid.Empty || userId == Guid.Empty)
        {
            return false;
        }

        bool isOwner = await this.dbContext.SubscriptionUsers
            .AnyAsync(
                membership =>
                    membership.SubscriptionId == subscriptionId &&
                    membership.UserId == userId &&
                    membership.Role == SubscriptionUserRole.Owner,
                cancellationToken);

        return isOwner;
    }

    private static bool HasRequiredScopes(IEnumerable<string> scopes)
    {
        HashSet<string> grantedScopes = scopes
            .Where(scope => !String.IsNullOrWhiteSpace(scope))
            .Select(scope => scope.Trim())
            .ToHashSet(StringComparer.Ordinal);

        return RequiredFullScopes.All(grantedScopes.Contains);
    }

    private static bool TryNormalizeReturnUrl(string rawReturnUrl, out string safeReturnUrl)
    {
        safeReturnUrl = String.Empty;

        if (String.IsNullOrWhiteSpace(rawReturnUrl))
        {
            return false;
        }

        string trimmed = rawReturnUrl.Trim();
        if (!trimmed.StartsWith('/'))
        {
            return false;
        }

        if (trimmed.StartsWith("//", StringComparison.Ordinal))
        {
            return false;
        }

        if (Uri.TryCreate(trimmed, UriKind.Absolute, out _))
        {
            return false;
        }

        safeReturnUrl = trimmed;
        return true;
    }

    private static string BuildAbsoluteCallbackUrl(Uri publicBaseUrl, string callbackPath)
    {
        Uri callbackUri = new(publicBaseUrl, callbackPath);
        return callbackUri.ToString();
    }

    private static string AddQuery(string path, string key, string value)
    {
        return QueryHelpers.AddQueryString(path, key, value);
    }

    private static string? NormalizeOAuthError(string? rawOAuthError)
    {
        if (String.IsNullOrWhiteSpace(rawOAuthError))
        {
            return null;
        }

        return rawOAuthError.Trim();
    }

    private static string ResolveOAuthErrorCode(string oauthError)
    {
        if (String.Equals(oauthError, "access_denied", StringComparison.OrdinalIgnoreCase))
        {
            return "square_oauth_access_denied";
        }

        return "square_oauth_callback_error";
    }
}
