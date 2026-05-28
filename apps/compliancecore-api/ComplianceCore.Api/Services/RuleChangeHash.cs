using System.Security.Cryptography;
using System.Text;

namespace ComplianceCore.Api.Services;

public static class RuleChangeHash
{
    public static string? Compute(string? ruleContentJson)
    {
        if (string.IsNullOrWhiteSpace(ruleContentJson))
        {
            return null;
        }

        var normalized = ruleContentJson.Trim();
        var bytes = SHA256.HashData(Encoding.UTF8.GetBytes(normalized));
        return Convert.ToHexString(bytes).ToLowerInvariant();
    }
}
