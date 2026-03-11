namespace BuyAlan;

using System.Diagnostics;
using System.Text.RegularExpressions;

public static class StringExtensions
{
    [DebuggerStepThrough]
    public static string ToSnakeCase(this string text)
    {
        if (String.IsNullOrWhiteSpace(text))
        {
            return text;
        }

        // Matches lower-case/digit followed by upper-case
        return Regex.Replace(text, "([a-z0-9])([A-Z])", "$1_$2").ToLowerInvariant();
    }
}