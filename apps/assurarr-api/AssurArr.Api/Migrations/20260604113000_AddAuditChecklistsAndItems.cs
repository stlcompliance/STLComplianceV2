using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace AssurArr.Api.Migrations
{
    /// <inheritdoc />
    public partial class AddAuditChecklistsAndItems : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "assurarr_quality_audit_checklists",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    TenantId = table.Column<Guid>(type: "uuid", nullable: false),
                    Number = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: false),
                    AuditId = table.Column<Guid>(type: "uuid", nullable: false),
                    Title = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: false),
                    Description = table.Column<string>(type: "character varying(4000)", maxLength: 4000, nullable: false),
                    Status = table.Column<string>(type: "character varying(32)", maxLength: 32, nullable: false),
                    ItemRefs = table.Column<string[]>(type: "text[]", nullable: false),
                    CreatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    ClosedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    ClosedByPersonId = table.Column<Guid>(type: "uuid", nullable: true),
                    ClosureSummary = table.Column<string>(type: "character varying(4000)", maxLength: 4000, nullable: true),
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_assurarr_quality_audit_checklists", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "assurarr_quality_audit_checklist_items",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    TenantId = table.Column<Guid>(type: "uuid", nullable: false),
                    Number = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: false),
                    ChecklistId = table.Column<Guid>(type: "uuid", nullable: false),
                    Sequence = table.Column<int>(type: "integer", nullable: false),
                    Prompt = table.Column<string>(type: "character varying(4000)", maxLength: 4000, nullable: false),
                    HelpText = table.Column<string>(type: "character varying(4000)", maxLength: 4000, nullable: true),
                    RequirementRef = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: true),
                    ResponseType = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: false),
                    Required = table.Column<bool>(type: "boolean", nullable: false),
                    ResponseValue = table.Column<string>(type: "character varying(4000)", maxLength: 4000, nullable: true),
                    Result = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: true),
                    FindingCreated = table.Column<bool>(type: "boolean", nullable: false),
                    FindingRef = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: true),
                    EvidenceRecordRefs = table.Column<string[]>(type: "text[]", nullable: false),
                    AnsweredAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    AnsweredByPersonId = table.Column<Guid>(type: "uuid", nullable: true),
                    CreatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_assurarr_quality_audit_checklist_items", x => x.Id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_assurarr_quality_audit_checklists_TenantId",
                table: "assurarr_quality_audit_checklists",
                column: "TenantId");

            migrationBuilder.CreateIndex(
                name: "IX_assurarr_quality_audit_checklists_TenantId_AuditId",
                table: "assurarr_quality_audit_checklists",
                columns: new[] { "TenantId", "AuditId" });

            migrationBuilder.CreateIndex(
                name: "IX_assurarr_quality_audit_checklists_TenantId_Number",
                table: "assurarr_quality_audit_checklists",
                columns: new[] { "TenantId", "Number" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_assurarr_quality_audit_checklists_TenantId_Status",
                table: "assurarr_quality_audit_checklists",
                columns: new[] { "TenantId", "Status" });

            migrationBuilder.CreateIndex(
                name: "IX_assurarr_quality_audit_checklist_items_TenantId",
                table: "assurarr_quality_audit_checklist_items",
                column: "TenantId");

            migrationBuilder.CreateIndex(
                name: "IX_assurarr_quality_audit_checklist_items_TenantId_ChecklistId",
                table: "assurarr_quality_audit_checklist_items",
                columns: new[] { "TenantId", "ChecklistId" });

            migrationBuilder.CreateIndex(
                name: "IX_assurarr_quality_audit_checklist_items_TenantId_Number",
                table: "assurarr_quality_audit_checklist_items",
                columns: new[] { "TenantId", "Number" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_assurarr_quality_audit_checklist_items_TenantId_Sequence",
                table: "assurarr_quality_audit_checklist_items",
                columns: new[] { "TenantId", "Sequence" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "assurarr_quality_audit_checklist_items");

            migrationBuilder.DropTable(
                name: "assurarr_quality_audit_checklists");
        }
    }
}
