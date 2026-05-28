using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace TrainArr.Api.Migrations
{
    /// <inheritdoc />
    public partial class TrainArrStaffarrPublicationRetry : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "trainarr_staffarr_publication_deliveries",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    TenantId = table.Column<Guid>(type: "uuid", nullable: false),
                    CertificationPublicationId = table.Column<Guid>(type: "uuid", nullable: false),
                    StaffarrPersonId = table.Column<Guid>(type: "uuid", nullable: false),
                    OperationKind = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: false),
                    PayloadJson = table.Column<string>(type: "character varying(8192)", maxLength: 8192, nullable: false),
                    DeliveryStatus = table.Column<string>(type: "character varying(32)", maxLength: 32, nullable: false),
                    AttemptCount = table.Column<int>(type: "integer", nullable: false),
                    NextRetryAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    HttpStatusCode = table.Column<int>(type: "integer", nullable: true),
                    ErrorMessage = table.Column<string>(type: "character varying(512)", maxLength: 512, nullable: true),
                    CreatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    DeliveredAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_trainarr_staffarr_publication_deliveries", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "trainarr_tenant_staffarr_publication_settings",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    TenantId = table.Column<Guid>(type: "uuid", nullable: false),
                    IsEnabled = table.Column<bool>(type: "boolean", nullable: false),
                    MaxAttempts = table.Column<int>(type: "integer", nullable: false),
                    RetryIntervalMinutes = table.Column<int>(type: "integer", nullable: false),
                    UpdatedByUserId = table.Column<Guid>(type: "uuid", nullable: true),
                    CreatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_trainarr_tenant_staffarr_publication_settings", x => x.Id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_trainarr_staffarr_publication_deliveries_TenantId",
                table: "trainarr_staffarr_publication_deliveries",
                column: "TenantId");

            migrationBuilder.CreateIndex(
                name: "IX_trainarr_staffarr_publication_deliveries_TenantId_Certifica~",
                table: "trainarr_staffarr_publication_deliveries",
                columns: new[] { "TenantId", "CertificationPublicationId", "OperationKind", "DeliveryStatus" });

            migrationBuilder.CreateIndex(
                name: "IX_trainarr_staffarr_publication_deliveries_TenantId_DeliveryS~",
                table: "trainarr_staffarr_publication_deliveries",
                columns: new[] { "TenantId", "DeliveryStatus", "NextRetryAt", "CreatedAt" });

            migrationBuilder.CreateIndex(
                name: "IX_trainarr_tenant_staffarr_publication_settings_TenantId",
                table: "trainarr_tenant_staffarr_publication_settings",
                column: "TenantId",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "trainarr_staffarr_publication_deliveries");

            migrationBuilder.DropTable(
                name: "trainarr_tenant_staffarr_publication_settings");
        }
    }
}
