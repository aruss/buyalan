namespace ShelfBuddy.Configuration;

using Microsoft.Extensions.Configuration;

public static class AppOptionsConfigurationExtensions
{
    public static AppOptions TryGetAppOptions(this IConfiguration configuration)
    {
        string endpointRaw = configuration["PUBLIC_BASE_URL"]
            ?? throw ConfigurationErrors.Missing("PUBLIC_BASE_URL");

        if (!Uri.TryCreate(endpointRaw, UriKind.Absolute, out var endpoint))
        {
            throw ConfigurationErrors.Invalid("PUBLIC_BASE_URL");
        }

        AppOptions options = new()
        {
            PublicBaseUrl = endpoint,
            GoogleClientId = NormalizeOptional(configuration["GOOGLE_CLIENT_ID"]),
            GoogleClientSecret = NormalizeOptional(configuration["GOOGLE_CLIENT_SECRET"]),
            SquareClientId = NormalizeOptional(configuration["SQUARE_CLIENT_ID"]),
            SquareClientSecret = NormalizeOptional(configuration["SQUARE_CLIENT_SECRET"])
        };

        AppOptionsValidator.Validate(options);
        return options;
    }

    private static string? NormalizeOptional(string? value)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            return null;
        }

        return value.Trim();
    }
}

public record AppOptions
{
    public Uri PublicBaseUrl { get; init; }

    public string? GoogleClientId { get; init; }

    public string? GoogleClientSecret { get; init; }

    public string? SquareClientId { get; init; }

    public string? SquareClientSecret { get; init; }
}

public static class AppOptionsValidator
{
    public static void Validate(AppOptions options)
    {
        bool hasGoogleClientId = !string.IsNullOrWhiteSpace(options.GoogleClientId);
        bool hasGoogleClientSecret = !string.IsNullOrWhiteSpace(options.GoogleClientSecret);

        if (hasGoogleClientId != hasGoogleClientSecret)
        {
            throw ConfigurationErrors.Invalid("GOOGLE_CLIENT_ID and GOOGLE_CLIENT_SECRET must both be set or both be missing");
        }
    }
}
