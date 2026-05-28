using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace TrainArr.Api.Migrations
{
    /// <inheritdoc />
    public partial class TrainArrNotificationDispatchEnhancements : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_trainarr_training_notification_dispatches_TenantId_Dispatch~",
                table: "trainarr_training_notification_dispatches");

            migrationBuilder.AddColumn<int>(
                name: "AttemptCount",
                table: "trainarr_training_notification_dispatches",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<DateTimeOffset>(
                name: "NextRetryAt",
                table: "trainarr_training_notification_dispatches",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.AddColumn<DateTimeOffset>(
                name: "UpdatedAt",
                table: "trainarr_training_notification_dispatches",
                type: "timestamp with time zone",
                nullable: false,
                defaultValue: new DateTimeOffset(new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)));

            migrationBuilder.AddColumn<int>(
                name: "MaxAttempts",
                table: "trainarr_tenant_training_notification_settings",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<bool>(
                name: "NotifyOnAssignmentCompleted",
                table: "trainarr_tenant_training_notification_settings",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "NotifyOnQualificationIssued",
                table: "trainarr_tenant_training_notification_settings",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "NotifyOnQualificationRevoked",
                table: "trainarr_tenant_training_notification_settings",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "NotifyOnQualificationSuspended",
                table: "trainarr_tenant_training_notification_settings",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<int>(
                name: "RetryIntervalMinutes",
                table: "trainarr_tenant_training_notification_settings",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.CreateIndex(
                name: "IX_trainarr_training_notification_dispatches_TenantId_Dispatch~",
                table: "trainarr_training_notification_dispatches",
                columns: new[] { "TenantId", "DispatchStatus", "NextRetryAt", "CreatedAt" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_trainarr_training_notification_dispatches_TenantId_Dispatch~",
                table: "trainarr_training_notification_dispatches");

            migrationBuilder.DropColumn(
                name: "AttemptCount",
                table: "trainarr_training_notification_dispatches");

            migrationBuilder.DropColumn(
                name: "NextRetryAt",
                table: "trainarr_training_notification_dispatches");

            migrationBuilder.DropColumn(
                name: "UpdatedAt",
                table: "trainarr_training_notification_dispatches");

            migrationBuilder.DropColumn(
                name: "MaxAttempts",
                table: "trainarr_tenant_training_notification_settings");

            migrationBuilder.DropColumn(
                name: "NotifyOnAssignmentCompleted",
                table: "trainarr_tenant_training_notification_settings");

            migrationBuilder.DropColumn(
                name: "NotifyOnQualificationIssued",
                table: "trainarr_tenant_training_notification_settings");

            migrationBuilder.DropColumn(
                name: "NotifyOnQualificationRevoked",
                table: "trainarr_tenant_training_notification_settings");

            migrationBuilder.DropColumn(
                name: "NotifyOnQualificationSuspended",
                table: "trainarr_tenant_training_notification_settings");

            migrationBuilder.DropColumn(
                name: "RetryIntervalMinutes",
                table: "trainarr_tenant_training_notification_settings");

            migrationBuilder.CreateIndex(
                name: "IX_trainarr_training_notification_dispatches_TenantId_Dispatch~",
                table: "trainarr_training_notification_dispatches",
                columns: new[] { "TenantId", "DispatchStatus", "CreatedAt" });
        }
    }
}
