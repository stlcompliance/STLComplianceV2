using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace StaffArr.Api.Migrations
{
    /// <inheritdoc />
    public partial class StaffRoleManagementV1 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "staffarr_permission_audit_log",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    TenantId = table.Column<Guid>(type: "uuid", nullable: false),
                    ActorPersonId = table.Column<Guid>(type: "uuid", nullable: true),
                    Action = table.Column<string>(type: "character varying(128)", maxLength: 128, nullable: false),
                    RoleId = table.Column<Guid>(type: "uuid", nullable: true),
                    BeforeJson = table.Column<string>(type: "text", nullable: true),
                    AfterJson = table.Column<string>(type: "text", nullable: true),
                    Reason = table.Column<string>(type: "character varying(1024)", maxLength: 1024, nullable: true),
                    CreatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_staffarr_permission_audit_log", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "staffarr_permission_catalog_cache",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    TenantId = table.Column<Guid>(type: "uuid", nullable: false),
                    ProductKey = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: false),
                    CatalogVersion = table.Column<string>(type: "character varying(128)", maxLength: 128, nullable: false),
                    CatalogJson = table.Column<string>(type: "text", nullable: false),
                    FetchedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    IsActive = table.Column<bool>(type: "boolean", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_staffarr_permission_catalog_cache", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "staffarr_person_roles",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    TenantId = table.Column<Guid>(type: "uuid", nullable: false),
                    PersonId = table.Column<Guid>(type: "uuid", nullable: false),
                    RoleId = table.Column<Guid>(type: "uuid", nullable: false),
                    AssignmentScopeType = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: false),
                    AssignmentScopeRefId = table.Column<string>(type: "character varying(128)", maxLength: 128, nullable: true),
                    StartsAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    EndsAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    AssignedByPersonId = table.Column<Guid>(type: "uuid", nullable: true),
                    CreatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_staffarr_person_roles", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "staffarr_role_permissions",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    TenantId = table.Column<Guid>(type: "uuid", nullable: false),
                    RoleId = table.Column<Guid>(type: "uuid", nullable: false),
                    ProductKey = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: false),
                    PermissionKey = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: false),
                    Effect = table.Column<string>(type: "character varying(16)", maxLength: 16, nullable: false),
                    CreatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_staffarr_role_permissions", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "staffarr_role_scopes",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    TenantId = table.Column<Guid>(type: "uuid", nullable: false),
                    RoleId = table.Column<Guid>(type: "uuid", nullable: false),
                    ScopeType = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: false),
                    ScopeRefId = table.Column<string>(type: "character varying(128)", maxLength: 128, nullable: true),
                    ScopeRefSnapshot = table.Column<string>(type: "character varying(2048)", maxLength: 2048, nullable: true),
                    CreatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_staffarr_role_scopes", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "staffarr_roles",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    TenantId = table.Column<Guid>(type: "uuid", nullable: false),
                    Name = table.Column<string>(type: "character varying(128)", maxLength: 128, nullable: false),
                    Description = table.Column<string>(type: "character varying(1024)", maxLength: 1024, nullable: true),
                    RoleType = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: false),
                    IsSystem = table.Column<bool>(type: "boolean", nullable: false),
                    IsArchived = table.Column<bool>(type: "boolean", nullable: false),
                    CreatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_staffarr_roles", x => x.Id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_staffarr_permission_audit_log_TenantId",
                table: "staffarr_permission_audit_log",
                column: "TenantId");

            migrationBuilder.CreateIndex(
                name: "IX_staffarr_permission_audit_log_TenantId_ActorPersonId_Create~",
                table: "staffarr_permission_audit_log",
                columns: new[] { "TenantId", "ActorPersonId", "CreatedAt" });

            migrationBuilder.CreateIndex(
                name: "IX_staffarr_permission_audit_log_TenantId_RoleId_CreatedAt",
                table: "staffarr_permission_audit_log",
                columns: new[] { "TenantId", "RoleId", "CreatedAt" });

            migrationBuilder.CreateIndex(
                name: "IX_staffarr_permission_catalog_cache_TenantId",
                table: "staffarr_permission_catalog_cache",
                column: "TenantId");

            migrationBuilder.CreateIndex(
                name: "IX_staffarr_permission_catalog_cache_TenantId_ProductKey_Catal~",
                table: "staffarr_permission_catalog_cache",
                columns: new[] { "TenantId", "ProductKey", "CatalogVersion" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_staffarr_permission_catalog_cache_TenantId_ProductKey_IsAct~",
                table: "staffarr_permission_catalog_cache",
                columns: new[] { "TenantId", "ProductKey", "IsActive" });

            migrationBuilder.CreateIndex(
                name: "IX_staffarr_person_roles_TenantId",
                table: "staffarr_person_roles",
                column: "TenantId");

            migrationBuilder.CreateIndex(
                name: "IX_staffarr_person_roles_TenantId_PersonId",
                table: "staffarr_person_roles",
                columns: new[] { "TenantId", "PersonId" });

            migrationBuilder.CreateIndex(
                name: "IX_staffarr_person_roles_TenantId_PersonId_RoleId_AssignmentSc~",
                table: "staffarr_person_roles",
                columns: new[] { "TenantId", "PersonId", "RoleId", "AssignmentScopeType", "AssignmentScopeRefId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_staffarr_person_roles_TenantId_RoleId",
                table: "staffarr_person_roles",
                columns: new[] { "TenantId", "RoleId" });

            migrationBuilder.CreateIndex(
                name: "IX_staffarr_role_permissions_TenantId",
                table: "staffarr_role_permissions",
                column: "TenantId");

            migrationBuilder.CreateIndex(
                name: "IX_staffarr_role_permissions_TenantId_RoleId",
                table: "staffarr_role_permissions",
                columns: new[] { "TenantId", "RoleId" });

            migrationBuilder.CreateIndex(
                name: "IX_staffarr_role_permissions_TenantId_RoleId_ProductKey_Permis~",
                table: "staffarr_role_permissions",
                columns: new[] { "TenantId", "RoleId", "ProductKey", "PermissionKey", "Effect" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_staffarr_role_scopes_TenantId",
                table: "staffarr_role_scopes",
                column: "TenantId");

            migrationBuilder.CreateIndex(
                name: "IX_staffarr_role_scopes_TenantId_RoleId",
                table: "staffarr_role_scopes",
                columns: new[] { "TenantId", "RoleId" });

            migrationBuilder.CreateIndex(
                name: "IX_staffarr_role_scopes_TenantId_RoleId_ScopeType_ScopeRefId",
                table: "staffarr_role_scopes",
                columns: new[] { "TenantId", "RoleId", "ScopeType", "ScopeRefId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_staffarr_roles_TenantId",
                table: "staffarr_roles",
                column: "TenantId");

            migrationBuilder.CreateIndex(
                name: "IX_staffarr_roles_TenantId_IsArchived",
                table: "staffarr_roles",
                columns: new[] { "TenantId", "IsArchived" });

            migrationBuilder.CreateIndex(
                name: "IX_staffarr_roles_TenantId_Name",
                table: "staffarr_roles",
                columns: new[] { "TenantId", "Name" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "staffarr_permission_audit_log");

            migrationBuilder.DropTable(
                name: "staffarr_permission_catalog_cache");

            migrationBuilder.DropTable(
                name: "staffarr_person_roles");

            migrationBuilder.DropTable(
                name: "staffarr_role_permissions");

            migrationBuilder.DropTable(
                name: "staffarr_role_scopes");

            migrationBuilder.DropTable(
                name: "staffarr_roles");
        }
    }
}
