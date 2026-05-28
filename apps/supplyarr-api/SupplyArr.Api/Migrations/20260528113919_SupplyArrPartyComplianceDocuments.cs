using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SupplyArr.Api.Migrations
{
    /// <inheritdoc />
    public partial class SupplyArrPartyComplianceDocuments : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "supplyarr_party_compliance_documents",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    TenantId = table.Column<Guid>(type: "uuid", nullable: false),
                    ExternalPartyId = table.Column<Guid>(type: "uuid", nullable: false),
                    DocumentKey = table.Column<string>(type: "character varying(128)", maxLength: 128, nullable: false),
                    DocumentTypeKey = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: false),
                    Title = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: false),
                    Version = table.Column<int>(type: "integer", nullable: false),
                    ReviewStatus = table.Column<string>(type: "character varying(32)", maxLength: 32, nullable: false),
                    ExpiresAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    EffectiveAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    FileName = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: false),
                    ContentType = table.Column<string>(type: "character varying(128)", maxLength: 128, nullable: false),
                    SizeBytes = table.Column<long>(type: "bigint", nullable: false),
                    StorageKey = table.Column<string>(type: "character varying(512)", maxLength: 512, nullable: true),
                    Notes = table.Column<string>(type: "character varying(1024)", maxLength: 1024, nullable: false),
                    UploadedByUserId = table.Column<Guid>(type: "uuid", nullable: true),
                    ReviewedByUserId = table.Column<Guid>(type: "uuid", nullable: true),
                    ReviewedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    CreatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_supplyarr_party_compliance_documents", x => x.Id);
                    table.ForeignKey(
                        name: "FK_supplyarr_party_compliance_documents_supplyarr_external_par~",
                        column: x => x.ExternalPartyId,
                        principalTable: "supplyarr_external_parties",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_supplyarr_party_compliance_documents_ExternalPartyId",
                table: "supplyarr_party_compliance_documents",
                column: "ExternalPartyId");

            migrationBuilder.CreateIndex(
                name: "IX_supplyarr_party_compliance_documents_TenantId",
                table: "supplyarr_party_compliance_documents",
                column: "TenantId");

            migrationBuilder.CreateIndex(
                name: "IX_supplyarr_party_compliance_documents_TenantId_ExpiresAt",
                table: "supplyarr_party_compliance_documents",
                columns: new[] { "TenantId", "ExpiresAt" });

            migrationBuilder.CreateIndex(
                name: "IX_supplyarr_party_compliance_documents_TenantId_ExternalPart~1",
                table: "supplyarr_party_compliance_documents",
                columns: new[] { "TenantId", "ExternalPartyId", "DocumentKey", "Version" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_supplyarr_party_compliance_documents_TenantId_ExternalParty~",
                table: "supplyarr_party_compliance_documents",
                columns: new[] { "TenantId", "ExternalPartyId" });

            migrationBuilder.CreateIndex(
                name: "IX_supplyarr_party_compliance_documents_TenantId_ReviewStatus_~",
                table: "supplyarr_party_compliance_documents",
                columns: new[] { "TenantId", "ReviewStatus", "UpdatedAt" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "supplyarr_party_compliance_documents");
        }
    }
}
