using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace AssurArr.Api.Migrations
{
    /// <inheritdoc />
    public partial class AddSupplierQualityIssuesAndCustomerComplaintCases : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "assurarr_customer_complaint_quality_cases",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    TenantId = table.Column<Guid>(type: "uuid", nullable: false),
                    Number = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: false),
                    Title = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: false),
                    Description = table.Column<string>(type: "character varying(4000)", maxLength: 4000, nullable: false),
                    Severity = table.Column<string>(type: "character varying(32)", maxLength: 32, nullable: false),
                    Status = table.Column<string>(type: "character varying(32)", maxLength: 32, nullable: false),
                    SourceProduct = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: true),
                    SourceObjectRef = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: true),
                    AffectedOrderRefs = table.Column<string[]>(type: "text[]", nullable: false),
                    AffectedShipmentRefs = table.Column<string[]>(type: "text[]", nullable: false),
                    AffectedItemRefs = table.Column<string[]>(type: "text[]", nullable: false),
                    AffectedAssetRefs = table.Column<string[]>(type: "text[]", nullable: false),
                    CustomerRef = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: true),
                    CustomerContactSnapshot = table.Column<string>(type: "character varying(4000)", maxLength: 4000, nullable: true),
                    CustomerLocationRef = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: true),
                    NonconformanceRef = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: true),
                    HoldRefs = table.Column<string[]>(type: "text[]", nullable: false),
                    CapaRefs = table.Column<string[]>(type: "text[]", nullable: false),
                    CustomerResponseRecordRefs = table.Column<string[]>(type: "text[]", nullable: false),
                    RecordRefs = table.Column<string[]>(type: "text[]", nullable: false),
                    CreatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    ClosedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    ClosedByPersonId = table.Column<Guid>(type: "uuid", nullable: true),
                    ClosureSummary = table.Column<string>(type: "character varying(4000)", maxLength: 4000, nullable: true),
                    ComplaintType = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: false),
                    OwnerPersonId = table.Column<Guid>(type: "uuid", nullable: true),
                    ReceivedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    ReceivedByPersonId = table.Column<Guid>(type: "uuid", nullable: true),
                    CustomerResponseDueAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_assurarr_customer_complaint_quality_cases", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "assurarr_supplier_quality_issues",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    TenantId = table.Column<Guid>(type: "uuid", nullable: false),
                    Number = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: false),
                    Title = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: false),
                    Description = table.Column<string>(type: "character varying(4000)", maxLength: 4000, nullable: false),
                    Severity = table.Column<string>(type: "character varying(32)", maxLength: 32, nullable: false),
                    Status = table.Column<string>(type: "character varying(32)", maxLength: 32, nullable: false),
                    SourceProduct = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: true),
                    SourceObjectRef = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: true),
                    AffectedReceiptRefs = table.Column<string[]>(type: "text[]", nullable: false),
                    AffectedPurchaseOrderRefs = table.Column<string[]>(type: "text[]", nullable: false),
                    AffectedItemRefs = table.Column<string[]>(type: "text[]", nullable: false),
                    SupplierRef = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: true),
                    NonconformanceRef = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: true),
                    ScarRef = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: true),
                    HoldRefs = table.Column<string[]>(type: "text[]", nullable: false),
                    RecordRefs = table.Column<string[]>(type: "text[]", nullable: false),
                    CreatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    ClosedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    ClosedByPersonId = table.Column<Guid>(type: "uuid", nullable: true),
                    ClosureSummary = table.Column<string>(type: "character varying(4000)", maxLength: 4000, nullable: true),
                    IssueType = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: false),
                    OwnerPersonId = table.Column<Guid>(type: "uuid", nullable: true),
                    OpenedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_assurarr_supplier_quality_issues", x => x.Id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_assurarr_customer_complaint_quality_cases_TenantId",
                table: "assurarr_customer_complaint_quality_cases",
                column: "TenantId");

            migrationBuilder.CreateIndex(
                name: "IX_assurarr_customer_complaint_quality_cases_TenantId_Customer~",
                table: "assurarr_customer_complaint_quality_cases",
                columns: new[] { "TenantId", "CustomerRef" });

            migrationBuilder.CreateIndex(
                name: "IX_assurarr_customer_complaint_quality_cases_TenantId_Number",
                table: "assurarr_customer_complaint_quality_cases",
                columns: new[] { "TenantId", "Number" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_assurarr_customer_complaint_quality_cases_TenantId_Status",
                table: "assurarr_customer_complaint_quality_cases",
                columns: new[] { "TenantId", "Status" });

            migrationBuilder.CreateIndex(
                name: "IX_assurarr_supplier_quality_issues_TenantId",
                table: "assurarr_supplier_quality_issues",
                column: "TenantId");

            migrationBuilder.CreateIndex(
                name: "IX_assurarr_supplier_quality_issues_TenantId_Number",
                table: "assurarr_supplier_quality_issues",
                columns: new[] { "TenantId", "Number" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_assurarr_supplier_quality_issues_TenantId_Status",
                table: "assurarr_supplier_quality_issues",
                columns: new[] { "TenantId", "Status" });

            migrationBuilder.CreateIndex(
                name: "IX_assurarr_supplier_quality_issues_TenantId_SupplierRef",
                table: "assurarr_supplier_quality_issues",
                columns: new[] { "TenantId", "SupplierRef" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "assurarr_customer_complaint_quality_cases");

            migrationBuilder.DropTable(
                name: "assurarr_supplier_quality_issues");
        }
    }
}
