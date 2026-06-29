using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace RecordArr.Api.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddRecordArrRetentionSchedulerOutboxDelivery : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<DateTimeOffset>(
                name: "DeliveredAt",
                table: "recordarr_retention_scheduler_outbox_messages",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "DeliveredByPersonId",
                table: "recordarr_retention_scheduler_outbox_messages",
                type: "character varying(128)",
                maxLength: 128,
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "DeliveryAttemptCount",
                table: "recordarr_retention_scheduler_outbox_messages",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<DateTimeOffset>(
                name: "LastAttemptAt",
                table: "recordarr_retention_scheduler_outbox_messages",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_recordarr_retention_scheduler_outbox_messages_TenantId_Sta~1",
                table: "recordarr_retention_scheduler_outbox_messages",
                columns: new[] { "TenantId", "Status", "LastAttemptAt" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_recordarr_retention_scheduler_outbox_messages_TenantId_Sta~1",
                table: "recordarr_retention_scheduler_outbox_messages");

            migrationBuilder.DropColumn(
                name: "DeliveredAt",
                table: "recordarr_retention_scheduler_outbox_messages");

            migrationBuilder.DropColumn(
                name: "DeliveredByPersonId",
                table: "recordarr_retention_scheduler_outbox_messages");

            migrationBuilder.DropColumn(
                name: "DeliveryAttemptCount",
                table: "recordarr_retention_scheduler_outbox_messages");

            migrationBuilder.DropColumn(
                name: "LastAttemptAt",
                table: "recordarr_retention_scheduler_outbox_messages");
        }
    }
}
