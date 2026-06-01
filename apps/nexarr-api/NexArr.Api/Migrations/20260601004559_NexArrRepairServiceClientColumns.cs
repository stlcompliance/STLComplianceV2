using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace NexArr.Api.Migrations
{
    /// <inheritdoc />
    public partial class NexArrRepairServiceClientColumns : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql(
                """
                ALTER TABLE service_clients
                    ADD COLUMN IF NOT EXISTS "AllowedTenantIds" character varying(2048) NOT NULL DEFAULT '',
                    ADD COLUMN IF NOT EXISTS "LastUsedAt" timestamp with time zone NULL,
                    ADD COLUMN IF NOT EXISTS "FailedAuthenticationAttempts" integer NOT NULL DEFAULT 0;
                """);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql(
                """
                ALTER TABLE service_clients
                    DROP COLUMN IF EXISTS "AllowedTenantIds",
                    DROP COLUMN IF EXISTS "LastUsedAt",
                    DROP COLUMN IF EXISTS "FailedAuthenticationAttempts";
                """);
        }
    }
}
