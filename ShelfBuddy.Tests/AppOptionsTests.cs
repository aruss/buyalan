namespace ShelfBuddy.Tests;

using Microsoft.Extensions.Configuration;
using ShelfBuddy.Configuration;

public class AppOptionsTests
{
    [Fact]
    public void TryGetAppOptions_WhenOnlyPublicBaseUrlExists_ReturnsOptionsWithoutGoogleCredentials()
    {
        IConfiguration configuration = CreateConfiguration(new Dictionary<string, string?>
        {
            ["PUBLIC_BASE_URL"] = "https://shelfbuddy.test"
        });

        AppOptions appOptions = configuration.TryGetAppOptions();

        Assert.Equal(new Uri("https://shelfbuddy.test"), appOptions.PublicBaseUrl);
        Assert.Null(appOptions.GoogleClientId);
        Assert.Null(appOptions.GoogleClientSecret);
        Assert.Null(appOptions.SquareClientId);
        Assert.Null(appOptions.SquareClientSecret);
    }

    [Fact]
    public void TryGetAppOptions_WhenBothGoogleCredentialsExist_ReturnsTrimmedCredentials()
    {
        IConfiguration configuration = CreateConfiguration(new Dictionary<string, string?>
        {
            ["PUBLIC_BASE_URL"] = "https://shelfbuddy.test",
            ["GOOGLE_CLIENT_ID"] = "  test-client-id  ",
            ["GOOGLE_CLIENT_SECRET"] = "  test-client-secret  "
        });

        AppOptions appOptions = configuration.TryGetAppOptions();

        Assert.Equal("test-client-id", appOptions.GoogleClientId);
        Assert.Equal("test-client-secret", appOptions.GoogleClientSecret);
    }

    [Fact]
    public void TryGetAppOptions_WhenOnlyClientIdExists_ThrowsInvalidOperationException()
    {
        IConfiguration configuration = CreateConfiguration(new Dictionary<string, string?>
        {
            ["PUBLIC_BASE_URL"] = "https://shelfbuddy.test",
            ["GOOGLE_CLIENT_ID"] = "test-client-id"
        });

        InvalidOperationException exception = Assert.Throws<InvalidOperationException>(() =>
            configuration.TryGetAppOptions());

        Assert.Contains("GOOGLE_CLIENT_ID and GOOGLE_CLIENT_SECRET", exception.Message);
    }

    [Fact]
    public void TryGetAppOptions_WhenOnlyClientSecretExists_ThrowsInvalidOperationException()
    {
        IConfiguration configuration = CreateConfiguration(new Dictionary<string, string?>
        {
            ["PUBLIC_BASE_URL"] = "https://shelfbuddy.test",
            ["GOOGLE_CLIENT_SECRET"] = "test-client-secret"
        });

        InvalidOperationException exception = Assert.Throws<InvalidOperationException>(() =>
            configuration.TryGetAppOptions());

        Assert.Contains("GOOGLE_CLIENT_ID and GOOGLE_CLIENT_SECRET", exception.Message);
    }

    [Fact]
    public void TryGetAppOptions_WhenPublicBaseUrlIsMissing_ThrowsInvalidOperationException()
    {
        IConfiguration configuration = CreateConfiguration(new Dictionary<string, string?>());

        InvalidOperationException exception = Assert.Throws<InvalidOperationException>(() =>
            configuration.TryGetAppOptions());

        Assert.Contains("PUBLIC_BASE_URL", exception.Message);
    }

    [Fact]
    public void TryGetAppOptions_WhenPublicBaseUrlIsInvalid_ThrowsInvalidOperationException()
    {
        IConfiguration configuration = CreateConfiguration(new Dictionary<string, string?>
        {
            ["PUBLIC_BASE_URL"] = "not-a-valid-url"
        });

        InvalidOperationException exception = Assert.Throws<InvalidOperationException>(() =>
            configuration.TryGetAppOptions());

        Assert.Contains("PUBLIC_BASE_URL", exception.Message);
    }

    [Fact]
    public void TryGetAppOptions_WhenBothSquareCredentialsExist_ReturnsTrimmedSquareCredentials()
    {
        IConfiguration configuration = CreateConfiguration(new Dictionary<string, string?>
        {
            ["PUBLIC_BASE_URL"] = "https://shelfbuddy.test",
            ["SQUARE_CLIENT_ID"] = "  square-client-id  ",
            ["SQUARE_CLIENT_SECRET"] = "  square-client-secret  "
        });

        AppOptions appOptions = configuration.TryGetAppOptions();

        Assert.Equal("square-client-id", appOptions.SquareClientId);
        Assert.Equal("square-client-secret", appOptions.SquareClientSecret);
    }

    [Fact]
    public void TryGetAppOptions_WhenOnlySquareClientIdExists_ThrowsInvalidOperationException()
    {
        IConfiguration configuration = CreateConfiguration(new Dictionary<string, string?>
        {
            ["PUBLIC_BASE_URL"] = "https://shelfbuddy.test",
            ["SQUARE_CLIENT_ID"] = "square-client-id"
        });

        InvalidOperationException exception = Assert.Throws<InvalidOperationException>(() =>
            configuration.TryGetAppOptions());

        Assert.Contains("SQUARE_CLIENT_ID and SQUARE_CLIENT_SECRET", exception.Message);
    }

    [Fact]
    public void TryGetAppOptions_WhenOnlySquareClientSecretExists_ThrowsInvalidOperationException()
    {
        IConfiguration configuration = CreateConfiguration(new Dictionary<string, string?>
        {
            ["PUBLIC_BASE_URL"] = "https://shelfbuddy.test",
            ["SQUARE_CLIENT_SECRET"] = "square-client-secret"
        });

        InvalidOperationException exception = Assert.Throws<InvalidOperationException>(() =>
            configuration.TryGetAppOptions());

        Assert.Contains("SQUARE_CLIENT_ID and SQUARE_CLIENT_SECRET", exception.Message);
    }

    private static IConfiguration CreateConfiguration(IDictionary<string, string?> values)
    {
        return new ConfigurationBuilder()
            .AddInMemoryCollection(values)
            .Build();
    }
}
