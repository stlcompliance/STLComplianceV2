using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace CustomArr.Api.Migrations
{
    /// <inheritdoc />
    public partial class FixPendingModelChanges : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "print_export_logs",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    TenantId = table.Column<Guid>(type: "uuid", nullable: false),
                    ProductKey = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: false),
                    SourceEntityType = table.Column<string>(type: "character varying(128)", maxLength: 128, nullable: false),
                    SourceEntityId = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: false),
                    SourceDisplayRef = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: false),
                    TemplateKey = table.Column<string>(type: "character varying(160)", maxLength: 160, nullable: false),
                    TemplateVersion = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: false),
                    Action = table.Column<string>(type: "character varying(32)", maxLength: 32, nullable: false),
                    DocumentStatus = table.Column<string>(type: "character varying(32)", maxLength: 32, nullable: false),
                    RequestedByPersonId = table.Column<Guid>(type: "uuid", nullable: false),
                    RequestedAtUtc = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    CompletedAtUtc = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    RecordArrDocumentId = table.Column<string>(type: "character varying(128)", maxLength: 128, nullable: true),
                    FileName = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: true),
                    ContentHash = table.Column<string>(type: "character varying(128)", maxLength: 128, nullable: true),
                    ReprintReason = table.Column<string>(type: "character varying(1024)", maxLength: 1024, nullable: true),
                    FailureReason = table.Column<string>(type: "character varying(1024)", maxLength: 1024, nullable: true),
                    MetadataJson = table.Column<string>(type: "jsonb", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_print_export_logs", x => x.Id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_print_export_logs_action_lookup",
                table: "print_export_logs",
                columns: new[] { "TenantId", "ProductKey", "Action", "RequestedAtUtc" });

            migrationBuilder.CreateIndex(
                name: "IX_print_export_logs_lookup",
                table: "print_export_logs",
                columns: new[] { "TenantId", "ProductKey", "SourceEntityType", "SourceEntityId", "RequestedAtUtc" });

            migrationBuilder.CreateIndex(
                name: "IX_print_export_logs_TenantId",
                table: "print_export_logs",
                column: "TenantId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "print_export_logs");
        }
    }
}
