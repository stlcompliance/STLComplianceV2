using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SupplyArr.Api.Migrations
{
    /// <inheritdoc />
    public partial class AddPartOperationalSources : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "IsStocked",
                table: "supplyarr_parts",
                type: "boolean",
                nullable: false,
                defaultValue: true);

            migrationBuilder.AddColumn<bool>(
                name: "IsTrackable",
                table: "supplyarr_parts",
                type: "boolean",
                nullable: false,
                defaultValue: true);

            migrationBuilder.CreateTable(
                name: "supplyarr_part_sources",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    TenantId = table.Column<Guid>(type: "uuid", nullable: false),
                    PartId = table.Column<Guid>(type: "uuid", nullable: false),
                    SourceType = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: false),
                    Label = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: false),
                    Notes = table.Column<string>(type: "character varying(1024)", maxLength: 1024, nullable: false),
                    CreatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_supplyarr_part_sources", x => x.Id);
                    table.ForeignKey(
                        name: "FK_supplyarr_part_sources_supplyarr_parts_PartId",
                        column: x => x.PartId,
                        principalTable: "supplyarr_parts",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_supplyarr_part_sources_PartId",
                table: "supplyarr_part_sources",
                column: "PartId");

            migrationBuilder.CreateIndex(
                name: "IX_supplyarr_part_sources_TenantId",
                table: "supplyarr_part_sources",
                column: "TenantId");

            migrationBuilder.CreateIndex(
                name: "IX_supplyarr_part_sources_TenantId_PartId",
                table: "supplyarr_part_sources",
                columns: new[] { "TenantId", "PartId" });

            migrationBuilder.CreateIndex(
                name: "IX_supplyarr_part_sources_TenantId_PartId_SourceType_Label",
                table: "supplyarr_part_sources",
                columns: new[] { "TenantId", "PartId", "SourceType", "Label" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "supplyarr_part_sources");

            migrationBuilder.DropColumn(
                name: "IsStocked",
                table: "supplyarr_parts");

            migrationBuilder.DropColumn(
                name: "IsTrackable",
                table: "supplyarr_parts");
        }
    }
}
