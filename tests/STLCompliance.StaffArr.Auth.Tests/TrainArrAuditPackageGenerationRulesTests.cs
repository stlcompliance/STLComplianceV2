using TrainArr.Api.Entities;
using TrainArr.Api.Services;

namespace STLCompliance.StaffArr.Auth.Tests;

public sealed class TrainArrAuditPackageGenerationRulesTests
{
    [Theory]
    [InlineData("zip", "zip")]
    [InlineData("JSON", "json")]
    public void NormalizeFormat_accepts_zip_and_json(string raw, string expected)
    {
        Assert.Equal(expected, AuditPackageGenerationRules.NormalizeFormat(raw));
    }

    [Fact]
    public void NormalizeFormat_rejects_unknown()
    {
        Assert.Throws<STLCompliance.Shared.Contracts.StlApiException>(
            () => AuditPackageGenerationRules.NormalizeFormat("csv"));
    }

    [Fact]
    public void IsDownloadReady_requires_completed_status_and_artifact()
    {
        var zipJob = new AuditPackageGenerationJob
        {
            Status = AuditPackageGenerationJobStatuses.Completed,
            Format = AuditPackageGenerationFormats.Zip,
            ArtifactZip = [1, 2, 3],
        };
        Assert.True(AuditPackageGenerationRules.IsDownloadReady(zipJob));

        var jsonJob = new AuditPackageGenerationJob
        {
            Status = AuditPackageGenerationJobStatuses.Completed,
            Format = AuditPackageGenerationFormats.Json,
            ArtifactJson = "{}",
        };
        Assert.True(AuditPackageGenerationRules.IsDownloadReady(jsonJob));

        var pendingJob = new AuditPackageGenerationJob
        {
            Status = AuditPackageGenerationJobStatuses.Pending,
            Format = AuditPackageGenerationFormats.Zip,
            ArtifactZip = [1],
        };
        Assert.False(AuditPackageGenerationRules.IsDownloadReady(pendingJob));
    }
}
