namespace ShelfBuddy.SquareIntegration;

using System.Collections.Concurrent;
using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using Microsoft.AspNetCore.DataProtection;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using ShelfBuddy.Configuration;
using ShelfBuddy.Data;
using ShelfBuddy.Data.Entities;

public sealed class SquareTokenService : ISquareTokenService
{
    private const string DataProtectionPurpose = "SquareIntegration.TokenStore.v1";
    private static readonly TimeSpan RefreshSkew = TimeSpan.FromMinutes(5);
    private static readonly ConcurrentDictionary<Guid, SemaphoreSlim> RefreshLocks = new();

    private readonly MainDataContext dbContext;
    private readonly IHttpClientFactory httpClientFactory;
    private readonly AppOptions appOptions;
    private readonly IDataProtector dataProtector;
    private readonly ILogger<SquareTokenService> logger;

    public SquareTokenService(
        MainDataContext dbContext,
        IHttpClientFactory httpClientFactory,
        AppOptions appOptions,
        IDataProtectionProvider dataProtectionProvider,
        ILogger<SquareTokenService> logger)
    {
        this.dbContext = dbContext ?? throw new ArgumentNullException(nameof(dbContext));
        this.httpClientFactory = httpClientFactory ?? throw new ArgumentNullException(nameof(httpClientFactory));
        this.appOptions = appOptions ?? throw new ArgumentNullException(nameof(appOptions));
        this.dataProtector = (dataProtectionProvider ?? throw new ArgumentNullException(nameof(dataProtectionProvider)))
            .CreateProtector(DataProtectionPurpose);
        this.logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    public async Task StoreConnectionAsync(SquareTokenStoreInput input, CancellationToken cancellationToken = default)
    {
        if (input.SubscriptionId == Guid.Empty)
        {
            throw new ArgumentException("SubscriptionId is required.", nameof(input));
        }

        if (input.ConnectedByUserId == Guid.Empty)
        {
            throw new ArgumentException("ConnectedByUserId is required.", nameof(input));
        }

        if (string.IsNullOrWhiteSpace(input.MerchantId))
        {
            throw new ArgumentException("MerchantId is required.", nameof(input));
        }

        if (string.IsNullOrWhiteSpace(input.AccessToken))
        {
            throw new ArgumentException("AccessToken is required.", nameof(input));
        }

        if (string.IsNullOrWhiteSpace(input.RefreshToken))
        {
            throw new ArgumentException("RefreshToken is required.", nameof(input));
        }

        if (input.AccessTokenExpiresAtUtc <= DateTime.UtcNow)
        {
            throw new ArgumentException("AccessTokenExpiresAtUtc must be in the future.", nameof(input));
        }

        SubscriptionSquareConnection? existingConnection = await this.dbContext.SubscriptionSquareConnections
            .SingleOrDefaultAsync(connection => connection.SubscriptionId == input.SubscriptionId, cancellationToken);

        string normalizedScopes = NormalizeScopes(input.Scopes);
        string encryptedAccessToken = this.dataProtector.Protect(input.AccessToken);
        string encryptedRefreshToken = this.dataProtector.Protect(input.RefreshToken);

        if (existingConnection is null)
        {
            SubscriptionSquareConnection createdConnection = new()
            {
                SubscriptionId = input.SubscriptionId,
                ConnectedByUserId = input.ConnectedByUserId,
                SquareMerchantId = input.MerchantId.Trim(),
                EncryptedAccessToken = encryptedAccessToken,
                EncryptedRefreshToken = encryptedRefreshToken,
                AccessTokenExpiresAtUtc = input.AccessTokenExpiresAtUtc,
                Scopes = normalizedScopes,
                DisconnectedAtUtc = null
            };

            this.dbContext.SubscriptionSquareConnections.Add(createdConnection);
        }
        else
        {
            existingConnection.ConnectedByUserId = input.ConnectedByUserId;
            existingConnection.SquareMerchantId = input.MerchantId.Trim();
            existingConnection.EncryptedAccessToken = encryptedAccessToken;
            existingConnection.EncryptedRefreshToken = encryptedRefreshToken;
            existingConnection.AccessTokenExpiresAtUtc = input.AccessTokenExpiresAtUtc;
            existingConnection.Scopes = normalizedScopes;
            existingConnection.DisconnectedAtUtc = null;
        }

        await this.dbContext.SaveChangesAsync(cancellationToken);
        this.logger.LogInformation(
            "Stored Square token connection for subscription {SubscriptionId} and merchant {MerchantId}.",
            input.SubscriptionId,
            input.MerchantId);
    }

    public async Task<SquareTokenResolution> GetValidAccessTokenAsync(Guid subscriptionId, CancellationToken cancellationToken = default)
    {
        SubscriptionSquareConnection? connection = await this.dbContext.SubscriptionSquareConnections
            .SingleOrDefaultAsync(item => item.SubscriptionId == subscriptionId, cancellationToken);

        if (connection is null)
        {
            return new SquareTokenResolution.ConnectionMissing();
        }

        SquareTokenResolution? immediateResolution = this.TryResolveWithoutRefresh(connection);
        if (immediateResolution is not null)
        {
            return immediateResolution;
        }

        SemaphoreSlim refreshLock = RefreshLocks.GetOrAdd(subscriptionId, static _ => new SemaphoreSlim(1, 1));
        await refreshLock.WaitAsync(cancellationToken);
        try
        {
            SubscriptionSquareConnection? lockedConnection = await this.dbContext.SubscriptionSquareConnections
                .SingleOrDefaultAsync(item => item.SubscriptionId == subscriptionId, cancellationToken);

            if (lockedConnection is null)
            {
                return new SquareTokenResolution.ConnectionMissing();
            }

            SquareTokenResolution? lockedImmediateResolution = this.TryResolveWithoutRefresh(lockedConnection);
            if (lockedImmediateResolution is not null)
            {
                return lockedImmediateResolution;
            }

            string refreshToken;
            try
            {
                refreshToken = this.dataProtector.Unprotect(lockedConnection.EncryptedRefreshToken);
            }
            catch (Exception exception)
            {
                this.logger.LogWarning(
                    exception,
                    "Unable to decrypt Square refresh token for subscription {SubscriptionId}. Reconnect required.",
                    subscriptionId);
                return new SquareTokenResolution.ReconnectRequired("token_decrypt_failed");
            }

            RefreshTokenApiOutcome refreshOutcome = await this.RefreshTokenAsync(refreshToken, cancellationToken);
            if (refreshOutcome.ResultType == RefreshTokenApiResultType.ReconnectRequired)
            {
                this.logger.LogWarning(
                    "Square token refresh requires reconnect for subscription {SubscriptionId} with code {ReasonCode}.",
                    subscriptionId,
                    refreshOutcome.ReasonCode);
                return new SquareTokenResolution.ReconnectRequired(refreshOutcome.ReasonCode);
            }

            if (refreshOutcome.ResultType != RefreshTokenApiResultType.Success || refreshOutcome.Payload is null)
            {
                this.logger.LogWarning(
                    "Square token refresh failed for subscription {SubscriptionId} with code {ReasonCode}.",
                    subscriptionId,
                    refreshOutcome.ReasonCode);
                return new SquareTokenResolution.RefreshFailed(refreshOutcome.ReasonCode);
            }

            RefreshTokenApiPayload payload = refreshOutcome.Payload;
            if (string.IsNullOrWhiteSpace(payload.AccessToken))
            {
                return new SquareTokenResolution.RefreshFailed("refresh_response_missing_access_token");
            }

            if (payload.AccessTokenExpiresAtUtc <= DateTime.UtcNow)
            {
                return new SquareTokenResolution.RefreshFailed("refresh_response_expired_token");
            }

            string rotatedRefreshToken = !string.IsNullOrWhiteSpace(payload.RefreshToken)
                ? payload.RefreshToken
                : refreshToken;

            lockedConnection.EncryptedAccessToken = this.dataProtector.Protect(payload.AccessToken);
            lockedConnection.EncryptedRefreshToken = this.dataProtector.Protect(rotatedRefreshToken);
            lockedConnection.AccessTokenExpiresAtUtc = payload.AccessTokenExpiresAtUtc;
            lockedConnection.Scopes = NormalizeScopes(payload.Scopes);
            lockedConnection.DisconnectedAtUtc = null;

            if (!string.IsNullOrWhiteSpace(payload.MerchantId))
            {
                lockedConnection.SquareMerchantId = payload.MerchantId.Trim();
            }

            await this.dbContext.SaveChangesAsync(cancellationToken);
            this.logger.LogInformation(
                "Square token refreshed for subscription {SubscriptionId}.",
                subscriptionId);

            return new SquareTokenResolution.Success(payload.AccessToken, payload.AccessTokenExpiresAtUtc);
        }
        finally
        {
            refreshLock.Release();
        }
    }

    private SquareTokenResolution? TryResolveWithoutRefresh(SubscriptionSquareConnection connection)
    {
        if (connection.AccessTokenExpiresAtUtc <= DateTime.UtcNow + RefreshSkew)
        {
            return null;
        }

        try
        {
            string decryptedAccessToken = this.dataProtector.Unprotect(connection.EncryptedAccessToken);
            if (string.IsNullOrWhiteSpace(decryptedAccessToken))
            {
                return new SquareTokenResolution.ReconnectRequired("token_decrypt_failed");
            }

            return new SquareTokenResolution.Success(decryptedAccessToken, connection.AccessTokenExpiresAtUtc);
        }
        catch (Exception exception)
        {
            this.logger.LogWarning(
                exception,
                "Unable to decrypt Square access token for subscription {SubscriptionId}. Reconnect required.",
                connection.SubscriptionId);
            return new SquareTokenResolution.ReconnectRequired("token_decrypt_failed");
        }
    }

    private async Task<RefreshTokenApiOutcome> RefreshTokenAsync(string refreshToken, CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(this.appOptions.SquareClientId) || string.IsNullOrWhiteSpace(this.appOptions.SquareClientSecret))
        {
            return new RefreshTokenApiOutcome(RefreshTokenApiResultType.ReconnectRequired, "square_not_configured");
        }

        bool isSandbox = this.appOptions.SquareClientId.StartsWith("sandbox-", StringComparison.OrdinalIgnoreCase);
        string baseUrl = isSandbox
            ? "https://connect.squareupsandbox.com"
            : "https://connect.squareup.com";

        HttpClient client = this.httpClientFactory.CreateClient("SquareOAuthClient");
        using HttpRequestMessage request = new(HttpMethod.Post, $"{baseUrl}/oauth2/token");
        request.Headers.Add("Square-Version", "2024-01-18");
        request.Content = JsonContent.Create(new
        {
            client_id = this.appOptions.SquareClientId,
            client_secret = this.appOptions.SquareClientSecret,
            grant_type = "refresh_token",
            refresh_token = refreshToken
        });

        using HttpResponseMessage response = await client.SendAsync(request, cancellationToken);
        string responseText = await response.Content.ReadAsStringAsync(cancellationToken);

        if (!response.IsSuccessStatusCode)
        {
            string? errorCode = TryReadSquareErrorCode(responseText);
            bool reconnectRequired = response.StatusCode is HttpStatusCode.BadRequest or HttpStatusCode.Unauthorized ||
                                     string.Equals(errorCode, "invalid_grant", StringComparison.OrdinalIgnoreCase);

            if (reconnectRequired)
            {
                return new RefreshTokenApiOutcome(RefreshTokenApiResultType.ReconnectRequired, "refresh_invalid_or_revoked");
            }

            return new RefreshTokenApiOutcome(RefreshTokenApiResultType.Failed, "refresh_request_failed");
        }

        RefreshTokenApiPayload? payload = TryParseRefreshPayload(responseText);
        if (payload is null)
        {
            return new RefreshTokenApiOutcome(RefreshTokenApiResultType.Failed, "refresh_response_invalid");
        }

        return new RefreshTokenApiOutcome(RefreshTokenApiResultType.Success, "ok", payload);
    }

