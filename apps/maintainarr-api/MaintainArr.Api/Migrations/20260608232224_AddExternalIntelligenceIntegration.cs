using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace MaintainArr.Api.Migrations
{
    /// <inheritdoc />
    public partial class AddExternalIntelligenceIntegration : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "maintainarr_asset_enrichment_snapshots",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    TenantId = table.Column<Guid>(type: "uuid", nullable: false),
                    AssetId = table.Column<Guid>(type: "uuid", nullable: false),
                    ProviderKey = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: false),
                    SnapshotType = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: false),
                    SourceObjectRef = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: true),
                    Summary = table.Column<string>(type: "character varying(1024)", maxLength: 1024, nullable: false),
                    PayloadJson = table.Column<string>(type: "text", nullable: false),
                    CapturedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    CreatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_maintainarr_asset_enrichment_snapshots", x => x.Id);
                    table.ForeignKey(
                        name: "FK_maintainarr_asset_enrichment_snapshots_maintainarr_assets_A~",
                        column: x => x.AssetId,
                        principalTable: "maintainarr_assets",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "maintainarr_asset_external_identifiers",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    TenantId = table.Column<Guid>(type: "uuid", nullable: false),
                    AssetId = table.Column<Guid>(type: "uuid", nullable: false),
                    SourceSystem = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: false),
                    IdentifierType = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: false),
                    IdentifierValue = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: false),
                    NormalizedValue = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: false),
                    IsPrimary = table.Column<bool>(type: "boolean", nullable: false),
                    IsVerified = table.Column<bool>(type: "boolean", nullable: false),
                    MetadataJson = table.Column<string>(type: "text", nullable: false),
                    ObservedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    CreatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_maintainarr_asset_external_identifiers", x => x.Id);
                    table.ForeignKey(
                        name: "FK_maintainarr_asset_external_identifiers_maintainarr_assets_A~",
                        column: x => x.AssetId,
                        principalTable: "maintainarr_assets",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "maintainarr_asset_recall_snapshots",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    TenantId = table.Column<Guid>(type: "uuid", nullable: false),
                    AssetId = table.Column<Guid>(type: "uuid", nullable: false),
                    ProviderKey = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: false),
                    CampaignNumber = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: false),
                    ActionNumber = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: true),
                    Manufacturer = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: false),
                    Component = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: false),
                    Summary = table.Column<string>(type: "character varying(1024)", maxLength: 1024, nullable: false),
                    Consequence = table.Column<string>(type: "character varying(1024)", maxLength: 1024, nullable: false),
                    Remedy = table.Column<string>(type: "character varying(1024)", maxLength: 1024, nullable: false),
                    Notes = table.Column<string>(type: "character varying(1024)", maxLength: 1024, nullable: false),
                    ModelYear = table.Column<string>(type: "character varying(16)", maxLength: 16, nullable: true),
                    Make = table.Column<string>(type: "character varying(128)", maxLength: 128, nullable: true),
                    Model = table.Column<string>(type: "character varying(128)", maxLength: 128, nullable: true),
                    ReportReceivedDate = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: true),
                    Status = table.Column<string>(type: "character varying(32)", maxLength: 32, nullable: false),
                    QualityHoldId = table.Column<Guid>(type: "uuid", nullable: true),
                    CapturedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    CreatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_maintainarr_asset_recall_snapshots", x => x.Id);
                    table.ForeignKey(
                        name: "FK_maintainarr_asset_recall_snapshots_maintainarr_assets_Asset~",
                        column: x => x.AssetId,
                        principalTable: "maintainarr_assets",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "maintainarr_external_provider_audit_log_entries",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    TenantId = table.Column<Guid>(type: "uuid", nullable: false),
                    AssetId = table.Column<Guid>(type: "uuid", nullable: true),
                    ProviderKey = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: false),
                    OperationKey = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: false),
                    CacheKey = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: false),
                    ResultStatus = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: false),
                    DurationMs = table.Column<int>(type: "integer", nullable: true),
                    Message = table.Column<string>(type: "character varying(1024)", maxLength: 1024, nullable: false),
                    CreatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_maintainarr_external_provider_audit_log_entries", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "maintainarr_external_provider_cache_entries",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    TenantId = table.Column<Guid>(type: "uuid", nullable: false),
                    ProviderKey = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: false),
                    CacheKey = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: false),
                    OperationKey = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: false),
                    RequestJson = table.Column<string>(type: "text", nullable: false),
                    ResponseJson = table.Column<string>(type: "text", nullable: false),
                    StatusCode = table.Column<int>(type: "integer", nullable: true),
                    ErrorMessage = table.Column<string>(type: "character varying(1024)", maxLength: 1024, nullable: true),
                    LastFetchedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    ExpiresAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    CreatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_maintainarr_external_provider_cache_entries", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "maintainarr_asset_enrichment_suggestions",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    TenantId = table.Column<Guid>(type: "uuid", nullable: false),
                    AssetId = table.Column<Guid>(type: "uuid", nullable: false),
                    SnapshotId = table.Column<Guid>(type: "uuid", nullable: true),
                    ProviderKey = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: false),
                    FieldKey = table.Column<string>(type: "character varying(128)", maxLength: 128, nullable: false),
                    FieldLabel = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: false),
                    CurrentValue = table.Column<string>(type: "character varying(512)", maxLength: 512, nullable: true),
                    ProposedValue = table.Column<string>(type: "character varying(512)", maxLength: 512, nullable: true),
                    Reason = table.Column<string>(type: "character varying(1024)", maxLength: 1024, nullable: false),
                    Confidence = table.Column<double>(type: "double precision", nullable: false),
                    Status = table.Column<string>(type: "character varying(32)", maxLength: 32, nullable: false),
                    ReviewedByPersonId = table.Column<string>(type: "character varying(128)", maxLength: 128, nullable: true),
                    ReviewedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    CreatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_maintainarr_asset_enrichment_suggestions", x => x.Id);
                    table.ForeignKey(
                        name: "FK_maintainarr_asset_enrichment_suggestions_maintainarr_asset_~",
                        column: x => x.SnapshotId,
                        principalTable: "maintainarr_asset_enrichment_snapshots",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                    table.ForeignKey(
                        name: "FK_maintainarr_asset_enrichment_suggestions_maintainarr_assets~",
                        column: x => x.AssetId,
                        principalTable: "maintainarr_assets",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_maintainarr_asset_enrichment_snapshots_AssetId",
                table: "maintainarr_asset_enrichment_snapshots",
                column: "AssetId");

            migrationBuilder.CreateIndex(
                name: "IX_maintainarr_asset_enrichment_snapshots_TenantId",
                table: "maintainarr_asset_enrichment_snapshots",
                column: "TenantId");

            migrationBuilder.CreateIndex(
                name: "IX_maintainarr_asset_enrichment_snapshots_TenantId_AssetId",
                table: "maintainarr_asset_enrichment_snapshots",
                columns: new[] { "TenantId", "AssetId" });

            migrationBuilder.CreateIndex(
                name: "IX_maintainarr_asset_enrichment_snapshots_TenantId_AssetId_Pro~",
                table: "maintainarr_asset_enrichment_snapshots",
                columns: new[] { "TenantId", "AssetId", "ProviderKey", "SnapshotType", "CapturedAt" });

            migrationBuilder.CreateIndex(
                name: "IX_maintainarr_asset_enrichment_suggestions_AssetId",
                table: "maintainarr_asset_enrichment_suggestions",
                column: "AssetId");

            migrationBuilder.CreateIndex(
                name: "IX_maintainarr_asset_enrichment_suggestions_SnapshotId",
                table: "maintainarr_asset_enrichment_suggestions",
                column: "SnapshotId");

            migrationBuilder.CreateIndex(
                name: "IX_maintainarr_asset_enrichment_suggestions_TenantId",
                table: "maintainarr_asset_enrichment_suggestions",
                column: "TenantId");

            migrationBuilder.CreateIndex(
                name: "IX_maintainarr_asset_enrichment_suggestions_TenantId_AssetId",
                table: "maintainarr_asset_enrichment_suggestions",
                columns: new[] { "TenantId", "AssetId" });

            migrationBuilder.CreateIndex(
                name: "IX_maintainarr_asset_enrichment_suggestions_TenantId_AssetId_P~",
                table: "maintainarr_asset_enrichment_suggestions",
                columns: new[] { "TenantId", "AssetId", "ProviderKey", "FieldKey" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_maintainarr_asset_enrichment_suggestions_TenantId_AssetId_S~",
                table: "maintainarr_asset_enrichment_suggestions",
                columns: new[] { "TenantId", "AssetId", "Status" });

            migrationBuilder.CreateIndex(
                name: "IX_maintainarr_asset_external_identifiers_AssetId",
                table: "maintainarr_asset_external_identifiers",
                column: "AssetId");

            migrationBuilder.CreateIndex(
                name: "IX_maintainarr_asset_external_identifiers_TenantId",
                table: "maintainarr_asset_external_identifiers",
                column: "TenantId");

            migrationBuilder.CreateIndex(
                name: "IX_maintainarr_asset_external_identifiers_TenantId_AssetId",
                table: "maintainarr_asset_external_identifiers",
                columns: new[] { "TenantId", "AssetId" });

            migrationBuilder.CreateIndex(
                name: "IX_maintainarr_asset_external_identifiers_TenantId_AssetId_Sou~",
                table: "maintainarr_asset_external_identifiers",
                columns: new[] { "TenantId", "AssetId", "SourceSystem", "IdentifierType", "NormalizedValue" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_maintainarr_asset_recall_snapshots_AssetId",
                table: "maintainarr_asset_recall_snapshots",
                column: "AssetId");

            migrationBuilder.CreateIndex(
                name: "IX_maintainarr_asset_recall_snapshots_TenantId",
                table: "maintainarr_asset_recall_snapshots",
                column: "TenantId");

            migrationBuilder.CreateIndex(
                name: "IX_maintainarr_asset_recall_snapshots_TenantId_AssetId",
                table: "maintainarr_asset_recall_snapshots",
                columns: new[] { "TenantId", "AssetId" });

            migrationBuilder.CreateIndex(
                name: "IX_maintainarr_asset_recall_snapshots_TenantId_AssetId_Campaig~",
                table: "maintainarr_asset_recall_snapshots",
                columns: new[] { "TenantId", "AssetId", "CampaignNumber" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_maintainarr_asset_recall_snapshots_TenantId_AssetId_Status",
                table: "maintainarr_asset_recall_snapshots",
                columns: new[] { "TenantId", "AssetId", "Status" });

            migrationBuilder.CreateIndex(
                name: "IX_maintainarr_external_provider_audit_log_entries_TenantId",
                table: "maintainarr_external_provider_audit_log_entries",
                column: "TenantId");

            migrationBuilder.CreateIndex(
                name: "IX_maintainarr_external_provider_audit_log_entries_TenantId_Pr~",
                table: "maintainarr_external_provider_audit_log_entries",
                columns: new[] { "TenantId", "ProviderKey", "CreatedAt" });

            migrationBuilder.CreateIndex(
                name: "IX_maintainarr_external_provider_cache_entries_TenantId",
                table: "maintainarr_external_provider_cache_entries",
                column: "TenantId");

            migrationBuilder.CreateIndex(
                name: "IX_maintainarr_external_provider_cache_entries_TenantId_Provi~1",
                table: "maintainarr_external_provider_cache_entries",
                columns: new[] { "TenantId", "ProviderKey", "ExpiresAt" });

            migrationBuilder.CreateIndex(
                name: "IX_maintainarr_external_provider_cache_entries_TenantId_Provid~",
                table: "maintainarr_external_provider_cache_entries",
                columns: new[] { "TenantId", "ProviderKey", "CacheKey" },
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "maintainarr_asset_enrichment_suggestions");

            migrationBuilder.DropTable(
                name: "maintainarr_asset_external_identifiers");

            migrationBuilder.DropTable(
                name: "maintainarr_asset_recall_snapshots");

            migrationBuilder.DropTable(
                name: "maintainarr_external_provider_audit_log_entries");

            migrationBuilder.DropTable(
                name: "maintainarr_external_provider_cache_entries");

            migrationBuilder.DropTable(
                name: "maintainarr_asset_enrichment_snapshots");
        }
    }
}
