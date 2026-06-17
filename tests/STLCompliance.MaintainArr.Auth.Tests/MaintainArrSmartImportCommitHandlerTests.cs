using System.Text.Json;
using MaintainArr.Api.Data;
using MaintainArr.Api.Services;
using Microsoft.EntityFrameworkCore;
using STLCompliance.Shared.SmartImport;

namespace STLCompliance.MaintainArr.Auth.Tests;

public sealed class MaintainArrSmartImportCommitHandlerTests
{
    [Fact]
    public async Task Asset_commit_maps_source_style_asset_csv_columns()
    {
        var options = new DbContextOptionsBuilder<MaintainArrDbContext>()
            .UseInMemoryDatabase($"MaintainArrSmartImport-{Guid.NewGuid():N}")
            .Options;

        await using var db = new MaintainArrDbContext(options);
        await db.Database.EnsureCreatedAsync();
        var handler = new MaintainArrSmartImportCommitHandler(db);
        var tenantId = Guid.NewGuid();
        var actorPersonId = Guid.NewGuid();
        var importBatchId = Guid.NewGuid();
        var commitPlanId = Guid.NewGuid();
        var commitStepId = Guid.NewGuid();

        using var payloadDocument = JsonDocument.Parse(
            """
            {
              "proposedFields": {
                "Fleet Asset": "16458",
                "Description": "50KW 277/480, 60 Hz Generac 32118990200",
                "Class": "GENERATE",
                "Sub-class": "GENERATOR",
                "Status": "Active",
                "VIN/Serial #": "2072462"
              }
            }
            """);

        var response = await handler.CommitAsync(
            "asset",
            new SmartImportDestinationCommitRequest(
                TenantId: tenantId,
                ActorPersonId: actorPersonId,
                ApprovedByPersonId: actorPersonId,
                ImportBatchId: importBatchId,
                CommitPlanId: commitPlanId,
                CommitStepId: commitStepId,
                DestinationProduct: "maintainarr",
                EntityType: "asset",
                Operation: "create",
                DeterministicPayload: payloadDocument.RootElement.Clone(),
                RecordArrSourceRecordId: "recordarr-source",
                IdempotencyKey: $"smart-import:{importBatchId:D}:{commitStepId:D}"));

        Assert.Equal(SmartImportStatuses.Committed, response.Status);
        Assert.Equal("50KW 277/480, 60 Hz Generac 32118990200", response.DisplayName);

        var asset = await db.Assets
            .Include(x => x.AssetType)
            .ThenInclude(x => x.AssetClass)
            .SingleAsync(x => x.TenantId == tenantId);

        Assert.Equal("16458", asset.AssetTag);
        Assert.Equal("50KW 277/480, 60 Hz Generac 32118990200", asset.Name);
        Assert.Equal("Active", asset.LifecycleStatus);
        Assert.Equal("generate", asset.AssetType.AssetClass.ClassKey);
        Assert.Equal("GENERATE", asset.AssetType.AssetClass.Name);
        Assert.Equal("generator", asset.AssetType.TypeKey);
        Assert.Equal("GENERATOR", asset.AssetType.Name);
    }
}