    private static string NormalizeScopes(IEnumerable<string>? scopes)
    {
        if (scopes is null)
        {
            return string.Empty;
        }

        string[] normalizedScopes = scopes
            .Where(scope => !string.IsNullOrWhiteSpace(scope))
            .Select(scope => scope.Trim())
            .Distinct(StringComparer.Ordinal)
            .OrderBy(scope => scope, StringComparer.Ordinal)
            .ToArray();

        return string.Join(' ', normalizedScopes);
    }

    private static RefreshTokenApiPayload? TryParseRefreshPayload(string responseText)
    {
        try
        {
            using JsonDocument document = JsonDocument.Parse(responseText);
            JsonElement root = document.RootElement;

            string? accessToken = root.TryGetProperty("access_token", out JsonElement accessTokenElement)
                ? accessTokenElement.GetString()
                : null;
            string? refreshToken = root.TryGetProperty("refresh_token", out JsonElement refreshTokenElement)
                ? refreshTokenElement.GetString()
                : null;
            string? merchantId = root.TryGetProperty("merchant_id", out JsonElement merchantIdElement)
                ? merchantIdElement.GetString()
                : null;

            DateTime accessTokenExpiresAtUtc = ResolveAccessTokenExpiry(root);
            string[] scopes = ResolveScopes(root);

            return new RefreshTokenApiPayload(
                accessToken,
                refreshToken,
                merchantId,
                accessTokenExpiresAtUtc,
                scopes);
        }
        catch
        {
            return null;
        }
    }

