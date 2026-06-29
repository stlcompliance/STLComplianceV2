using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace RecordArr.Api.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddRecordArrRetentionOutboxEscalation : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameIndex(
                name: "IX_recordarr_retention_scheduler_outbox_messages_TenantId_Sta~1",
                table: "recordarr_retention_scheduler_outbox_messages",
                newName: "IX_recordarr_retention_scheduler_outbox_messages_TenantId_Sta~2");

            migrationBuilder.AddColumn<string>(
                name: "DeliveryChannel",
                table: "recordarr_retention_scheduler_outbox_messages",
                type: "character varying(64)",
                maxLength: 64,
                nullable: false,
                defaultValue: "in_app");

            migrationBuilder.AddColumn<DateTimeOffset>(
                name: "DueAt",
                table: "recordarr_retention_scheduler_outbox_messages",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.AddColumn<DateTimeOffset>(
                name: "EscalateAfter",
                table: "recordarr_retention_scheduler_outbox_messages",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.AddColumn<DateTimeOffset>(
                name: "EscalatedAt",
                table: "recordarr_retention_scheduler_outbox_messages",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "EscalatedToRecipientRef",
                table: "recordarr_retention_scheduler_outbox_messages",
                type: "character varying(160)",
                maxLength: 160,
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "EscalationLevel",
                table: "recordarr_retention_scheduler_outbox_messages",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<string>(
                name: "ExternalProviderRef",
                table: "recordarr_retention_scheduler_outbox_messages",
                type: "character varying(160)",
                maxLength: 160,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "RecipientRef",
                table: "recordarr_retention_scheduler_outbox_messages",
                type: "character varying(160)",
                maxLength: 160,
                nullable: false,
                defaultValue: "role:records");

            migrationBuilder.CreateIndex(
                name: "IX_recordarr_retention_scheduler_outbox_messages_TenantId_Sta~1",
                table: "recordarr_retention_scheduler_outbox_messages",
                columns: new[] { "TenantId", "Status", "EscalateAfter" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_recordarr_retention_scheduler_outbox_messages_TenantId_Sta~1",
                table: "recordarr_retention_scheduler_outbox_messages");

            migrationBuilder.DropColumn(
                name: "DeliveryChannel",
                table: "recordarr_retention_scheduler_outbox_messages");

            migrationBuilder.DropColumn(
                name: "DueAt",
                table: "recordarr_retention_scheduler_outbox_messages");

            migrationBuilder.DropColumn(
                name: "EscalateAfter",
                table: "recordarr_retention_scheduler_outbox_messages");

            migrationBuilder.DropColumn(
                name: "EscalatedAt",
                table: "recordarr_retention_scheduler_outbox_messages");

            migrationBuilder.DropColumn(
                name: "EscalatedToRecipientRef",
                table: "recordarr_retention_scheduler_outbox_messages");

            migrationBuilder.DropColumn(
                name: "EscalationLevel",
                table: "recordarr_retention_scheduler_outbox_messages");

            migrationBuilder.DropColumn(
                name: "ExternalProviderRef",
                table: "recordarr_retention_scheduler_outbox_messages");

            migrationBuilder.DropColumn(
                name: "RecipientRef",
                table: "recordarr_retention_scheduler_outbox_messages");

            migrationBuilder.RenameIndex(
                name: "IX_recordarr_retention_scheduler_outbox_messages_TenantId_Sta~2",
                table: "recordarr_retention_scheduler_outbox_messages",
                newName: "IX_recordarr_retention_scheduler_outbox_messages_TenantId_Sta~1");
        }
    }
}
