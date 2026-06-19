using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace StaffArr.Api.Migrations
{
    /// <inheritdoc />
    public partial class DropLegacyRoleTemplateTables : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "staffarr_permission_history_events");

            migrationBuilder.DropTable(
                name: "staffarr_role_template_permissions");

            migrationBuilder.DropTable(
                name: "staffarr_person_role_assignments");

            migrationBuilder.DropTable(
                name: "staffarr_role_templates");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "staffarr_role_templates",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    CreatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    Description = table.Column<string>(type: "character varying(512)", maxLength: 512, nullable: true),
                    Name = table.Column<string>(type: "character varying(128)", maxLength: 128, nullable: false),
                    RoleKey = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: false),
                    Status = table.Column<string>(type: "character varying(32)", maxLength: 32, nullable: false),
                    TenantId = table.Column<Guid>(type: "uuid", nullable: false),
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
                    CreatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    ExpiresAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    PersonId = table.Column<Guid>(type: "uuid", nullable: false),
                    RoleTemplateId = table.Column<Guid>(type: "uuid", nullable: false),
                    ScopeType = table.Column<string>(type: "character varying(32)", maxLength: 32, nullable: false),
                    ScopeValue = table.Column<string>(type: "character varying(128)", maxLength: 128, nullable: true),
                    Status = table.Column<string>(type: "character varying(32)", maxLength: 32, nullable: false),
                    TenantId = table.Column<Guid>(type: "uuid", nullable: false),
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
                    CreatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    PermissionTemplateId = table.Column<Guid>(type: "uuid", nullable: false),
                    RoleTemplateId = table.Column<Guid>(type: "uuid", nullable: false),
                    ScopeType = table.Column<string>(type: "character varying(32)", maxLength: 32, nullable: false),
                    ScopeValue = table.Column<string>(type: "character varying(128)", maxLength: 128, nullable: true),
                    TenantId = table.Column<Guid>(type: "uuid", nullable: false),
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

            migrationBuilder.CreateTable(
                name: "staffarr_permission_history_events",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    ActorUserId = table.Column<Guid>(type: "uuid", nullable: true),
                    AssignmentId = table.Column<Guid>(type: "uuid", nullable: false),
                    AssignmentStatus = table.Column<string>(type: "character varying(32)", maxLength: 32, nullable: false),
                    EventType = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: false),
                    OccurredAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    PermissionKey = table.Column<string>(type: "character varying(128)", maxLength: 128, nullable: false),
                    PermissionName = table.Column<string>(type: "character varying(128)", maxLength: 128, nullable: false),
                    PermissionTemplateId = table.Column<Guid>(type: "uuid", nullable: false),
                    PersonId = table.Column<Guid>(type: "uuid", nullable: false),
                    RoleKey = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: false),
                    RoleName = table.Column<string>(type: "character varying(128)", maxLength: 128, nullable: false),
                    RoleTemplateId = table.Column<Guid>(type: "uuid", nullable: false),
                    ScopeType = table.Column<string>(type: "character varying(32)", maxLength: 32, nullable: false),
                    ScopeValue = table.Column<string>(type: "character varying(128)", maxLength: 128, nullable: true),
                    TenantId = table.Column<Guid>(type: "uuid", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_staffarr_permission_history_events", x => x.Id);
                    table.ForeignKey(
                        name: "FK_staffarr_permission_history_events_staffarr_people_PersonId",
                        column: x => x.PersonId,
                        principalTable: "staffarr_people",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_staffarr_permission_history_events_staffarr_permission_temp~",
                        column: x => x.PermissionTemplateId,
                        principalTable: "staffarr_permission_templates",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_staffarr_permission_history_events_staffarr_person_role_ass~",
                        column: x => x.AssignmentId,
                        principalTable: "staffarr_person_role_assignments",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_staffarr_permission_history_events_staffarr_role_templates_~",
                        column: x => x.RoleTemplateId,
                        principalTable: "staffarr_role_templates",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_staffarr_permission_history_events_AssignmentId",
                table: "staffarr_permission_history_events",
                column: "AssignmentId");

            migrationBuilder.CreateIndex(
                name: "IX_staffarr_permission_history_events_PermissionTemplateId",
                table: "staffarr_permission_history_events",
                column: "PermissionTemplateId");

            migrationBuilder.CreateIndex(
                name: "IX_staffarr_permission_history_events_PersonId",
                table: "staffarr_permission_history_events",
                column: "PersonId");

            migrationBuilder.CreateIndex(
                name: "IX_staffarr_permission_history_events_RoleTemplateId",
                table: "staffarr_permission_history_events",
                column: "RoleTemplateId");

            migrationBuilder.CreateIndex(
                name: "IX_staffarr_permission_history_events_TenantId",
                table: "staffarr_permission_history_events",
                column: "TenantId");

            migrationBuilder.CreateIndex(
                name: "IX_staffarr_permission_history_events_TenantId_AssignmentId_Oc~",
                table: "staffarr_permission_history_events",
                columns: new[] { "TenantId", "AssignmentId", "OccurredAt" });

            migrationBuilder.CreateIndex(
                name: "IX_staffarr_permission_history_events_TenantId_PersonId_Occurr~",
                table: "staffarr_permission_history_events",
                columns: new[] { "TenantId", "PersonId", "OccurredAt" });

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
                name: "IX_staffarr_person_role_assignments_TenantId_PersonId_Status_E~",
                table: "staffarr_person_role_assignments",
                columns: new[] { "TenantId", "PersonId", "Status", "ExpiresAt" });

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
    }
}