    private static DateTime ResolveAccessTokenExpiry(JsonElement root)
    {
        if (root.TryGetProperty("expires_at", out JsonElement expiresAtElement))
        {
            string? expiresAtRaw = expiresAtElement.GetString();
            if (!string.IsNullOrWhiteSpace(expiresAtRaw) &&
                DateTimeOffset.TryParse(expiresAtRaw, out DateTimeOffset parsedOffset))
            {
                return parsedOffset.UtcDateTime;
            }
        }

        if (root.TryGetProperty("expires_in", out JsonElement expiresInElement) &&
            expiresInElement.TryGetInt32(out int expiresInSeconds) &&
            expiresInSeconds > 0)
        {
            return DateTime.UtcNow.AddSeconds(expiresInSeconds);
        }

        return DateTime.UtcNow.AddMinutes(-1);
    }

    private static string[] ResolveScopes(JsonElement root)
    {
        if (root.TryGetProperty("scope", out JsonElement singleScopeElement) &&
            singleScopeElement.ValueKind == JsonValueKind.String)
        {
            string? singleScope = singleScopeElement.GetString();
            if (!string.IsNullOrWhiteSpace(singleScope))
            {
                return singleScope.Split(' ', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);
            }
        }

        if (root.TryGetProperty("scopes", out JsonElement scopesArrayElement) &&
            scopesArrayElement.ValueKind == JsonValueKind.Array)
        {
            List<string> scopes = [];
            foreach (JsonElement scopeElement in scopesArrayElement.EnumerateArray())
            {
                string? scope = scopeElement.GetString();
                if (!string.IsNullOrWhiteSpace(scope))
                {
                    scopes.Add(scope.Trim());
                }
            }

            return scopes.ToArray();
        }

        return [];
    }

