using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace AssurArr.Api.Migrations
{
    /// <inheritdoc />
    public partial class AddSupplierCorrectiveActionRequests : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "assurarr_supplier_corrective_action_requests",
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
                    AffectedObjectRefs = table.Column<string[]>(type: "text[]", nullable: false),
                    SupplierRef = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: true),
                    SourceNonconformanceRef = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: true),
                    SourceCapaRef = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: true),
                    RequestedByPersonId = table.Column<Guid>(type: "uuid", nullable: true),
                    RequestedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    SupplierDueAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    SupplierResponseRecordRefs = table.Column<string[]>(type: "text[]", nullable: false),
                    ReviewPersonId = table.Column<Guid>(type: "uuid", nullable: true),
                    ReviewedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    ReviewDecision = table.Column<string>(type: "character varying(128)", maxLength: 128, nullable: true),
                    FollowUpCapaRef = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: true),
                    RecordRefs = table.Column<string[]>(type: "text[]", nullable: false),
                    CreatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    ClosedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    ClosedByPersonId = table.Column<Guid>(type: "uuid", nullable: true),
                    ClosureSummary = table.Column<string>(type: "character varying(4000)", maxLength: 4000, nullable: true),
                    OwnerPersonId = table.Column<Guid>(type: "uuid", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_assurarr_supplier_corrective_action_requests", x => x.Id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_assurarr_supplier_corrective_action_requests_TenantId",
                table: "assurarr_supplier_corrective_action_requests",
                column: "TenantId");

            migrationBuilder.CreateIndex(
                name: "IX_assurarr_supplier_corrective_action_requests_TenantId_Number",
                table: "assurarr_supplier_corrective_action_requests",
                columns: new[] { "TenantId", "Number" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_assurarr_supplier_corrective_action_requests_TenantId_Sourc~",
                table: "assurarr_supplier_corrective_action_requests",
                columns: new[] { "TenantId", "SourceNonconformanceRef" });

            migrationBuilder.CreateIndex(
                name: "IX_assurarr_supplier_corrective_action_requests_TenantId_Status",
                table: "assurarr_supplier_corrective_action_requests",
                columns: new[] { "TenantId", "Status" });

            migrationBuilder.CreateIndex(
                name: "IX_assurarr_supplier_corrective_action_requests_TenantId_Suppl~",
                table: "assurarr_supplier_corrective_action_requests",
                columns: new[] { "TenantId", "SupplierRef" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "assurarr_supplier_corrective_action_requests");
        }
    }
}
