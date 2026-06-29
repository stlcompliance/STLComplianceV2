using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace RecordArr.Api.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddRecordArrDurableRetentionHolds : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "recordarr_disposal_reviews",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    TenantId = table.Column<Guid>(type: "uuid", nullable: false),
                    DisposalReviewId = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: false),
                    RecordId = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: false),
                    RetentionStatusRef = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: false),
                    ProposedAction = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: false),
                    Status = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: false),
                    RequestedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    RequestedByPersonId = table.Column<string>(type: "character varying(128)", maxLength: 128, nullable: false),
                    ReviewedByPersonId = table.Column<string>(type: "character varying(128)", maxLength: 128, nullable: true),
                    ReviewedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    CompletedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    PayloadJson = table.Column<string>(type: "jsonb", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_recordarr_disposal_reviews", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "recordarr_legal_holds",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    TenantId = table.Column<Guid>(type: "uuid", nullable: false),
                    LegalHoldId = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: false),
                    HoldNumber = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: false),
                    Title = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: false),
                    Status = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: false),
                    HoldType = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: false),
                    SourceProduct = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: false),
                    SourceObjectType = table.Column<string>(type: "character varying(96)", maxLength: 96, nullable: false),
                    SourceObjectId = table.Column<string>(type: "character varying(160)", maxLength: 160, nullable: false),
                    CreatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    CreatedByPersonId = table.Column<string>(type: "character varying(128)", maxLength: 128, nullable: false),
                    ActivatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    ReleasedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    ReleasedByPersonId = table.Column<string>(type: "character varying(128)", maxLength: 128, nullable: true),
                    PayloadJson = table.Column<string>(type: "jsonb", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_recordarr_legal_holds", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "recordarr_retention_statuses",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    TenantId = table.Column<Guid>(type: "uuid", nullable: false),
                    RetentionStatusId = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: false),
                    RecordId = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: false),
                    RetentionPolicyRef = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: false),
                    Status = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: false),
                    RetentionStartAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    RetentionExpiresAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    NextReviewAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    LastReviewedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    ReviewedByPersonId = table.Column<string>(type: "character varying(128)", maxLength: 128, nullable: true),
                    DisposalReviewRef = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: true),
                    PayloadJson = table.Column<string>(type: "jsonb", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_recordarr_retention_statuses", x => x.Id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_recordarr_disposal_reviews_TenantId",
                table: "recordarr_disposal_reviews",
                column: "TenantId");

            migrationBuilder.CreateIndex(
                name: "IX_recordarr_disposal_reviews_TenantId_DisposalReviewId",
                table: "recordarr_disposal_reviews",
                columns: new[] { "TenantId", "DisposalReviewId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_recordarr_disposal_reviews_TenantId_RecordId_Status",
                table: "recordarr_disposal_reviews",
                columns: new[] { "TenantId", "RecordId", "Status" });

            migrationBuilder.CreateIndex(
                name: "IX_recordarr_disposal_reviews_TenantId_RetentionStatusRef",
                table: "recordarr_disposal_reviews",
                columns: new[] { "TenantId", "RetentionStatusRef" });

            migrationBuilder.CreateIndex(
                name: "IX_recordarr_legal_holds_TenantId",
                table: "recordarr_legal_holds",
                column: "TenantId");

            migrationBuilder.CreateIndex(
                name: "IX_recordarr_legal_holds_TenantId_LegalHoldId",
                table: "recordarr_legal_holds",
                columns: new[] { "TenantId", "LegalHoldId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_recordarr_legal_holds_TenantId_SourceProduct_SourceObjectTy~",
                table: "recordarr_legal_holds",
                columns: new[] { "TenantId", "SourceProduct", "SourceObjectType", "SourceObjectId" });

            migrationBuilder.CreateIndex(
                name: "IX_recordarr_legal_holds_TenantId_Status_CreatedAt",
                table: "recordarr_legal_holds",
                columns: new[] { "TenantId", "Status", "CreatedAt" });

            migrationBuilder.CreateIndex(
                name: "IX_recordarr_retention_statuses_TenantId",
                table: "recordarr_retention_statuses",
                column: "TenantId");

            migrationBuilder.CreateIndex(
                name: "IX_recordarr_retention_statuses_TenantId_RecordId",
                table: "recordarr_retention_statuses",
                columns: new[] { "TenantId", "RecordId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_recordarr_retention_statuses_TenantId_RetentionStatusId",
                table: "recordarr_retention_statuses",
                columns: new[] { "TenantId", "RetentionStatusId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_recordarr_retention_statuses_TenantId_Status_NextReviewAt",
                table: "recordarr_retention_statuses",
                columns: new[] { "TenantId", "Status", "NextReviewAt" });

            migrationBuilder.CreateIndex(
                name: "IX_recordarr_retention_statuses_TenantId_Status_RetentionExpir~",
                table: "recordarr_retention_statuses",
                columns: new[] { "TenantId", "Status", "RetentionExpiresAt" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "recordarr_disposal_reviews");

            migrationBuilder.DropTable(
                name: "recordarr_legal_holds");

            migrationBuilder.DropTable(
                name: "recordarr_retention_statuses");
        }
    }
}
