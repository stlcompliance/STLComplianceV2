using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ComplianceCore.Api.Migrations
{
    /// <inheritdoc />
    public partial class ComplianceCoreRegulatoryMappings : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "compliancecore_regulatory_mappings",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    TenantId = table.Column<Guid>(type: "uuid", nullable: false),
                    MappingKey = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: false),
                    Label = table.Column<string>(type: "character varying(128)", maxLength: 128, nullable: false),
                    Description = table.Column<string>(type: "character varying(1024)", maxLength: 1024, nullable: false),
                    TargetKind = table.Column<string>(type: "character varying(32)", maxLength: 32, nullable: false),
                    RegulatoryProgramId = table.Column<Guid>(type: "uuid", nullable: false),
                    RulePackId = table.Column<Guid>(type: "uuid", nullable: true),
                    CitationId = table.Column<Guid>(type: "uuid", nullable: true),
                    FactDefinitionId = table.Column<Guid>(type: "uuid", nullable: true),
                    ComplianceKeyId = table.Column<Guid>(type: "uuid", nullable: true),
                    MaterialKeyId = table.Column<Guid>(type: "uuid", nullable: true),
                    IsActive = table.Column<bool>(type: "boolean", nullable: false),
                    CreatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_compliancecore_regulatory_mappings", x => x.Id);
                    table.ForeignKey(
                        name: "FK_compliancecore_regulatory_mappings_citation",
                        column: x => x.CitationId,
                        principalTable: "compliancecore_regulatory_citations",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_compliancecore_regulatory_mappings_compliance_key",
                        column: x => x.ComplianceKeyId,
                        principalTable: "compliancecore_compliance_keys",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_compliancecore_regulatory_mappings_fact_definition",
                        column: x => x.FactDefinitionId,
                        principalTable: "compliancecore_fact_definitions",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_compliancecore_regulatory_mappings_material_key",
                        column: x => x.MaterialKeyId,
                        principalTable: "compliancecore_material_keys",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_compliancecore_regulatory_mappings_program",
                        column: x => x.RegulatoryProgramId,
                        principalTable: "compliancecore_regulatory_programs",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_compliancecore_regulatory_mappings_rule_pack",
                        column: x => x.RulePackId,
                        principalTable: "compliancecore_rule_packs",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_compliancecore_regulatory_mappings_CitationId",
                table: "compliancecore_regulatory_mappings",
                column: "CitationId");

            migrationBuilder.CreateIndex(
                name: "IX_compliancecore_regulatory_mappings_ComplianceKeyId",
                table: "compliancecore_regulatory_mappings",
                column: "ComplianceKeyId");

            migrationBuilder.CreateIndex(
                name: "IX_compliancecore_regulatory_mappings_FactDefinitionId",
                table: "compliancecore_regulatory_mappings",
                column: "FactDefinitionId");

            migrationBuilder.CreateIndex(
                name: "IX_compliancecore_regulatory_mappings_MaterialKeyId",
                table: "compliancecore_regulatory_mappings",
                column: "MaterialKeyId");

            migrationBuilder.CreateIndex(
                name: "IX_compliancecore_regulatory_mappings_RegulatoryProgramId",
                table: "compliancecore_regulatory_mappings",
                column: "RegulatoryProgramId");

            migrationBuilder.CreateIndex(
                name: "IX_compliancecore_regulatory_mappings_RulePackId",
                table: "compliancecore_regulatory_mappings",
                column: "RulePackId");

            migrationBuilder.CreateIndex(
                name: "IX_compliancecore_regulatory_mappings_TenantId",
                table: "compliancecore_regulatory_mappings",
                column: "TenantId");

            migrationBuilder.CreateIndex(
                name: "IX_compliancecore_regulatory_mappings_TenantId_MappingKey",
                table: "compliancecore_regulatory_mappings",
                columns: new[] { "TenantId", "MappingKey" },
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "compliancecore_regulatory_mappings");
        }
    }
}
