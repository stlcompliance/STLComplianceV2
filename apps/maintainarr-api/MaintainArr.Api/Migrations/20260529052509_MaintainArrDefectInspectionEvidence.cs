using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace MaintainArr.Api.Migrations
{
    /// <inheritdoc />
    public partial class MaintainArrDefectInspectionEvidence : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "maintainarr_defect_evidence",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    TenantId = table.Column<Guid>(type: "uuid", nullable: false),
                    DefectId = table.Column<Guid>(type: "uuid", nullable: false),
                    EvidenceTypeKey = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: false),
                    FileName = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: false),
                    ContentType = table.Column<string>(type: "character varying(128)", maxLength: 128, nullable: false),
                    SizeBytes = table.Column<long>(type: "bigint", nullable: false),
                    StorageKey = table.Column<string>(type: "character varying(512)", maxLength: 512, nullable: false),
                    Notes = table.Column<string>(type: "character varying(1024)", maxLength: 1024, nullable: true),
                    UploadedByUserId = table.Column<Guid>(type: "uuid", nullable: false),
                    CreatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_maintainarr_defect_evidence", x => x.Id);
                    table.ForeignKey(
                        name: "FK_maintainarr_defect_evidence_maintainarr_defects_DefectId",
                        column: x => x.DefectId,
                        principalTable: "maintainarr_defects",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "maintainarr_inspection_run_evidence",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    TenantId = table.Column<Guid>(type: "uuid", nullable: false),
                    InspectionRunId = table.Column<Guid>(type: "uuid", nullable: false),
                    ChecklistItemId = table.Column<Guid>(type: "uuid", nullable: true),
                    EvidenceTypeKey = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: false),
                    FileName = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: false),
                    ContentType = table.Column<string>(type: "character varying(128)", maxLength: 128, nullable: false),
                    SizeBytes = table.Column<long>(type: "bigint", nullable: false),
                    StorageKey = table.Column<string>(type: "character varying(512)", maxLength: 512, nullable: false),
                    Notes = table.Column<string>(type: "character varying(1024)", maxLength: 1024, nullable: true),
                    UploadedByUserId = table.Column<Guid>(type: "uuid", nullable: false),
                    CreatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_maintainarr_inspection_run_evidence", x => x.Id);
                    table.ForeignKey(
                        name: "FK_maintainarr_inspection_run_evidence_maintainarr_inspection_~",
                        column: x => x.ChecklistItemId,
                        principalTable: "maintainarr_inspection_checklist_items",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                    table.ForeignKey(
                        name: "FK_maintainarr_inspection_run_evidence_maintainarr_inspection~1",
                        column: x => x.InspectionRunId,
                        principalTable: "maintainarr_inspection_runs",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_maintainarr_defect_evidence_DefectId",
                table: "maintainarr_defect_evidence",
                column: "DefectId");

            migrationBuilder.CreateIndex(
                name: "IX_maintainarr_defect_evidence_TenantId",
                table: "maintainarr_defect_evidence",
                column: "TenantId");

            migrationBuilder.CreateIndex(
                name: "IX_maintainarr_defect_evidence_TenantId_DefectId_CreatedAt",
                table: "maintainarr_defect_evidence",
                columns: new[] { "TenantId", "DefectId", "CreatedAt" });

            migrationBuilder.CreateIndex(
                name: "IX_maintainarr_inspection_run_evidence_ChecklistItemId",
                table: "maintainarr_inspection_run_evidence",
                column: "ChecklistItemId");

            migrationBuilder.CreateIndex(
                name: "IX_maintainarr_inspection_run_evidence_InspectionRunId",
                table: "maintainarr_inspection_run_evidence",
                column: "InspectionRunId");

            migrationBuilder.CreateIndex(
                name: "IX_maintainarr_inspection_run_evidence_TenantId",
                table: "maintainarr_inspection_run_evidence",
                column: "TenantId");

            migrationBuilder.CreateIndex(
                name: "IX_maintainarr_inspection_run_evidence_TenantId_InspectionRunI~",
                table: "maintainarr_inspection_run_evidence",
                columns: new[] { "TenantId", "InspectionRunId", "CreatedAt" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "maintainarr_defect_evidence");

            migrationBuilder.DropTable(
                name: "maintainarr_inspection_run_evidence");
        }
    }
}
