using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ComplianceCore.Api.Migrations
{
    /// <inheritdoc />
    public partial class ComplianceCoreProductFactMirrors : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "compliancecore_product_fact_mirrors",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    TenantId = table.Column<Guid>(type: "uuid", nullable: false),
                    SourceProduct = table.Column<string>(type: "character varying(32)", maxLength: 32, nullable: false),
                    FactKey = table.Column<string>(type: "character varying(128)", maxLength: 128, nullable: false),
                    ScopeKey = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: false),
                    ValueType = table.Column<string>(type: "character varying(32)", maxLength: 32, nullable: false),
                    StringValue = table.Column<string>(type: "character varying(512)", maxLength: 512, nullable: true),
                    BooleanValue = table.Column<bool>(type: "boolean", nullable: true),
                    NumberValue = table.Column<decimal>(type: "numeric", nullable: true),
                    DateValue = table.Column<DateOnly>(type: "date", nullable: true),
                    SourceEntityType = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: false),
                    SourceEntityId = table.Column<Guid>(type: "uuid", nullable: true),
                    SourceEventKind = table.Column<string>(type: "character varying(128)", maxLength: 128, nullable: false),
                    SourcePublicationId = table.Column<Guid>(type: "uuid", nullable: false),
                    IdempotencyKey = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: false),
                    PublishedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    CreatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_compliancecore_product_fact_mirrors", x => x.Id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_compliancecore_product_fact_mirrors_TenantId",
                table: "compliancecore_product_fact_mirrors",
                column: "TenantId");

            migrationBuilder.CreateIndex(
                name: "IX_compliancecore_product_fact_mirrors_TenantId_IdempotencyKey",
                table: "compliancecore_product_fact_mirrors",
                columns: new[] { "TenantId", "IdempotencyKey" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_compliancecore_product_fact_mirrors_TenantId_SourceProduct_~",
                table: "compliancecore_product_fact_mirrors",
                columns: new[] { "TenantId", "SourceProduct", "FactKey", "ScopeKey" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "compliancecore_product_fact_mirrors");
        }
    }
}
