using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace NexArr.Api.Migrations
{
    /// <inheritdoc />
    public partial class NexArrEnsurePlatformRoleAssignmentsTable : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql(
                """
                DO $$
                BEGIN
                    IF to_regclass('public.platform_role_assignments') IS NULL THEN
                        CREATE TABLE public.platform_role_assignments
                        (
                            "Id" uuid NOT NULL,
                            "UserId" uuid NOT NULL,
                            "TenantId" uuid NULL,
                            "RoleKey" character varying(64) NOT NULL,
                            "CreatedAt" timestamp with time zone NOT NULL,
                            "CreatedByUserId" uuid NOT NULL,
                            CONSTRAINT "PK_platform_role_assignments" PRIMARY KEY ("Id"),
                            CONSTRAINT "FK_platform_role_assignments_platform_users_UserId"
                                FOREIGN KEY ("UserId")
                                REFERENCES public.platform_users ("Id")
                                ON DELETE CASCADE
                        );
                    END IF;
                END
                $$;
                """);

            migrationBuilder.Sql(
                """
                CREATE UNIQUE INDEX IF NOT EXISTS "IX_platform_role_assignments_UserId_RoleKey_TenantId"
                ON public.platform_role_assignments ("UserId", "RoleKey", "TenantId");
                """);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql("""DROP TABLE IF EXISTS public.platform_role_assignments;""");
        }
    }
}
