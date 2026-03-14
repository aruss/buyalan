namespace BuyAlan.Tests;

using BuyAlan.Tokens;

public class TokenServiceTests
{
    [Fact]
    public void CreateOpaqueToken_ReturnsDistinctUrlSafeTokens()
    {
        OpaqueTokenService service = new();

        string firstToken = service.CreateOpaqueToken();
        string secondToken = service.CreateOpaqueToken();

        Assert.False(String.IsNullOrWhiteSpace(firstToken));
        Assert.False(String.IsNullOrWhiteSpace(secondToken));
        Assert.NotEqual(firstToken, secondToken);
        Assert.DoesNotContain("+", firstToken, StringComparison.Ordinal);
        Assert.DoesNotContain("/", firstToken, StringComparison.Ordinal);
        Assert.DoesNotContain("=", firstToken, StringComparison.Ordinal);
    }
}
