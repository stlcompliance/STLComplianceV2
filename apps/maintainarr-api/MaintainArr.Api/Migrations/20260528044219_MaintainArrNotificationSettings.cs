using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace MaintainArr.Api.Migrations
{
    /// <inheritdoc />
    public partial class MaintainArrNotificationSettings : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "maintainarr_notification_dispatches",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    TenantId = table.Column<Guid>(type: "uuid", nullable: false),
                    EventKind = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: false),
                    AssetId = table.Column<Guid>(type: "uuid", nullable: false),
                    RelatedEntityType = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: false),
                    RelatedEntityId = table.Column<Guid>(type: "uuid", nullable: false),
                    DispatchStatus = table.Column<string>(type: "character varying(32)", maxLength: 32, nullable: false),
                    WebhookHost = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: true),
                    HttpStatusCode = table.Column<int>(type: "integer", nullable: true),
                    ErrorMessage = table.Column<string>(type: "character varying(512)", maxLength: 512, nullable: true),
                    CreatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    DispatchedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_maintainarr_notification_dispatches", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "maintainarr_tenant_notification_settings",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    TenantId = table.Column<Guid>(type: "uuid", nullable: false),
                    IsEnabled = table.Column<bool>(type: "boolean", nullable: false),
                    NotificationWebhookUrl = table.Column<string>(type: "character varying(2048)", maxLength: 2048, nullable: true),
                    NotifyOnWorkOrderCreated = table.Column<bool>(type: "boolean", nullable: false),
                    NotifyOnPmScheduleDue = table.Column<bool>(type: "boolean", nullable: false),
                    NotifyOnPmScheduleOverdue = table.Column<bool>(type: "boolean", nullable: false),
                    UpdatedByUserId = table.Column<Guid>(type: "uuid", nullable: true),
                    CreatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_maintainarr_tenant_notification_settings", x => x.Id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_maintainarr_notification_dispatches_TenantId",
                table: "maintainarr_notification_dispatches",
                column: "TenantId");

            migrationBuilder.CreateIndex(
                name: "IX_maintainarr_notification_dispatches_TenantId_DispatchStatus~",
                table: "maintainarr_notification_dispatches",
                columns: new[] { "TenantId", "DispatchStatus", "CreatedAt" });

            migrationBuilder.CreateIndex(
                name: "IX_maintainarr_notification_dispatches_TenantId_EventKind_Rela~",
                table: "maintainarr_notification_dispatches",
                columns: new[] { "TenantId", "EventKind", "RelatedEntityType", "RelatedEntityId" });

            migrationBuilder.CreateIndex(
                name: "IX_maintainarr_tenant_notification_settings_TenantId",
                table: "maintainarr_tenant_notification_settings",
                column: "TenantId",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "maintainarr_notification_dispatches");

            migrationBuilder.DropTable(
                name: "maintainarr_tenant_notification_settings");
        }
    }
}
