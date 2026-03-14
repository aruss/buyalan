namespace BuyAlan.Tokens;

using Microsoft.AspNetCore.WebUtilities;
using System.Security.Cryptography;

public sealed class OpaqueTokenService : ITokenService
{
    private const int TokenByteLength = 32;

    public string CreateOpaqueToken()
    {
        byte[] tokenBytes = RandomNumberGenerator.GetBytes(TokenByteLength);
        return WebEncoders.Base64UrlEncode(tokenBytes);
    }
}
