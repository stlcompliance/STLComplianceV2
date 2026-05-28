namespace SupplyArr.Api.Services;

public static class SupplierOnboardingRules
{
    public static readonly IReadOnlyList<OnboardingDocumentRequirementDefinitionSnapshot> DefaultRequirements =
    [
        new("w9", "W-9 tax form", true),
        new("insurance_certificate", "Certificate of insurance", true),
        new("supplier_agreement", "Signed supplier agreement", true),
    ];

    public static IReadOnlyList<string> NormalizeRequiredTypeKeys(IEnumerable<string>? keys)
    {
        if (keys is null)
        {
            return DefaultRequirements.Select(x => x.DocumentTypeKey).ToList();
        }

        return keys
            .Select(x => x.Trim().ToLowerInvariant())
            .Where(x => !string.IsNullOrWhiteSpace(x))
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .Take(20)
            .ToList();
    }

    public static string ResolveLabel(string documentTypeKey) =>
        DefaultRequirements.FirstOrDefault(x =>
            string.Equals(x.DocumentTypeKey, documentTypeKey, StringComparison.OrdinalIgnoreCase))?.Label
        ?? documentTypeKey;
}

public sealed record OnboardingDocumentRequirementDefinitionSnapshot(
    string DocumentTypeKey,
    string Label,
    bool IsRequired);
