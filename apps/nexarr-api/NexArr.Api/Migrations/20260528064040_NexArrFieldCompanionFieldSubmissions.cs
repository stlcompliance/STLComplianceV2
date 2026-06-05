using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace NexArr.Api.Migrations
{
    /// <inheritdoc />
    public partial class NexArrFieldCompanionFieldSubmissions : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "nexarr_fieldcompanion_field_submissions",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    TenantId = table.Column<Guid>(type: "uuid", nullable: false),
                    UserId = table.Column<Guid>(type: "uuid", nullable: false),
                    TaskKey = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: false),
                    ProductKey = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: false),
                    SubmissionKind = table.Column<string>(type: "character varying(32)", maxLength: 32, nullable: false),
                    Status = table.Column<string>(type: "character varying(32)", maxLength: 32, nullable: false),
                    DetailMessage = table.Column<string>(type: "character varying(512)", maxLength: 512, nullable: true),
                    ClientSubmittedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    RecordedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_nexarr_fieldcompanion_field_submissions", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "nexarr_fieldcompanion_offline_actions",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    TenantId = table.Column<Guid>(type: "uuid", nullable: false),
                    UserId = table.Column<Guid>(type: "uuid", nullable: false),
                    IdempotencyKey = table.Column<string>(type: "character varying(128)", maxLength: 128, nullable: false),
                    ActionKind = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: false),
                    TaskKey = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: false),
                    ProductKey = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: false),
                    ClientCreatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    SyncedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_nexarr_fieldcompanion_offline_actions", x => x.Id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_nexarr_fieldcompanion_field_submissions_TenantId_UserId_TaskKey_~",
                table: "nexarr_fieldcompanion_field_submissions",
                columns: new[] { "TenantId", "UserId", "TaskKey", "RecordedAt" });

            migrationBuilder.CreateIndex(
                name: "IX_nexarr_fieldcompanion_offline_actions_TenantId_IdempotencyKey",
                table: "nexarr_fieldcompanion_offline_actions",
                columns: new[] { "TenantId", "IdempotencyKey" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_nexarr_fieldcompanion_offline_actions_TenantId_UserId_SyncedAt",
                table: "nexarr_fieldcompanion_offline_actions",
                columns: new[] { "TenantId", "UserId", "SyncedAt" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "nexarr_fieldcompanion_field_submissions");

            migrationBuilder.DropTable(
                name: "nexarr_fieldcompanion_offline_actions");
        }
    }
}