    private static string? TryReadSquareErrorCode(string responseText)
    {
        try
        {
            using JsonDocument document = JsonDocument.Parse(responseText);
            JsonElement root = document.RootElement;

            if (root.TryGetProperty("error", out JsonElement errorElement) &&
                errorElement.ValueKind == JsonValueKind.String)
            {
                return errorElement.GetString();
            }

            if (root.TryGetProperty("errors", out JsonElement errorsElement) &&
                errorsElement.ValueKind == JsonValueKind.Array)
            {
                JsonElement firstError = errorsElement.EnumerateArray().FirstOrDefault();
                if (firstError.ValueKind != JsonValueKind.Undefined &&
                    firstError.TryGetProperty("code", out JsonElement codeElement) &&
                    codeElement.ValueKind == JsonValueKind.String)
                {
                    string? code = codeElement.GetString();
                    return code;
                }
            }

            return null;
        }
        catch
        {
            return null;
        }
    }

    private enum RefreshTokenApiResultType
    {
        Success,
        ReconnectRequired,
        Failed
    }

    private sealed record RefreshTokenApiPayload(
        string? AccessToken,
        string? RefreshToken,
        string? MerchantId,
        DateTime AccessTokenExpiresAtUtc,
        IReadOnlyCollection<string> Scopes);

    private sealed record RefreshTokenApiOutcome(
        RefreshTokenApiResultType ResultType,
        string ReasonCode,
        RefreshTokenApiPayload? Payload = null);
}
