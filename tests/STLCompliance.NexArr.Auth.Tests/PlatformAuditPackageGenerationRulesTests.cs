using NexArr.Api.Entities;
using NexArr.Api.Services;
using STLCompliance.Shared.Contracts;

namespace STLCompliance.NexArr.Auth.Tests;

public sealed class PlatformAuditPackageGenerationRulesTests
{
    [Theory]
    [InlineData("zip", "zip")]
    [InlineData("JSON", "json")]
    public void NormalizeFormat_accepts_zip_and_json(string raw, string expected)
    {
        Assert.Equal(expected, PlatformAuditPackageGenerationRules.NormalizeFormat(raw));
    }

    [Fact]
    public void NormalizeFormat_rejects_unknown()
    {
        var ex = Assert.Throws<StlApiException>(() => PlatformAuditPackageGenerationRules.NormalizeFormat("csv"));
        Assert.Equal("platform_audit_package_generation.format_invalid", ex.Code);
    }

    [Fact]
    public void IsDownloadReady_requires_completed_artifact()
    {
        var zipJob = new PlatformAuditPackageGenerationJob
        {
            Status = PlatformAuditPackageGenerationJobStatuses.Completed,
            Format = PlatformAuditPackageGenerationFormats.Zip,
            ArtifactZip = [1, 2, 3],
        };
        Assert.True(PlatformAuditPackageGenerationRules.IsDownloadReady(zipJob));
    }
}
