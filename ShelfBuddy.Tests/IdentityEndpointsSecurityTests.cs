namespace ShelfBuddy.Tests;

using ShelfBuddy.WebApi.Identity;
using System.Security.Claims;

public class IdentityEndpointsSecurityTests
{
    [Theory]
    [InlineData(null, "/admin")]
    [InlineData("", "/admin")]
    [InlineData(" ", "/admin")]
    [InlineData("/admin/inbox", "/admin/inbox")]
    [InlineData("http://evil.test", "/admin")]
    [InlineData("//evil.test/path", "/admin")]
    [InlineData("admin/inbox", "/admin")]
    public void NormalizeReturnUrl_ReturnsExpectedValue(string? input, string expected)
    {
        string normalized = IdentityEndpoints.NormalizeReturnUrl(input);
        Assert.Equal(expected, normalized);
    }

    [Theory]
    [InlineData("email_verified", "true", true)]
    [InlineData("email_verified", "True", true)]
    [InlineData("verified_email", "1", true)]
    [InlineData("urn:google:email_verified", "yes", true)]
    [InlineData("email_verified", "false", false)]
    [InlineData("email_verified", "0", false)]
    [InlineData("unrelated", "true", false)]
    public void IsExternalEmailVerified_ParsesVerificationClaims(string claimType, string claimValue, bool expected)
    {
        ClaimsPrincipal principal = CreatePrincipal(new Claim(claimType, claimValue));

        bool isVerified = IdentityEndpoints.IsExternalEmailVerified(principal);

        Assert.Equal(expected, isVerified);
    }

    [Fact]
    public void ResolvePostLoginRedirectTarget_WhenNotOnboarded_ReturnsOnboardingRoute()
    {
        string redirectTarget = IdentityEndpoints.ResolvePostLoginRedirectTarget("/admin/inbox", isOnboarded: false);
        Assert.Equal("/onboarding", redirectTarget);
    }

    [Fact]
    public void ResolvePostLoginRedirectTarget_WhenOnboarded_ReturnsRequestedRoute()
    {
        string redirectTarget = IdentityEndpoints.ResolvePostLoginRedirectTarget("/admin/inbox", isOnboarded: true);
        Assert.Equal("/admin/inbox", redirectTarget);
    }

    private static ClaimsPrincipal CreatePrincipal(params Claim[] claims)
    {
        ClaimsIdentity identity = new(claims, "test");
        return new ClaimsPrincipal(identity);
    }
}
