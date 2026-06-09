using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace MaintainArr.Api.Migrations
{
    /// <inheritdoc />
    public partial class AddRecallDomain : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "maintainarr_recall_audit_log_entries",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    TenantId = table.Column<Guid>(type: "uuid", nullable: false),
                    AssetId = table.Column<Guid>(type: "uuid", nullable: true),
                    RecallCampaignId = table.Column<Guid>(type: "uuid", nullable: true),
                    Action = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: false),
                    PreviousStatus = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: true),
                    NewStatus = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: true),
                    PersonId = table.Column<string>(type: "character varying(128)", maxLength: 128, nullable: true),
                    ServiceClientId = table.Column<Guid>(type: "uuid", nullable: true),
                    DetailsJson = table.Column<string>(type: "text", nullable: true),
                    CreatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_maintainarr_recall_audit_log_entries", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "maintainarr_recall_campaigns",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    TenantId = table.Column<Guid>(type: "uuid", nullable: false),
                    SourceProvider = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: false),
                    SourceType = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: false),
                    SourceProviderRecordId = table.Column<string>(type: "character varying(128)", maxLength: 128, nullable: true),
                    NhtsaCampaignNumber = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: true),
                    NhtsaActionNumber = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: true),
                    ManufacturerCampaignNumber = table.Column<string>(type: "character varying(128)", maxLength: 128, nullable: true),
                    CampaignTitle = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: true),
                    Manufacturer = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: false),
                    Component = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: false),
                    ReportReceivedDate = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: true),
                    CampaignStartDate = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: true),
                    CampaignEndDate = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: true),
                    CampaignStatus = table.Column<string>(type: "character varying(32)", maxLength: 32, nullable: false),
                    PotentialUnitsAffected = table.Column<int>(type: "integer", nullable: true),
                    Summary = table.Column<string>(type: "character varying(1024)", maxLength: 1024, nullable: false),
                    Consequence = table.Column<string>(type: "character varying(1024)", maxLength: 1024, nullable: false),
                    Remedy = table.Column<string>(type: "character varying(1024)", maxLength: 1024, nullable: false),
                    Notes = table.Column<string>(type: "character varying(1024)", maxLength: 1024, nullable: false),
                    ParkIt = table.Column<bool>(type: "boolean", nullable: false),
                    ParkOutside = table.Column<bool>(type: "boolean", nullable: false),
                    OverTheAirUpdate = table.Column<bool>(type: "boolean", nullable: false),
                    RecallType = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: false),
                    SourceRawJson = table.Column<string>(type: "text", nullable: true),
                    SourceUrl = table.Column<string>(type: "character varying(512)", maxLength: 512, nullable: true),
                    FetchedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    CreatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_maintainarr_recall_campaigns", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "maintainarr_recall_make_model_aliases",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    Provider = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: false),
                    RawMake = table.Column<string>(type: "character varying(128)", maxLength: 128, nullable: false),
                    RawModel = table.Column<string>(type: "character varying(128)", maxLength: 128, nullable: false),
                    NormalizedMake = table.Column<string>(type: "character varying(128)", maxLength: 128, nullable: false),
                    NormalizedModel = table.Column<string>(type: "character varying(128)", maxLength: 128, nullable: false),
                    Confidence = table.Column<double>(type: "double precision", nullable: false),
                    CreatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_maintainarr_recall_make_model_aliases", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "maintainarr_asset_recall_cases",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    TenantId = table.Column<Guid>(type: "uuid", nullable: false),
                    AssetId = table.Column<Guid>(type: "uuid", nullable: false),
                    RecallCampaignId = table.Column<Guid>(type: "uuid", nullable: false),
                    MatchBasis = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: false),
                    MatchConfidence = table.Column<string>(type: "character varying(16)", maxLength: 16, nullable: false),
                    MatchScore = table.Column<decimal>(type: "numeric(6,2)", nullable: true),
                    Status = table.Column<string>(type: "character varying(32)", maxLength: 32, nullable: false),
                    ReadinessImpact = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: false),
                    Reason = table.Column<string>(type: "character varying(1024)", maxLength: 1024, nullable: false),
                    DetectedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    LastRefreshedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    DismissedByPersonId = table.Column<string>(type: "character varying(128)", maxLength: 128, nullable: true),
                    DismissedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    DismissalReason = table.Column<string>(type: "character varying(1024)", maxLength: 1024, nullable: true),
                    VerificationSource = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: true),
                    VerificationMethod = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: true),
                    VerificationStatus = table.Column<string>(type: "character varying(32)", maxLength: 32, nullable: false),
                    VerifiedByPersonId = table.Column<string>(type: "character varying(128)", maxLength: 128, nullable: true),
                    VerifiedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    EvidenceDocumentId = table.Column<Guid>(type: "uuid", nullable: true),
                    EvidenceUrl = table.Column<string>(type: "character varying(512)", maxLength: 512, nullable: true),
                    EvidenceText = table.Column<string>(type: "character varying(1024)", maxLength: 1024, nullable: true),
                    ProviderRawJson = table.Column<string>(type: "text", nullable: true),
                    ExpiresAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    WorkOrderId = table.Column<Guid>(type: "uuid", nullable: true),
                    InspectionRunId = table.Column<Guid>(type: "uuid", nullable: true),
                    DefectId = table.Column<Guid>(type: "uuid", nullable: true),
                    ReadinessHoldId = table.Column<Guid>(type: "uuid", nullable: true),
                    ActionType = table.Column<string>(type: "character varying(32)", maxLength: 32, nullable: false),
                    ActionStatus = table.Column<string>(type: "character varying(32)", maxLength: 32, nullable: false),
                    CreatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_maintainarr_asset_recall_cases", x => x.Id);
                    table.ForeignKey(
                        name: "FK_maintainarr_asset_recall_cases_maintainarr_assets_AssetId",
                        column: x => x.AssetId,
                        principalTable: "maintainarr_assets",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_maintainarr_asset_recall_cases_maintainarr_recall_campaigns~",
                        column: x => x.RecallCampaignId,
                        principalTable: "maintainarr_recall_campaigns",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "maintainarr_recall_campaign_applicabilities",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    RecallCampaignId = table.Column<Guid>(type: "uuid", nullable: false),
                    ModelYear = table.Column<int>(type: "integer", nullable: true),
                    Make = table.Column<string>(type: "character varying(128)", maxLength: 128, nullable: true),
                    Model = table.Column<string>(type: "character varying(128)", maxLength: 128, nullable: true),
                    AssetClass = table.Column<string>(type: "character varying(128)", maxLength: 128, nullable: true),
                    AssetType = table.Column<string>(type: "character varying(128)", maxLength: 128, nullable: true),
                    BodyClass = table.Column<string>(type: "character varying(128)", maxLength: 128, nullable: true),
                    VehicleType = table.Column<string>(type: "character varying(128)", maxLength: 128, nullable: true),
                    FuelType = table.Column<string>(type: "character varying(128)", maxLength: 128, nullable: true),
                    EngineFamily = table.Column<string>(type: "character varying(128)", maxLength: 128, nullable: true),
                    EngineManufacturer = table.Column<string>(type: "character varying(128)", maxLength: 128, nullable: true),
                    ComponentCategory = table.Column<string>(type: "character varying(128)", maxLength: 128, nullable: true),
                    TireBrand = table.Column<string>(type: "character varying(128)", maxLength: 128, nullable: true),
                    TireLine = table.Column<string>(type: "character varying(128)", maxLength: 128, nullable: true),
                    TireSize = table.Column<string>(type: "character varying(128)", maxLength: 128, nullable: true),
                    EquipmentMake = table.Column<string>(type: "character varying(128)", maxLength: 128, nullable: true),
                    EquipmentModel = table.Column<string>(type: "character varying(128)", maxLength: 128, nullable: true),
                    SerialRangeStart = table.Column<string>(type: "character varying(128)", maxLength: 128, nullable: true),
                    SerialRangeEnd = table.Column<string>(type: "character varying(128)", maxLength: 128, nullable: true),
                    ProductionStartDate = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    ProductionEndDate = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    Notes = table.Column<string>(type: "character varying(1024)", maxLength: 1024, nullable: true),
                    SourceRawJson = table.Column<string>(type: "text", nullable: true),
                    CreatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_maintainarr_recall_campaign_applicabilities", x => x.Id);
                    table.ForeignKey(
                        name: "FK_maintainarr_recall_campaign_applicabilities_maintainarr_rec~",
                        column: x => x.RecallCampaignId,
                        principalTable: "maintainarr_recall_campaigns",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_maintainarr_asset_recall_cases_AssetId",
                table: "maintainarr_asset_recall_cases",
                column: "AssetId");

            migrationBuilder.CreateIndex(
                name: "IX_maintainarr_asset_recall_cases_RecallCampaignId",
                table: "maintainarr_asset_recall_cases",
                column: "RecallCampaignId");

            migrationBuilder.CreateIndex(
                name: "IX_maintainarr_asset_recall_cases_TenantId",
                table: "maintainarr_asset_recall_cases",
                column: "TenantId");

            migrationBuilder.CreateIndex(
                name: "IX_maintainarr_asset_recall_cases_TenantId_AssetId",
                table: "maintainarr_asset_recall_cases",
                columns: new[] { "TenantId", "AssetId" });

            migrationBuilder.CreateIndex(
                name: "IX_maintainarr_asset_recall_cases_TenantId_AssetId_RecallCampa~",
                table: "maintainarr_asset_recall_cases",
                columns: new[] { "TenantId", "AssetId", "RecallCampaignId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_maintainarr_asset_recall_cases_TenantId_Status",
                table: "maintainarr_asset_recall_cases",
                columns: new[] { "TenantId", "Status" });

            migrationBuilder.CreateIndex(
                name: "IX_maintainarr_asset_recall_cases_TenantId_VerificationStatus",
                table: "maintainarr_asset_recall_cases",
                columns: new[] { "TenantId", "VerificationStatus" });

            migrationBuilder.CreateIndex(
                name: "IX_maintainarr_recall_audit_log_entries_TenantId",
                table: "maintainarr_recall_audit_log_entries",
                column: "TenantId");

            migrationBuilder.CreateIndex(
                name: "IX_maintainarr_recall_audit_log_entries_TenantId_AssetId_Creat~",
                table: "maintainarr_recall_audit_log_entries",
                columns: new[] { "TenantId", "AssetId", "CreatedAt" });

            migrationBuilder.CreateIndex(
                name: "IX_maintainarr_recall_audit_log_entries_TenantId_RecallCampaig~",
                table: "maintainarr_recall_audit_log_entries",
                columns: new[] { "TenantId", "RecallCampaignId", "CreatedAt" });

            migrationBuilder.CreateIndex(
                name: "IX_maintainarr_recall_campaign_applicabilities_RecallCampaignI~",
                table: "maintainarr_recall_campaign_applicabilities",
                columns: new[] { "RecallCampaignId", "ModelYear", "Make", "Model" });

            migrationBuilder.CreateIndex(
                name: "IX_maintainarr_recall_campaigns_TenantId",
                table: "maintainarr_recall_campaigns",
                column: "TenantId");

            migrationBuilder.CreateIndex(
                name: "IX_maintainarr_recall_campaigns_TenantId_SourceProvider_Source~",
                table: "maintainarr_recall_campaigns",
                columns: new[] { "TenantId", "SourceProvider", "SourceProviderRecordId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_maintainarr_recall_make_model_aliases_Provider_NormalizedMa~",
                table: "maintainarr_recall_make_model_aliases",
                columns: new[] { "Provider", "NormalizedMake", "NormalizedModel" });

            migrationBuilder.CreateIndex(
                name: "IX_maintainarr_recall_make_model_aliases_Provider_RawMake_RawM~",
                table: "maintainarr_recall_make_model_aliases",
                columns: new[] { "Provider", "RawMake", "RawModel" },
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "maintainarr_asset_recall_cases");

            migrationBuilder.DropTable(
                name: "maintainarr_recall_audit_log_entries");

            migrationBuilder.DropTable(
                name: "maintainarr_recall_campaign_applicabilities");

            migrationBuilder.DropTable(
                name: "maintainarr_recall_make_model_aliases");

            migrationBuilder.DropTable(
                name: "maintainarr_recall_campaigns");
        }
    }
}
