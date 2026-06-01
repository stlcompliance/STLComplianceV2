using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;
using NexArr.Api.Data;

#nullable disable

namespace NexArr.Api.Migrations
{
    [DbContext(typeof(NexArrDbContext))]
    [Migration("20260601070000_NexArrRepairUserCredentialColumns")]
    public partial class NexArrRepairUserCredentialColumns : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql(
                """
                ALTER TABLE user_credentials
                    ADD COLUMN IF NOT EXISTS "IsEmailVerified" boolean NOT NULL DEFAULT TRUE,
                    ADD COLUMN IF NOT EXISTS "IsMfaEnabled" boolean NOT NULL DEFAULT FALSE;

                UPDATE user_credentials
                SET "IsEmailVerified" = TRUE
                WHERE "IsEmailVerified" IS NULL;

                UPDATE user_credentials
                SET "IsMfaEnabled" = FALSE
                WHERE "IsMfaEnabled" IS NULL;

                ALTER TABLE user_credentials
                    ALTER COLUMN "IsEmailVerified" SET DEFAULT TRUE,
                    ALTER COLUMN "IsEmailVerified" SET NOT NULL,
                    ALTER COLUMN "IsMfaEnabled" SET DEFAULT FALSE,
                    ALTER COLUMN "IsMfaEnabled" SET NOT NULL;
                """);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql(
                """
                ALTER TABLE user_credentials
                    DROP COLUMN IF EXISTS "IsEmailVerified",
                    DROP COLUMN IF EXISTS "IsMfaEnabled";
                """);
        }
    }
}
