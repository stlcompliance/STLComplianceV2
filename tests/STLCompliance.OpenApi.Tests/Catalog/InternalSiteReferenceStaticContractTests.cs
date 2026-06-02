using System.Text.RegularExpressions;

namespace STLCompliance.OpenApi.Tests.Catalog;

[Trait("Category", "OpenApi")]
[Trait("Area", "SiteContracts")]
public sealed partial class InternalSiteReferenceStaticContractTests
{
    [Fact]
    public void StaffArr_does_not_define_operational_site_classification()
    {
        var matches = ReadSourceFiles("apps/staffarr-api")
            .SelectMany(file => FindMatches(file, SiteClassPattern()))
            .ToArray();

        Assert.Empty(matches);
    }

    [Fact]
    public void Products_do_not_declare_competing_internal_site_tables()
    {
        var matches = ReadSourceFiles("apps")
            .SelectMany(file => FindMatches(file, CompetingSiteTablePattern()))
            .Where(match => !IsAllowedSiteReferenceHelper(match.Path))
            .ToArray();

        Assert.Empty(matches);
    }

    [Fact]
    public void Free_text_internal_site_identity_fields_are_legacy_allowlisted()
    {
        var matches = ReadSourceFiles("apps")
            .SelectMany(file => FindMatches(file, FreeTextInternalSiteFieldPattern()))
            .Where(match => !IsAllowedLegacySiteAlias(match.Path))
            .ToArray();

        Assert.Empty(matches);
    }

    [Fact]
    public void StaffArr_site_name_snapshots_are_not_indexed_as_identity()
    {
        var matches = ReadSourceFiles("apps")
            .SelectMany(file => FindMatches(file, SnapshotIdentityIndexPattern()))
            .ToArray();

        Assert.Empty(matches);
    }

    private static IEnumerable<SourceFile> ReadSourceFiles(string relativeRoot)
    {
        var root = Path.Combine(FindRepoRoot(), relativeRoot);
        return Directory.EnumerateFiles(root, "*.cs", SearchOption.AllDirectories)
            .Where(path => !path.Contains($"{Path.DirectorySeparatorChar}bin{Path.DirectorySeparatorChar}", StringComparison.OrdinalIgnoreCase))
            .Where(path => !path.Contains($"{Path.DirectorySeparatorChar}obj{Path.DirectorySeparatorChar}", StringComparison.OrdinalIgnoreCase))
            .Where(path => !path.Contains($"{Path.DirectorySeparatorChar}Migrations{Path.DirectorySeparatorChar}", StringComparison.OrdinalIgnoreCase))
            .Select(path => new SourceFile(NormalizePath(Path.GetRelativePath(FindRepoRoot(), path)), File.ReadAllText(path)));
    }

    private static IEnumerable<SiteContractMatch> FindMatches(SourceFile file, Regex pattern) =>
        pattern.Matches(file.Contents)
            .Select(match => new SiteContractMatch(file.RelativePath, match.Value.Trim()));

    private static bool IsAllowedSiteReferenceHelper(string path) =>
        path.EndsWith("/Services/StaffArrSiteReferenceService.cs", StringComparison.OrdinalIgnoreCase)
        || path.EndsWith("/Integration/StaffArrSiteIntegration.cs", StringComparison.OrdinalIgnoreCase)
        || path.EndsWith("/Entities/InventoryLocation.cs", StringComparison.OrdinalIgnoreCase);

    private static bool IsAllowedLegacySiteAlias(string path) =>
        path.Contains("/maintainarr-api/MaintainArr.Api/", StringComparison.OrdinalIgnoreCase)
        || path.EndsWith("/staffarr-api/StaffArr.Api/Contracts/PersonLookupContracts.cs", StringComparison.OrdinalIgnoreCase);

    private static string FindRepoRoot()
    {
        var directory = new DirectoryInfo(AppContext.BaseDirectory);
        while (directory is not null)
        {
            if (File.Exists(Path.Combine(directory.FullName, "STLCompliance.slnx")))
            {
                return directory.FullName;
            }

            directory = directory.Parent;
        }

        throw new InvalidOperationException("Could not locate repository root.");
    }

    private static string NormalizePath(string path) =>
        path.Replace('\\', '/');

    [GeneratedRegex("siteClass|SiteClass", RegexOptions.CultureInvariant)]
    private static partial Regex SiteClassPattern();

    [GeneratedRegex(@"(?:DbSet<[^>]*(?:InternalSite|StaffArrSite|SiteClass|OperationalSite)[^>]*>|public\s+(?:sealed\s+)?(?:class|record)\s+(?:InternalSite|StaffArrSite|SiteClass|OperationalSite)\b)", RegexOptions.CultureInvariant)]
    private static partial Regex CompetingSiteTablePattern();

    [GeneratedRegex(@"\bpublic\s+[\w?<>,\s]+\s+(?:SiteId|SiteRef|SiteKey|SiteName)\b", RegexOptions.CultureInvariant)]
    private static partial Regex FreeTextInternalSiteFieldPattern();

    [GeneratedRegex(@"HasIndex\s*\([^;\n]*StaffarrSiteNameSnapshot", RegexOptions.CultureInvariant)]
    private static partial Regex SnapshotIdentityIndexPattern();

    private sealed record SourceFile(string RelativePath, string Contents);

    private sealed record SiteContractMatch(string Path, string Match)
    {
        public override string ToString() => $"{Path}: {Match}";
    }
}
