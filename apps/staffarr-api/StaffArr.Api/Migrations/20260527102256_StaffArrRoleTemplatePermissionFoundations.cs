using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace StaffArr.Api.Migrations
{
    /// <inheritdoc />
    public partial class StaffArrRoleTemplatePermissionFoundations : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "staffarr_permission_templates",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    TenantId = table.Column<Guid>(type: "uuid", nullable: false),
                    PermissionKey = table.Column<string>(type: "character varying(128)", maxLength: 128, nullable: false),
                    Name = table.Column<string>(type: "character varying(128)", maxLength: 128, nullable: false),
                    Description = table.Column<string>(type: "character varying(512)", maxLength: 512, nullable: true),
                    Status = table.Column<string>(type: "character varying(32)", maxLength: 32, nullable: false),
                    CreatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_staffarr_permission_templates", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "staffarr_role_templates",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    TenantId = table.Column<Guid>(type: "uuid", nullable: false),
                    RoleKey = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: false),
                    Name = table.Column<string>(type: "character varying(128)", maxLength: 128, nullable: false),
                    Description = table.Column<string>(type: "character varying(512)", maxLength: 512, nullable: true),
                    Status = table.Column<string>(type: "character varying(32)", maxLength: 32, nullable: false),
                    CreatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_staffarr_role_templates", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "staffarr_person_role_assignments",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    TenantId = table.Column<Guid>(type: "uuid", nullable: false),
                    PersonId = table.Column<Guid>(type: "uuid", nullable: false),
                    RoleTemplateId = table.Column<Guid>(type: "uuid", nullable: false),
                    ScopeType = table.Column<string>(type: "character varying(32)", maxLength: 32, nullable: false),
                    ScopeValue = table.Column<string>(type: "character varying(128)", maxLength: 128, nullable: true),
                    Status = table.Column<string>(type: "character varying(32)", maxLength: 32, nullable: false),
                    CreatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_staffarr_person_role_assignments", x => x.Id);
                    table.ForeignKey(
                        name: "FK_staffarr_person_role_assignments_staffarr_people_PersonId",
                        column: x => x.PersonId,
                        principalTable: "staffarr_people",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_staffarr_person_role_assignments_staffarr_role_templates_Ro~",
                        column: x => x.RoleTemplateId,
                        principalTable: "staffarr_role_templates",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "staffarr_role_template_permissions",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    TenantId = table.Column<Guid>(type: "uuid", nullable: false),
                    RoleTemplateId = table.Column<Guid>(type: "uuid", nullable: false),
                    PermissionTemplateId = table.Column<Guid>(type: "uuid", nullable: false),
                    ScopeType = table.Column<string>(type: "character varying(32)", maxLength: 32, nullable: false),
                    ScopeValue = table.Column<string>(type: "character varying(128)", maxLength: 128, nullable: true),
                    CreatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_staffarr_role_template_permissions", x => x.Id);
                    table.ForeignKey(
                        name: "FK_staffarr_role_template_permissions_staffarr_permission_temp~",
                        column: x => x.PermissionTemplateId,
                        principalTable: "staffarr_permission_templates",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_staffarr_role_template_permissions_staffarr_role_templates_~",
                        column: x => x.RoleTemplateId,
                        principalTable: "staffarr_role_templates",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_staffarr_permission_templates_TenantId",
                table: "staffarr_permission_templates",
                column: "TenantId");

            migrationBuilder.CreateIndex(
                name: "IX_staffarr_permission_templates_TenantId_PermissionKey",
                table: "staffarr_permission_templates",
                columns: new[] { "TenantId", "PermissionKey" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_staffarr_person_role_assignments_PersonId",
                table: "staffarr_person_role_assignments",
                column: "PersonId");

            migrationBuilder.CreateIndex(
                name: "IX_staffarr_person_role_assignments_RoleTemplateId",
                table: "staffarr_person_role_assignments",
                column: "RoleTemplateId");

            migrationBuilder.CreateIndex(
                name: "IX_staffarr_person_role_assignments_TenantId",
                table: "staffarr_person_role_assignments",
                column: "TenantId");

            migrationBuilder.CreateIndex(
                name: "IX_staffarr_person_role_assignments_TenantId_PersonId_RoleTemp~",
                table: "staffarr_person_role_assignments",
                columns: new[] { "TenantId", "PersonId", "RoleTemplateId", "ScopeType", "ScopeValue" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_staffarr_person_role_assignments_TenantId_PersonId_Status",
                table: "staffarr_person_role_assignments",
                columns: new[] { "TenantId", "PersonId", "Status" });

            migrationBuilder.CreateIndex(
                name: "IX_staffarr_role_template_permissions_PermissionTemplateId",
                table: "staffarr_role_template_permissions",
                column: "PermissionTemplateId");

            migrationBuilder.CreateIndex(
                name: "IX_staffarr_role_template_permissions_RoleTemplateId",
                table: "staffarr_role_template_permissions",
                column: "RoleTemplateId");

            migrationBuilder.CreateIndex(
                name: "IX_staffarr_role_template_permissions_TenantId",
                table: "staffarr_role_template_permissions",
                column: "TenantId");

            migrationBuilder.CreateIndex(
                name: "IX_staffarr_role_template_permissions_TenantId_RoleTemplateId_~",
                table: "staffarr_role_template_permissions",
                columns: new[] { "TenantId", "RoleTemplateId", "PermissionTemplateId", "ScopeType", "ScopeValue" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_staffarr_role_templates_TenantId",
                table: "staffarr_role_templates",
                column: "TenantId");

            migrationBuilder.CreateIndex(
                name: "IX_staffarr_role_templates_TenantId_RoleKey",
                table: "staffarr_role_templates",
                columns: new[] { "TenantId", "RoleKey" },
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "staffarr_person_role_assignments");

            migrationBuilder.DropTable(
                name: "staffarr_role_template_permissions");

            migrationBuilder.DropTable(
                name: "staffarr_permission_templates");

            migrationBuilder.DropTable(
                name: "staffarr_role_templates");
        }
    }
}
