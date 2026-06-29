using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace RecordArr.Api.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddRecordArrDurableRecordContextStore : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "recordarr_record_comments",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    TenantId = table.Column<Guid>(type: "uuid", nullable: false),
                    CommentId = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: false),
                    RecordId = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: false),
                    Visibility = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: false),
                    CreatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    CreatedByPersonId = table.Column<string>(type: "character varying(128)", maxLength: 128, nullable: false),
                    EditedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    EditedByPersonId = table.Column<string>(type: "character varying(128)", maxLength: 128, nullable: true),
                    PayloadJson = table.Column<string>(type: "jsonb", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_recordarr_record_comments", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "recordarr_record_links",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    TenantId = table.Column<Guid>(type: "uuid", nullable: false),
                    RecordLinkId = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: false),
                    RecordId = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: false),
                    LinkedRecordId = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: true),
                    SourceObjectRef = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: true),
                    LinkType = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: false),
                    CreatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    CreatedByPersonId = table.Column<string>(type: "character varying(128)", maxLength: 128, nullable: false),
                    PayloadJson = table.Column<string>(type: "jsonb", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_recordarr_record_links", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "recordarr_record_metadata",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    TenantId = table.Column<Guid>(type: "uuid", nullable: false),
                    MetadataId = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: false),
                    RecordId = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: false),
                    Key = table.Column<string>(type: "character varying(128)", maxLength: 128, nullable: false),
                    ValueType = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: false),
                    Source = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: false),
                    ConfidenceScore = table.Column<decimal>(type: "numeric", nullable: false),
                    Verified = table.Column<bool>(type: "boolean", nullable: false),
                    CreatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    VerifiedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    PayloadJson = table.Column<string>(type: "jsonb", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_recordarr_record_metadata", x => x.Id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_recordarr_record_comments_TenantId",
                table: "recordarr_record_comments",
                column: "TenantId");

            migrationBuilder.CreateIndex(
                name: "IX_recordarr_record_comments_TenantId_CommentId",
                table: "recordarr_record_comments",
                columns: new[] { "TenantId", "CommentId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_recordarr_record_comments_TenantId_RecordId_CreatedAt",
                table: "recordarr_record_comments",
                columns: new[] { "TenantId", "RecordId", "CreatedAt" });

            migrationBuilder.CreateIndex(
                name: "IX_recordarr_record_links_TenantId",
                table: "recordarr_record_links",
                column: "TenantId");

            migrationBuilder.CreateIndex(
                name: "IX_recordarr_record_links_TenantId_RecordId_LinkType",
                table: "recordarr_record_links",
                columns: new[] { "TenantId", "RecordId", "LinkType" });

            migrationBuilder.CreateIndex(
                name: "IX_recordarr_record_links_TenantId_RecordLinkId",
                table: "recordarr_record_links",
                columns: new[] { "TenantId", "RecordLinkId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_recordarr_record_links_TenantId_SourceObjectRef",
                table: "recordarr_record_links",
                columns: new[] { "TenantId", "SourceObjectRef" });

            migrationBuilder.CreateIndex(
                name: "IX_recordarr_record_metadata_TenantId",
                table: "recordarr_record_metadata",
                column: "TenantId");

            migrationBuilder.CreateIndex(
                name: "IX_recordarr_record_metadata_TenantId_MetadataId",
                table: "recordarr_record_metadata",
                columns: new[] { "TenantId", "MetadataId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_recordarr_record_metadata_TenantId_RecordId_Key",
                table: "recordarr_record_metadata",
                columns: new[] { "TenantId", "RecordId", "Key" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "recordarr_record_comments");

            migrationBuilder.DropTable(
                name: "recordarr_record_links");

            migrationBuilder.DropTable(
                name: "recordarr_record_metadata");
        }
    }
}
