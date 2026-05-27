using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ComplianceCore.Api.Migrations
{
    /// <inheritdoc />
    public partial class ComplianceCoreFactSourceRegistry : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "compliancecore_fact_sources",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    TenantId = table.Column<Guid>(type: "uuid", nullable: false),
                    FactDefinitionId = table.Column<Guid>(type: "uuid", nullable: false),
                    SourceKey = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: false),
                    SourceType = table.Column<string>(type: "character varying(32)", maxLength: 32, nullable: false),
                    Label = table.Column<string>(type: "character varying(128)", maxLength: 128, nullable: false),
                    Description = table.Column<string>(type: "character varying(1024)", maxLength: 1024, nullable: false),
                    ProductKey = table.Column<string>(type: "character varying(32)", maxLength: 32, nullable: true),
                    ProductReference = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: true),
                    ConfigJson = table.Column<string>(type: "jsonb", nullable: false),
                    Priority = table.Column<int>(type: "integer", nullable: false),
                    IsActive = table.Column<bool>(type: "boolean", nullable: false),
                    CreatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_compliancecore_fact_sources", x => x.Id);
                    table.ForeignKey(
                        name: "FK_compliancecore_fact_sources_compliancecore_fact_definitions~",
                        column: x => x.FactDefinitionId,
                        principalTable: "compliancecore_fact_definitions",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_compliancecore_fact_sources_FactDefinitionId",
                table: "compliancecore_fact_sources",
                column: "FactDefinitionId");

            migrationBuilder.CreateIndex(
                name: "IX_compliancecore_fact_sources_TenantId",
                table: "compliancecore_fact_sources",
                column: "TenantId");

            migrationBuilder.CreateIndex(
                name: "IX_compliancecore_fact_sources_TenantId_SourceKey",
                table: "compliancecore_fact_sources",
                columns: new[] { "TenantId", "SourceKey" },
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "compliancecore_fact_sources");
        }
    }
}
