using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ComplianceCore.Api.Migrations
{
    /// <inheritdoc />
    public partial class ComplianceCoreCitationFactCatalog : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "compliancecore_fact_definitions",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    TenantId = table.Column<Guid>(type: "uuid", nullable: false),
                    FactKey = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: false),
                    Label = table.Column<string>(type: "character varying(128)", maxLength: 128, nullable: false),
                    Description = table.Column<string>(type: "character varying(1024)", maxLength: 1024, nullable: false),
                    ValueType = table.Column<string>(type: "character varying(32)", maxLength: 32, nullable: false),
                    IsActive = table.Column<bool>(type: "boolean", nullable: false),
                    CreatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_compliancecore_fact_definitions", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "compliancecore_regulatory_citations",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    TenantId = table.Column<Guid>(type: "uuid", nullable: false),
                    RegulatoryProgramId = table.Column<Guid>(type: "uuid", nullable: false),
                    RulePackId = table.Column<Guid>(type: "uuid", nullable: true),
                    CitationKey = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: false),
                    Label = table.Column<string>(type: "character varying(128)", maxLength: 128, nullable: false),
                    SourceReference = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: false),
                    Description = table.Column<string>(type: "character varying(1024)", maxLength: 1024, nullable: false),
                    VersionNumber = table.Column<int>(type: "integer", nullable: false),
                    SupersedesCitationId = table.Column<Guid>(type: "uuid", nullable: true),
                    IsActive = table.Column<bool>(type: "boolean", nullable: false),
                    CreatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_compliancecore_regulatory_citations", x => x.Id);
                    table.ForeignKey(
                        name: "FK_compliancecore_regulatory_citations_program",
                        column: x => x.RegulatoryProgramId,
                        principalTable: "compliancecore_regulatory_programs",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_compliancecore_regulatory_citations_rule_pack",
                        column: x => x.RulePackId,
                        principalTable: "compliancecore_rule_packs",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_compliancecore_regulatory_citations_supersedes",
                        column: x => x.SupersedesCitationId,
                        principalTable: "compliancecore_regulatory_citations",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "compliancecore_fact_requirements",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    TenantId = table.Column<Guid>(type: "uuid", nullable: false),
                    FactDefinitionId = table.Column<Guid>(type: "uuid", nullable: false),
                    RulePackId = table.Column<Guid>(type: "uuid", nullable: true),
                    CitationId = table.Column<Guid>(type: "uuid", nullable: true),
                    RequirementKey = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: false),
                    Label = table.Column<string>(type: "character varying(128)", maxLength: 128, nullable: false),
                    Description = table.Column<string>(type: "character varying(1024)", maxLength: 1024, nullable: false),
                    IsRequired = table.Column<bool>(type: "boolean", nullable: false),
                    IsActive = table.Column<bool>(type: "boolean", nullable: false),
                    CreatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_compliancecore_fact_requirements", x => x.Id);
                    table.ForeignKey(
                        name: "FK_compliancecore_fact_requirements_compliancecore_fact_defini~",
                        column: x => x.FactDefinitionId,
                        principalTable: "compliancecore_fact_definitions",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_compliancecore_fact_requirements_compliancecore_regulatory_~",
                        column: x => x.CitationId,
                        principalTable: "compliancecore_regulatory_citations",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_compliancecore_fact_requirements_compliancecore_rule_packs_~",
                        column: x => x.RulePackId,
                        principalTable: "compliancecore_rule_packs",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_compliancecore_fact_definitions_TenantId",
                table: "compliancecore_fact_definitions",
                column: "TenantId");

            migrationBuilder.CreateIndex(
                name: "IX_compliancecore_fact_definitions_TenantId_FactKey",
                table: "compliancecore_fact_definitions",
                columns: new[] { "TenantId", "FactKey" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_compliancecore_fact_requirements_CitationId",
                table: "compliancecore_fact_requirements",
                column: "CitationId");

            migrationBuilder.CreateIndex(
                name: "IX_compliancecore_fact_requirements_FactDefinitionId",
                table: "compliancecore_fact_requirements",
                column: "FactDefinitionId");

            migrationBuilder.CreateIndex(
                name: "IX_compliancecore_fact_requirements_RulePackId",
                table: "compliancecore_fact_requirements",
                column: "RulePackId");

            migrationBuilder.CreateIndex(
                name: "IX_compliancecore_fact_requirements_TenantId",
                table: "compliancecore_fact_requirements",
                column: "TenantId");

            migrationBuilder.CreateIndex(
                name: "IX_compliancecore_fact_requirements_TenantId_RequirementKey",
                table: "compliancecore_fact_requirements",
                columns: new[] { "TenantId", "RequirementKey" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_compliancecore_regulatory_citations_RegulatoryProgramId",
                table: "compliancecore_regulatory_citations",
                column: "RegulatoryProgramId");

            migrationBuilder.CreateIndex(
                name: "IX_compliancecore_regulatory_citations_RulePackId",
                table: "compliancecore_regulatory_citations",
                column: "RulePackId");

            migrationBuilder.CreateIndex(
                name: "IX_compliancecore_regulatory_citations_SupersedesCitationId",
                table: "compliancecore_regulatory_citations",
                column: "SupersedesCitationId");

            migrationBuilder.CreateIndex(
                name: "IX_compliancecore_regulatory_citations_TenantId",
                table: "compliancecore_regulatory_citations",
                column: "TenantId");

            migrationBuilder.CreateIndex(
                name: "IX_compliancecore_regulatory_citations_TenantId_CitationKey_Ve~",
                table: "compliancecore_regulatory_citations",
                columns: new[] { "TenantId", "CitationKey", "VersionNumber" },
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "compliancecore_fact_requirements");

            migrationBuilder.DropTable(
                name: "compliancecore_fact_definitions");

            migrationBuilder.DropTable(
                name: "compliancecore_regulatory_citations");
        }
    }
}
