using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace MaintainArr.Api.Migrations
{
    /// <inheritdoc />
    public partial class AddWorkOrderCommentsAndTimeline : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "maintainarr_asset_quality_holds",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    TenantId = table.Column<Guid>(type: "uuid", nullable: false),
                    AssetId = table.Column<Guid>(type: "uuid", nullable: false),
                    HoldType = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: false),
                    SourceProduct = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: false),
                    SourceObjectRef = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: true),
                    Title = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: false),
                    Description = table.Column<string>(type: "character varying(1024)", maxLength: 1024, nullable: false),
                    Severity = table.Column<string>(type: "character varying(32)", maxLength: 32, nullable: false),
                    Status = table.Column<string>(type: "character varying(32)", maxLength: 32, nullable: false),
                    CreatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    CreatedByPersonId = table.Column<string>(type: "character varying(128)", maxLength: 128, nullable: true),
                    ReleasedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    ReleasedByPersonId = table.Column<string>(type: "character varying(128)", maxLength: 128, nullable: true),
                    ReleaseReason = table.Column<string>(type: "character varying(1024)", maxLength: 1024, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_maintainarr_asset_quality_holds", x => x.Id);
                    table.ForeignKey(
                        name: "FK_maintainarr_asset_quality_holds_maintainarr_assets_AssetId",
                        column: x => x.AssetId,
                        principalTable: "maintainarr_assets",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "maintainarr_asset_readiness_checks",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    TenantId = table.Column<Guid>(type: "uuid", nullable: false),
                    AssetId = table.Column<Guid>(type: "uuid", nullable: false),
                    AssetTag = table.Column<string>(type: "character varying(128)", maxLength: 128, nullable: true),
                    VehicleRefKey = table.Column<string>(type: "character varying(128)", maxLength: 128, nullable: true),
                    SourceProduct = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: false),
                    RequestedBy = table.Column<string>(type: "character varying(128)", maxLength: 128, nullable: false),
                    Status = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: false),
                    ReadinessStatus = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: false),
                    ReadinessBasis = table.Column<string>(type: "character varying(128)", maxLength: 128, nullable: false),
                    CreatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_maintainarr_asset_readiness_checks", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "maintainarr_maintenance_vendor_works",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    TenantId = table.Column<Guid>(type: "uuid", nullable: false),
                    WorkOrderId = table.Column<Guid>(type: "uuid", nullable: false),
                    SupplierRef = table.Column<string>(type: "character varying(128)", maxLength: 128, nullable: false),
                    VendorContactSnapshot = table.Column<string>(type: "character varying(512)", maxLength: 512, nullable: true),
                    Status = table.Column<string>(type: "character varying(32)", maxLength: 32, nullable: false),
                    WorkDescription = table.Column<string>(type: "character varying(1024)", maxLength: 1024, nullable: true),
                    QuoteRecordRef = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: true),
                    ApprovalRef = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: true),
                    ScheduledAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    CompletedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    CostEstimateSnapshot = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: true),
                    InvoiceRecordRef = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: true),
                    WarrantyFlag = table.Column<bool>(type: "boolean", nullable: false),
                    Notes = table.Column<string>(type: "character varying(1024)", maxLength: 1024, nullable: true),
                    CreatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_maintainarr_maintenance_vendor_works", x => x.Id);
                    table.ForeignKey(
                        name: "FK_maintainarr_maintenance_vendor_works_maintainarr_work_order~",
                        column: x => x.WorkOrderId,
                        principalTable: "maintainarr_work_orders",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "maintainarr_work_order_blockers",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    TenantId = table.Column<Guid>(type: "uuid", nullable: false),
                    WorkOrderId = table.Column<Guid>(type: "uuid", nullable: false),
                    BlockerType = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: false),
                    SourceProduct = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: false),
                    SourceObjectRef = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: true),
                    Title = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: false),
                    Description = table.Column<string>(type: "character varying(1024)", maxLength: 1024, nullable: false),
                    Severity = table.Column<string>(type: "character varying(32)", maxLength: 32, nullable: false),
                    Status = table.Column<string>(type: "character varying(32)", maxLength: 32, nullable: false),
                    RequiredAction = table.Column<string>(type: "character varying(512)", maxLength: 512, nullable: true),
                    CreatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    CreatedByPersonId = table.Column<string>(type: "character varying(128)", maxLength: 128, nullable: true),
                    ResolvedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    ResolvedByPersonId = table.Column<string>(type: "character varying(128)", maxLength: 128, nullable: true),
                    OverrideReason = table.Column<string>(type: "character varying(1024)", maxLength: 1024, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_maintainarr_work_order_blockers", x => x.Id);
                    table.ForeignKey(
                        name: "FK_maintainarr_work_order_blockers_maintainarr_work_orders_Wor~",
                        column: x => x.WorkOrderId,
                        principalTable: "maintainarr_work_orders",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "maintainarr_work_order_closeouts",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    TenantId = table.Column<Guid>(type: "uuid", nullable: false),
                    WorkOrderId = table.Column<Guid>(type: "uuid", nullable: false),
                    CompletionSummary = table.Column<string>(type: "character varying(1024)", maxLength: 1024, nullable: false),
                    RootCause = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: true),
                    CorrectiveAction = table.Column<string>(type: "character varying(1024)", maxLength: 1024, nullable: true),
                    PreventiveActionRecommendation = table.Column<string>(type: "character varying(1024)", maxLength: 1024, nullable: true),
                    AssetReturnedToService = table.Column<bool>(type: "boolean", nullable: false),
                    ReturnToServiceAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    ReturnToServiceByPersonId = table.Column<string>(type: "character varying(128)", maxLength: 128, nullable: true),
                    PostRepairInspectionRequired = table.Column<bool>(type: "boolean", nullable: false),
                    PostRepairInspectionRef = table.Column<Guid>(type: "uuid", nullable: true),
                    SupervisorReviewRequired = table.Column<bool>(type: "boolean", nullable: false),
                    SupervisorReviewedByPersonId = table.Column<string>(type: "character varying(128)", maxLength: 128, nullable: true),
                    SupervisorReviewedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    ComplianceReviewRequired = table.Column<bool>(type: "boolean", nullable: false),
                    ComplianceReviewedByPersonId = table.Column<string>(type: "character varying(128)", maxLength: 128, nullable: true),
                    ComplianceReviewedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    QualityReviewRequired = table.Column<bool>(type: "boolean", nullable: false),
                    QualityReviewedByPersonId = table.Column<string>(type: "character varying(128)", maxLength: 128, nullable: true),
                    QualityReviewedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    EvidenceAccepted = table.Column<bool>(type: "boolean", nullable: false),
                    UnresolvedDefectRefs = table.Column<string>(type: "character varying(512)", maxLength: 512, nullable: true),
                    FollowUpWorkOrderRefs = table.Column<string>(type: "character varying(512)", maxLength: 512, nullable: true),
                    CustomerImpactSummary = table.Column<string>(type: "character varying(1024)", maxLength: 1024, nullable: true),
                    DowntimeSummary = table.Column<string>(type: "character varying(1024)", maxLength: 1024, nullable: true),
                    FinalAssetReadinessStatus = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: true),
                    FinalStatus = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: true),
                    CreatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    CreatedByPersonId = table.Column<string>(type: "character varying(128)", maxLength: 128, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_maintainarr_work_order_closeouts", x => x.Id);
                    table.ForeignKey(
                        name: "FK_maintainarr_work_order_closeouts_maintainarr_work_orders_Wo~",
                        column: x => x.WorkOrderId,
                        principalTable: "maintainarr_work_orders",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "maintainarr_work_order_comments",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    TenantId = table.Column<Guid>(type: "uuid", nullable: false),
                    WorkOrderId = table.Column<Guid>(type: "uuid", nullable: false),
                    Body = table.Column<string>(type: "character varying(2000)", maxLength: 2000, nullable: false),
                    Visibility = table.Column<string>(type: "character varying(32)", maxLength: 32, nullable: false),
                    CreatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    CreatedByPersonId = table.Column<string>(type: "character varying(128)", maxLength: 128, nullable: true),
                    EditedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    EditedByPersonId = table.Column<string>(type: "character varying(128)", maxLength: 128, nullable: true),
                    Pinned = table.Column<bool>(type: "boolean", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_maintainarr_work_order_comments", x => x.Id);
                    table.ForeignKey(
                        name: "FK_maintainarr_work_order_comments_maintainarr_work_orders_Wor~",
                        column: x => x.WorkOrderId,
                        principalTable: "maintainarr_work_orders",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "maintainarr_work_order_timeline_events",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    TenantId = table.Column<Guid>(type: "uuid", nullable: false),
                    WorkOrderId = table.Column<Guid>(type: "uuid", nullable: false),
                    EventType = table.Column<string>(type: "character varying(128)", maxLength: 128, nullable: false),
                    OccurredAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    ActorPersonId = table.Column<string>(type: "character varying(128)", maxLength: 128, nullable: true),
                    ActorServiceClientId = table.Column<string>(type: "character varying(128)", maxLength: 128, nullable: true),
                    Summary = table.Column<string>(type: "character varying(512)", maxLength: 512, nullable: false),
                    SourceProduct = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: true),
                    SourceObjectRef = table.Column<string>(type: "character varying(128)", maxLength: 128, nullable: true),
                    BeforeSnapshot = table.Column<string>(type: "text", nullable: true),
                    AfterSnapshot = table.Column<string>(type: "text", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_maintainarr_work_order_timeline_events", x => x.Id);
                    table.ForeignKey(
                        name: "FK_maintainarr_work_order_timeline_events_maintainarr_work_ord~",
                        column: x => x.WorkOrderId,
                        principalTable: "maintainarr_work_orders",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_maintainarr_asset_quality_holds_AssetId",
                table: "maintainarr_asset_quality_holds",
                column: "AssetId");

            migrationBuilder.CreateIndex(
                name: "IX_maintainarr_asset_quality_holds_TenantId",
                table: "maintainarr_asset_quality_holds",
                column: "TenantId");

            migrationBuilder.CreateIndex(
                name: "IX_maintainarr_asset_quality_holds_TenantId_AssetId_Status",
                table: "maintainarr_asset_quality_holds",
                columns: new[] { "TenantId", "AssetId", "Status" });

            migrationBuilder.CreateIndex(
                name: "IX_maintainarr_asset_quality_holds_TenantId_SourceProduct_Sour~",
                table: "maintainarr_asset_quality_holds",
                columns: new[] { "TenantId", "SourceProduct", "SourceObjectRef" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_maintainarr_asset_readiness_checks_TenantId",
                table: "maintainarr_asset_readiness_checks",
                column: "TenantId");

            migrationBuilder.CreateIndex(
                name: "IX_maintainarr_asset_readiness_checks_TenantId_AssetId_Created~",
                table: "maintainarr_asset_readiness_checks",
                columns: new[] { "TenantId", "AssetId", "CreatedAt" });

            migrationBuilder.CreateIndex(
                name: "IX_maintainarr_maintenance_vendor_works_TenantId",
                table: "maintainarr_maintenance_vendor_works",
                column: "TenantId");

            migrationBuilder.CreateIndex(
                name: "IX_maintainarr_maintenance_vendor_works_TenantId_WorkOrderId_S~",
                table: "maintainarr_maintenance_vendor_works",
                columns: new[] { "TenantId", "WorkOrderId", "SupplierRef" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_maintainarr_maintenance_vendor_works_WorkOrderId",
                table: "maintainarr_maintenance_vendor_works",
                column: "WorkOrderId");

            migrationBuilder.CreateIndex(
                name: "IX_maintainarr_work_order_blockers_TenantId",
                table: "maintainarr_work_order_blockers",
                column: "TenantId");

            migrationBuilder.CreateIndex(
                name: "IX_maintainarr_work_order_blockers_TenantId_WorkOrderId_Create~",
                table: "maintainarr_work_order_blockers",
                columns: new[] { "TenantId", "WorkOrderId", "CreatedAt" });

            migrationBuilder.CreateIndex(
                name: "IX_maintainarr_work_order_blockers_TenantId_WorkOrderId_Source~",
                table: "maintainarr_work_order_blockers",
                columns: new[] { "TenantId", "WorkOrderId", "SourceProduct", "SourceObjectRef" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_maintainarr_work_order_blockers_WorkOrderId",
                table: "maintainarr_work_order_blockers",
                column: "WorkOrderId");

            migrationBuilder.CreateIndex(
                name: "IX_maintainarr_work_order_closeouts_TenantId",
                table: "maintainarr_work_order_closeouts",
                column: "TenantId");

            migrationBuilder.CreateIndex(
                name: "IX_maintainarr_work_order_closeouts_TenantId_WorkOrderId",
                table: "maintainarr_work_order_closeouts",
                columns: new[] { "TenantId", "WorkOrderId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_maintainarr_work_order_closeouts_WorkOrderId",
                table: "maintainarr_work_order_closeouts",
                column: "WorkOrderId");

            migrationBuilder.CreateIndex(
                name: "IX_maintainarr_work_order_comments_TenantId",
                table: "maintainarr_work_order_comments",
                column: "TenantId");

            migrationBuilder.CreateIndex(
                name: "IX_maintainarr_work_order_comments_TenantId_WorkOrderId_Pinned~",
                table: "maintainarr_work_order_comments",
                columns: new[] { "TenantId", "WorkOrderId", "Pinned", "CreatedAt" });

            migrationBuilder.CreateIndex(
                name: "IX_maintainarr_work_order_comments_WorkOrderId",
                table: "maintainarr_work_order_comments",
                column: "WorkOrderId");

            migrationBuilder.CreateIndex(
                name: "IX_maintainarr_work_order_timeline_events_TenantId",
                table: "maintainarr_work_order_timeline_events",
                column: "TenantId");

            migrationBuilder.CreateIndex(
                name: "IX_maintainarr_work_order_timeline_events_TenantId_WorkOrderId~",
                table: "maintainarr_work_order_timeline_events",
                columns: new[] { "TenantId", "WorkOrderId", "OccurredAt" });

            migrationBuilder.CreateIndex(
                name: "IX_maintainarr_work_order_timeline_events_WorkOrderId",
                table: "maintainarr_work_order_timeline_events",
                column: "WorkOrderId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "maintainarr_asset_quality_holds");

            migrationBuilder.DropTable(
                name: "maintainarr_asset_readiness_checks");

            migrationBuilder.DropTable(
                name: "maintainarr_maintenance_vendor_works");

            migrationBuilder.DropTable(
                name: "maintainarr_work_order_blockers");

            migrationBuilder.DropTable(
                name: "maintainarr_work_order_closeouts");

            migrationBuilder.DropTable(
                name: "maintainarr_work_order_comments");

            migrationBuilder.DropTable(
                name: "maintainarr_work_order_timeline_events");
        }
    }
}
