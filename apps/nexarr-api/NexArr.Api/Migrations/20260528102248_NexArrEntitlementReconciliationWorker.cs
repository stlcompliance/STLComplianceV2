using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace NexArr.Api.Migrations
{
    /// <inheritdoc />
    public partial class NexArrEntitlementReconciliationWorker : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "nexarr_entitlement_reconciliation_runs",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    Outcome = table.Column<string>(type: "character varying(32)", maxLength: 32, nullable: false),
                    DriftFoundCount = table.Column<int>(type: "integer", nullable: false),
                    GrantedCount = table.Column<int>(type: "integer", nullable: false),
                    RevokedCount = table.Column<int>(type: "integer", nullable: false),
                    SkippedCount = table.Column<int>(type: "integer", nullable: false),
                    SkipReason = table.Column<string>(type: "character varying(512)", maxLength: 512, nullable: true),
                    ProcessedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    CreatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_nexarr_entitlement_reconciliation_runs", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "nexarr_platform_entitlement_reconciliation_settings",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    IsEnabled = table.Column<bool>(type: "boolean", nullable: false),
                    AutoGrantFromLicense = table.Column<bool>(type: "boolean", nullable: false),
                    AutoRevokeStaleEntitlements = table.Column<bool>(type: "boolean", nullable: false),
                    UpdatedByUserId = table.Column<Guid>(type: "uuid", nullable: true),
                    CreatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_nexarr_platform_entitlement_reconciliation_settings", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "nexarr_tenant_product_licenses",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    TenantId = table.Column<Guid>(type: "uuid", nullable: false),
                    ProductKey = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: false),
                    Status = table.Column<string>(type: "character varying(32)", maxLength: 32, nullable: false),
                    ValidFrom = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    ValidTo = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    ExternalReference = table.Column<string>(type: "character varying(128)", maxLength: 128, nullable: true),
                    CreatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    ModifiedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_nexarr_tenant_product_licenses", x => x.Id);
                    table.ForeignKey(
                        name: "FK_nexarr_tenant_product_licenses_product_catalog_ProductKey",
                        column: x => x.ProductKey,
                        principalTable: "product_catalog",
                        principalColumn: "ProductKey",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_nexarr_tenant_product_licenses_tenants_TenantId",
                        column: x => x.TenantId,
                        principalTable: "tenants",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_nexarr_entitlement_reconciliation_runs_ProcessedAt",
                table: "nexarr_entitlement_reconciliation_runs",
                column: "ProcessedAt");

            migrationBuilder.CreateIndex(
                name: "IX_nexarr_tenant_product_licenses_ProductKey",
                table: "nexarr_tenant_product_licenses",
                column: "ProductKey");

            migrationBuilder.CreateIndex(
                name: "IX_nexarr_tenant_product_licenses_TenantId",
                table: "nexarr_tenant_product_licenses",
                column: "TenantId");

            migrationBuilder.CreateIndex(
                name: "IX_nexarr_tenant_product_licenses_TenantId_ProductKey",
                table: "nexarr_tenant_product_licenses",
                columns: new[] { "TenantId", "ProductKey" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_nexarr_tenant_product_licenses_ValidTo",
                table: "nexarr_tenant_product_licenses",
                column: "ValidTo");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "nexarr_entitlement_reconciliation_runs");

            migrationBuilder.DropTable(
                name: "nexarr_platform_entitlement_reconciliation_settings");

            migrationBuilder.DropTable(
                name: "nexarr_tenant_product_licenses");
        }
    }
}
