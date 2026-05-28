using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace StaffArr.Api.Migrations
{
    /// <inheritdoc />
    public partial class StaffArrPersonExportDeliveryNotifications : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "NotificationWebhookUrl",
                table: "staffarr_tenant_person_export_schedules",
                type: "character varying(2048)",
                maxLength: 2048,
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "NotifyOnFailure",
                table: "staffarr_tenant_person_export_schedules",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "NotifyOnSuccess",
                table: "staffarr_tenant_person_export_schedules",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.CreateTable(
                name: "staffarr_person_export_delivery_notifications",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    TenantId = table.Column<Guid>(type: "uuid", nullable: false),
                    DeliveryRunId = table.Column<Guid>(type: "uuid", nullable: true),
                    EventKind = table.Column<string>(type: "character varying(32)", maxLength: 32, nullable: false),
                    DeliveryStatus = table.Column<string>(type: "character varying(32)", maxLength: 32, nullable: false),
                    WebhookHost = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: true),
                    HttpStatusCode = table.Column<int>(type: "integer", nullable: true),
                    ErrorMessage = table.Column<string>(type: "character varying(512)", maxLength: 512, nullable: true),
                    ExportId = table.Column<Guid>(type: "uuid", nullable: true),
                    PersonCount = table.Column<int>(type: "integer", nullable: true),
                    AttemptedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    CreatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_staffarr_person_export_delivery_notifications", x => x.Id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_staffarr_person_export_delivery_notifications_AttemptedAt",
                table: "staffarr_person_export_delivery_notifications",
                column: "AttemptedAt");

            migrationBuilder.CreateIndex(
                name: "IX_staffarr_person_export_delivery_notifications_TenantId",
                table: "staffarr_person_export_delivery_notifications",
                column: "TenantId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "staffarr_person_export_delivery_notifications");

            migrationBuilder.DropColumn(
                name: "NotificationWebhookUrl",
                table: "staffarr_tenant_person_export_schedules");

            migrationBuilder.DropColumn(
                name: "NotifyOnFailure",
                table: "staffarr_tenant_person_export_schedules");

            migrationBuilder.DropColumn(
                name: "NotifyOnSuccess",
                table: "staffarr_tenant_person_export_schedules");
        }
    }
}
