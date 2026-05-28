using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace TrainArr.Api.Migrations
{
    /// <inheritdoc />
    public partial class TrainArrIntegrationSettings : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "trainarr_tenant_integration_settings",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    TenantId = table.Column<Guid>(type: "uuid", nullable: false),
                    StaffArrIntegrationEnabled = table.Column<bool>(type: "boolean", nullable: false),
                    StaffArrIncidentIntakeEnabled = table.Column<bool>(type: "boolean", nullable: false),
                    StaffArrPublicationDeliveryEnabled = table.Column<bool>(type: "boolean", nullable: false),
                    ComplianceCoreIntegrationEnabled = table.Column<bool>(type: "boolean", nullable: false),
                    ComplianceCoreQualificationChecksEnabled = table.Column<bool>(type: "boolean", nullable: false),
                    RoutarrIntegrationEnabled = table.Column<bool>(type: "boolean", nullable: false),
                    RoutarrQualificationDispatchEnabled = table.Column<bool>(type: "boolean", nullable: false),
                    UpdatedByUserId = table.Column<Guid>(type: "uuid", nullable: true),
                    CreatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_trainarr_tenant_integration_settings", x => x.Id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_trainarr_tenant_integration_settings_TenantId",
                table: "trainarr_tenant_integration_settings",
                column: "TenantId",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "trainarr_tenant_integration_settings");
        }
    }
}
