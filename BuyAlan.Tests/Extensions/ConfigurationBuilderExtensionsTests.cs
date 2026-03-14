namespace BuyAlan.Tests;

using Microsoft.Extensions.Configuration;

public class ConfigurationBuilderExtensionsTests
{
    [Fact]
    public void GetTrimmedValue_WhenValueExists_ReturnsTrimmedValue()
    {
        IConfiguration configuration = CreateConfiguration(new Dictionary<string, string?>
        {
            ["KEY"] = "  value  "
        });

        string value = configuration.GetTrimmedValue("KEY");

        Assert.Equal("value", value);
    }

    [Fact]
    public void GetTrimmedValue_WhenValueMissing_Throws()
    {
        IConfiguration configuration = CreateConfiguration(new Dictionary<string, string?>());

        InvalidOperationException exception = Assert.Throws<InvalidOperationException>(() =>
            configuration.GetTrimmedValue("KEY"));

        Assert.Contains("KEY", exception.Message);
    }

    [Fact]
    public void GetTrimmedValue_WhenValueBlank_Throws()
    {
        IConfiguration configuration = CreateConfiguration(new Dictionary<string, string?>
        {
            ["KEY"] = "   "
        });

        InvalidOperationException exception = Assert.Throws<InvalidOperationException>(() =>
            configuration.GetTrimmedValue("KEY"));

        Assert.Contains("KEY", exception.Message);
    }

    [Fact]
    public void GetOptionalTrimmedValue_WhenValueExists_ReturnsTrimmedValue()
    {
        IConfiguration configuration = CreateConfiguration(new Dictionary<string, string?>
        {
            ["KEY"] = "  value  "
        });

        string? value = configuration.GetOptionalTrimmedValue("KEY");

        Assert.Equal("value", value);
    }

    [Fact]
    public void GetOptionalTrimmedValue_WhenValueMissing_ReturnsNull()
    {
        IConfiguration configuration = CreateConfiguration(new Dictionary<string, string?>());

        string? value = configuration.GetOptionalTrimmedValue("KEY");

        Assert.Null(value);
    }

    [Fact]
    public void GetOptionalTrimmedValue_WhenValueBlank_ReturnsNull()
    {
        IConfiguration configuration = CreateConfiguration(new Dictionary<string, string?>
        {
            ["KEY"] = "   "
        });

        string? value = configuration.GetOptionalTrimmedValue("KEY");

        Assert.Null(value);
    }

    private static IConfiguration CreateConfiguration(IDictionary<string, string?> values)
    {
        return new ConfigurationBuilder()
            .AddInMemoryCollection(values)
            .Build();
    }
}
