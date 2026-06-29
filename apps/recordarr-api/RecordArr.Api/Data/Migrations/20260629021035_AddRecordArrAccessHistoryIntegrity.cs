using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace RecordArr.Api.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddRecordArrAccessHistoryIntegrity : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "AccessLogHash",
                table: "recordarr_access_logs",
                type: "character varying(128)",
                maxLength: 128,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "PreviousAccessLogHash",
                table: "recordarr_access_logs",
                type: "character varying(128)",
                maxLength: 128,
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_recordarr_access_logs_TenantId_AccessLogHash",
                table: "recordarr_access_logs",
                columns: new[] { "TenantId", "AccessLogHash" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_recordarr_access_logs_TenantId_AccessLogHash",
                table: "recordarr_access_logs");

            migrationBuilder.DropColumn(
                name: "AccessLogHash",
                table: "recordarr_access_logs");

            migrationBuilder.DropColumn(
                name: "PreviousAccessLogHash",
                table: "recordarr_access_logs");
        }
    }
}
