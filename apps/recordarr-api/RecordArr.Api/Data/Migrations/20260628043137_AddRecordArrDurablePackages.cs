using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace RecordArr.Api.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddRecordArrDurablePackages : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "recordarr_package_manifests",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    TenantId = table.Column<Guid>(type: "uuid", nullable: false),
                    ManifestId = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: false),
                    PackageId = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: false),
                    ManifestVersion = table.Column<int>(type: "integer", nullable: false),
                    GeneratedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    Checksum = table.Column<string>(type: "character varying(128)", maxLength: 128, nullable: false),
                    GeneratedByPersonId = table.Column<string>(type: "character varying(128)", maxLength: 128, nullable: false),
                    PayloadJson = table.Column<string>(type: "jsonb", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_recordarr_package_manifests", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "recordarr_packages",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    TenantId = table.Column<Guid>(type: "uuid", nullable: false),
                    PackageId = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: false),
                    PackageNumber = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: false),
                    Title = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: false),
                    PackageType = table.Column<string>(type: "character varying(96)", maxLength: 96, nullable: false),
                    Status = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: false),
                    SourceProduct = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: false),
                    ManifestChecksum = table.Column<string>(type: "character varying(128)", maxLength: 128, nullable: true),
                    GeneratedPdfRecordRef = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: true),
                    GeneratedZipFileRef = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: true),
                    CreatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    CompletedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    LockedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    ArchivedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    ExpiresAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    PayloadJson = table.Column<string>(type: "jsonb", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_recordarr_packages", x => x.Id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_recordarr_package_manifests_TenantId",
                table: "recordarr_package_manifests",
                column: "TenantId");

            migrationBuilder.CreateIndex(
                name: "IX_recordarr_package_manifests_TenantId_ManifestId",
                table: "recordarr_package_manifests",
                columns: new[] { "TenantId", "ManifestId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_recordarr_package_manifests_TenantId_PackageId_ManifestVers~",
                table: "recordarr_package_manifests",
                columns: new[] { "TenantId", "PackageId", "ManifestVersion" });

            migrationBuilder.CreateIndex(
                name: "IX_recordarr_packages_TenantId",
                table: "recordarr_packages",
                column: "TenantId");

            migrationBuilder.CreateIndex(
                name: "IX_recordarr_packages_TenantId_PackageId",
                table: "recordarr_packages",
                columns: new[] { "TenantId", "PackageId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_recordarr_packages_TenantId_SourceProduct_CreatedAt",
                table: "recordarr_packages",
                columns: new[] { "TenantId", "SourceProduct", "CreatedAt" });

            migrationBuilder.CreateIndex(
                name: "IX_recordarr_packages_TenantId_Status_CreatedAt",
                table: "recordarr_packages",
                columns: new[] { "TenantId", "Status", "CreatedAt" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "recordarr_package_manifests");

            migrationBuilder.DropTable(
                name: "recordarr_packages");
        }
    }
}
