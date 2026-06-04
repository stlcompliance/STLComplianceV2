using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace AssurArr.Api.Migrations
{
    /// <inheritdoc />
    public partial class AddQualityReviewsAndReleases : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "assurarr_quality_releases",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    TenantId = table.Column<Guid>(type: "uuid", nullable: false),
                    Number = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: false),
                    Title = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: false),
                    Description = table.Column<string>(type: "character varying(4000)", maxLength: 4000, nullable: false),
                    Severity = table.Column<string>(type: "character varying(32)", maxLength: 32, nullable: false),
                    Status = table.Column<string>(type: "character varying(32)", maxLength: 32, nullable: false),
                    SourceProduct = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: true),
                    SourceObjectRef = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: true),
                    AffectedObjectRefs = table.Column<string[]>(type: "text[]", nullable: false),
                    OwnerPersonId = table.Column<Guid>(type: "uuid", nullable: true),
                    RecordRefs = table.Column<string[]>(type: "text[]", nullable: false),
                    CreatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    ClosedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    ClosedByPersonId = table.Column<Guid>(type: "uuid", nullable: true),
                    ClosureSummary = table.Column<string>(type: "character varying(4000)", maxLength: 4000, nullable: true),
                    HoldRef = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: false),
                    ReleaseType = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: false),
                    RequestedByPersonId = table.Column<Guid>(type: "uuid", nullable: true),
                    RequestedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    ApprovedByPersonId = table.Column<Guid>(type: "uuid", nullable: true),
                    ApprovedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    ExecutedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    Conditions = table.Column<string>(type: "character varying(4000)", maxLength: 4000, nullable: true),
                    ExpirationAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    EvidenceRecordRefs = table.Column<string[]>(type: "text[]", nullable: false),
                    Notes = table.Column<string>(type: "character varying(4000)", maxLength: 4000, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_assurarr_quality_releases", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "assurarr_quality_reviews",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    TenantId = table.Column<Guid>(type: "uuid", nullable: false),
                    Number = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: false),
                    Title = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: false),
                    Description = table.Column<string>(type: "character varying(4000)", maxLength: 4000, nullable: false),
                    Severity = table.Column<string>(type: "character varying(32)", maxLength: 32, nullable: false),
                    Status = table.Column<string>(type: "character varying(32)", maxLength: 32, nullable: false),
                    SourceProduct = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: true),
                    SourceObjectRef = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: true),
                    AffectedObjectRefs = table.Column<string[]>(type: "text[]", nullable: false),
                    OwnerPersonId = table.Column<Guid>(type: "uuid", nullable: true),
                    RecordRefs = table.Column<string[]>(type: "text[]", nullable: false),
                    CreatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    ClosedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    ClosedByPersonId = table.Column<Guid>(type: "uuid", nullable: true),
                    ClosureSummary = table.Column<string>(type: "character varying(4000)", maxLength: 4000, nullable: true),
                    ReviewType = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: false),
                    SourceReviewRef = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: true),
                    ReviewerPersonId = table.Column<Guid>(type: "uuid", nullable: true),
                    RequestedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    DueAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    DecisionAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    DecisionReason = table.Column<string>(type: "character varying(4000)", maxLength: 4000, nullable: true),
                    RequiredEvidenceRefs = table.Column<string[]>(type: "text[]", nullable: false),
                    SubmittedEvidenceRefs = table.Column<string[]>(type: "text[]", nullable: false),
                    Notes = table.Column<string>(type: "character varying(4000)", maxLength: 4000, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_assurarr_quality_reviews", x => x.Id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_assurarr_quality_releases_TenantId",
                table: "assurarr_quality_releases",
                column: "TenantId");

            migrationBuilder.CreateIndex(
                name: "IX_assurarr_quality_releases_TenantId_HoldRef",
                table: "assurarr_quality_releases",
                columns: new[] { "TenantId", "HoldRef" });

            migrationBuilder.CreateIndex(
                name: "IX_assurarr_quality_releases_TenantId_Number",
                table: "assurarr_quality_releases",
                columns: new[] { "TenantId", "Number" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_assurarr_quality_releases_TenantId_Status",
                table: "assurarr_quality_releases",
                columns: new[] { "TenantId", "Status" });

            migrationBuilder.CreateIndex(
                name: "IX_assurarr_quality_reviews_TenantId",
                table: "assurarr_quality_reviews",
                column: "TenantId");

            migrationBuilder.CreateIndex(
                name: "IX_assurarr_quality_reviews_TenantId_Number",
                table: "assurarr_quality_reviews",
                columns: new[] { "TenantId", "Number" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_assurarr_quality_reviews_TenantId_Status",
                table: "assurarr_quality_reviews",
                columns: new[] { "TenantId", "Status" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "assurarr_quality_releases");

            migrationBuilder.DropTable(
                name: "assurarr_quality_reviews");
        }
    }
}
