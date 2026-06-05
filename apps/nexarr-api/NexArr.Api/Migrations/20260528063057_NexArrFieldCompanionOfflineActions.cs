using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace NexArr.Api.Migrations
{
    /// <inheritdoc />
    public partial class NexArrFieldCompanionOfflineActions : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // No-op: nexarr_platform_audit_package_generation_jobs is created in
            // 20260528055012_NexArrPlatformAuditPackageGenerationJobs. fieldcompanion offline
            // action tables are created in 20260528064040_NexArrFieldCompanionFieldSubmissions.
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
        }
    }
}
