using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace RecordArr.Api.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddRecordArrDurableAccessControls : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "recordarr_access_grants",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    TenantId = table.Column<Guid>(type: "uuid", nullable: false),
                    AccessGrantId = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: false),
                    RecordId = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: false),
                    GranteeType = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: false),
                    GranteeRef = table.Column<string>(type: "character varying(160)", maxLength: 160, nullable: false),
                    Permission = table.Column<string>(type: "character varying(96)", maxLength: 96, nullable: false),
                    Status = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: false),
                    GrantedByPersonId = table.Column<string>(type: "character varying(128)", maxLength: 128, nullable: false),
                    GrantedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    ExpiresAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    RevokedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    PayloadJson = table.Column<string>(type: "jsonb", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_recordarr_access_grants", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "recordarr_access_logs",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    TenantId = table.Column<Guid>(type: "uuid", nullable: false),
                    AccessLogId = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: false),
                    RecordId = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: false),
                    Action = table.Column<string>(type: "character varying(96)", maxLength: 96, nullable: false),
                    Result = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: false),
                    ActorPersonId = table.Column<string>(type: "character varying(128)", maxLength: 128, nullable: true),
                    ActorServiceClientId = table.Column<string>(type: "character varying(128)", maxLength: 128, nullable: true),
                    ExternalShareId = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: true),
                    OccurredAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    ReasonCode = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: true),
                    PayloadJson = table.Column<string>(type: "jsonb", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_recordarr_access_logs", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "recordarr_access_policies",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    TenantId = table.Column<Guid>(type: "uuid", nullable: false),
                    AccessPolicyId = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: false),
                    RecordId = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: false),
                    PolicyType = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: false),
                    Status = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: false),
                    PayloadJson = table.Column<string>(type: "jsonb", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_recordarr_access_policies", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "recordarr_external_shares",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    TenantId = table.Column<Guid>(type: "uuid", nullable: false),
                    ExternalShareId = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: false),
                    ShareNumber = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: false),
                    RecordId = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: false),
                    SharePurpose = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: false),
                    Status = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: false),
                    RecipientEmail = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: false),
                    CreatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    CreatedByPersonId = table.Column<string>(type: "character varying(128)", maxLength: 128, nullable: false),
                    ExpiresAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    RevokedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    LastAccessedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    AccessCount = table.Column<int>(type: "integer", nullable: false),
                    PayloadJson = table.Column<string>(type: "jsonb", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_recordarr_external_shares", x => x.Id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_recordarr_access_grants_TenantId",
                table: "recordarr_access_grants",
                column: "TenantId");

            migrationBuilder.CreateIndex(
                name: "IX_recordarr_access_grants_TenantId_AccessGrantId",
                table: "recordarr_access_grants",
                columns: new[] { "TenantId", "AccessGrantId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_recordarr_access_grants_TenantId_GranteeType_GranteeRef_Per~",
                table: "recordarr_access_grants",
                columns: new[] { "TenantId", "GranteeType", "GranteeRef", "Permission" });

            migrationBuilder.CreateIndex(
                name: "IX_recordarr_access_grants_TenantId_RecordId_Status",
                table: "recordarr_access_grants",
                columns: new[] { "TenantId", "RecordId", "Status" });

            migrationBuilder.CreateIndex(
                name: "IX_recordarr_access_logs_TenantId",
                table: "recordarr_access_logs",
                column: "TenantId");

            migrationBuilder.CreateIndex(
                name: "IX_recordarr_access_logs_TenantId_AccessLogId",
                table: "recordarr_access_logs",
                columns: new[] { "TenantId", "AccessLogId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_recordarr_access_logs_TenantId_ExternalShareId_OccurredAt",
                table: "recordarr_access_logs",
                columns: new[] { "TenantId", "ExternalShareId", "OccurredAt" });

            migrationBuilder.CreateIndex(
                name: "IX_recordarr_access_logs_TenantId_RecordId_OccurredAt",
                table: "recordarr_access_logs",
                columns: new[] { "TenantId", "RecordId", "OccurredAt" });

            migrationBuilder.CreateIndex(
                name: "IX_recordarr_access_policies_TenantId",
                table: "recordarr_access_policies",
                column: "TenantId");

            migrationBuilder.CreateIndex(
                name: "IX_recordarr_access_policies_TenantId_AccessPolicyId",
                table: "recordarr_access_policies",
                columns: new[] { "TenantId", "AccessPolicyId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_recordarr_access_policies_TenantId_RecordId_Status",
                table: "recordarr_access_policies",
                columns: new[] { "TenantId", "RecordId", "Status" });

            migrationBuilder.CreateIndex(
                name: "IX_recordarr_external_shares_TenantId",
                table: "recordarr_external_shares",
                column: "TenantId");

            migrationBuilder.CreateIndex(
                name: "IX_recordarr_external_shares_TenantId_ExternalShareId",
                table: "recordarr_external_shares",
                columns: new[] { "TenantId", "ExternalShareId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_recordarr_external_shares_TenantId_RecordId_Status",
                table: "recordarr_external_shares",
                columns: new[] { "TenantId", "RecordId", "Status" });

            migrationBuilder.CreateIndex(
                name: "IX_recordarr_external_shares_TenantId_Status_ExpiresAt",
                table: "recordarr_external_shares",
                columns: new[] { "TenantId", "Status", "ExpiresAt" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "recordarr_access_grants");

            migrationBuilder.DropTable(
                name: "recordarr_access_logs");

            migrationBuilder.DropTable(
                name: "recordarr_access_policies");

            migrationBuilder.DropTable(
                name: "recordarr_external_shares");
        }
    }
}
