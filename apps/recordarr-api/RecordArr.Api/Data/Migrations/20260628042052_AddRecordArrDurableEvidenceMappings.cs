using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace RecordArr.Api.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddRecordArrDurableEvidenceMappings : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "recordarr_evidence_mappings",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    TenantId = table.Column<Guid>(type: "uuid", nullable: false),
                    EvidenceMappingId = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: false),
                    RecordId = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: false),
                    SourceProduct = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: false),
                    SourceObjectType = table.Column<string>(type: "character varying(96)", maxLength: 96, nullable: false),
                    SourceObjectId = table.Column<string>(type: "character varying(160)", maxLength: 160, nullable: false),
                    ComplianceRequirementRef = table.Column<string>(type: "character varying(160)", maxLength: 160, nullable: false),
                    EvidenceTypeKey = table.Column<string>(type: "character varying(96)", maxLength: 96, nullable: false),
                    Status = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: false),
                    MappingSource = table.Column<string>(type: "character varying(96)", maxLength: 96, nullable: false),
                    ConfidenceScore = table.Column<decimal>(type: "numeric", nullable: false),
                    ConfirmedByPersonId = table.Column<string>(type: "character varying(128)", maxLength: 128, nullable: true),
                    ConfirmedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    RejectedByPersonId = table.Column<string>(type: "character varying(128)", maxLength: 128, nullable: true),
                    RejectedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    PayloadJson = table.Column<string>(type: "jsonb", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_recordarr_evidence_mappings", x => x.Id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_recordarr_evidence_mappings_TenantId",
                table: "recordarr_evidence_mappings",
                column: "TenantId");

            migrationBuilder.CreateIndex(
                name: "IX_recordarr_evidence_mappings_TenantId_EvidenceMappingId",
                table: "recordarr_evidence_mappings",
                columns: new[] { "TenantId", "EvidenceMappingId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_recordarr_evidence_mappings_TenantId_RecordId_Status",
                table: "recordarr_evidence_mappings",
                columns: new[] { "TenantId", "RecordId", "Status" });

            migrationBuilder.CreateIndex(
                name: "IX_recordarr_evidence_mappings_TenantId_SourceProduct_SourceOb~",
                table: "recordarr_evidence_mappings",
                columns: new[] { "TenantId", "SourceProduct", "SourceObjectType", "SourceObjectId", "ComplianceRequirementRef" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "recordarr_evidence_mappings");
        }
    }
}
