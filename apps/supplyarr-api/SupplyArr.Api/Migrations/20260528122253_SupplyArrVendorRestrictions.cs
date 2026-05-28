using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SupplyArr.Api.Migrations
{
    /// <inheritdoc />
    public partial class SupplyArrVendorRestrictions : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "supplyarr_vendor_restrictions",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    TenantId = table.Column<Guid>(type: "uuid", nullable: false),
                    ExternalPartyId = table.Column<Guid>(type: "uuid", nullable: false),
                    RestrictionKey = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: false),
                    ScopesJson = table.Column<string>(type: "text", nullable: false),
                    Reason = table.Column<string>(type: "character varying(512)", maxLength: 512, nullable: false),
                    Status = table.Column<string>(type: "character varying(32)", maxLength: 32, nullable: false),
                    EffectiveFrom = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    EffectiveUntil = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    CreatedByUserId = table.Column<Guid>(type: "uuid", nullable: false),
                    LiftedByUserId = table.Column<Guid>(type: "uuid", nullable: true),
                    LiftedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    LiftNotes = table.Column<string>(type: "character varying(512)", maxLength: 512, nullable: true),
                    CreatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_supplyarr_vendor_restrictions", x => x.Id);
                    table.ForeignKey(
                        name: "FK_supplyarr_vendor_restrictions_supplyarr_external_parties_Ex~",
                        column: x => x.ExternalPartyId,
                        principalTable: "supplyarr_external_parties",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_supplyarr_vendor_restrictions_ExternalPartyId",
                table: "supplyarr_vendor_restrictions",
                column: "ExternalPartyId");

            migrationBuilder.CreateIndex(
                name: "IX_supplyarr_vendor_restrictions_TenantId",
                table: "supplyarr_vendor_restrictions",
                column: "TenantId");

            migrationBuilder.CreateIndex(
                name: "IX_supplyarr_vendor_restrictions_TenantId_ExternalPartyId",
                table: "supplyarr_vendor_restrictions",
                columns: new[] { "TenantId", "ExternalPartyId" });

            migrationBuilder.CreateIndex(
                name: "IX_supplyarr_vendor_restrictions_TenantId_ExternalPartyId_Rest~",
                table: "supplyarr_vendor_restrictions",
                columns: new[] { "TenantId", "ExternalPartyId", "RestrictionKey", "Status" });

            migrationBuilder.CreateIndex(
                name: "IX_supplyarr_vendor_restrictions_TenantId_Status_UpdatedAt",
                table: "supplyarr_vendor_restrictions",
                columns: new[] { "TenantId", "Status", "UpdatedAt" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "supplyarr_vendor_restrictions");
        }
    }
}
