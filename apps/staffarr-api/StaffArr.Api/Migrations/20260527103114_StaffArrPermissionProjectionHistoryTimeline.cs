using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace StaffArr.Api.Migrations
{
    /// <inheritdoc />
    public partial class StaffArrPermissionProjectionHistoryTimeline : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "staffarr_permission_history_events",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    TenantId = table.Column<Guid>(type: "uuid", nullable: false),
                    PersonId = table.Column<Guid>(type: "uuid", nullable: false),
                    AssignmentId = table.Column<Guid>(type: "uuid", nullable: false),
                    RoleTemplateId = table.Column<Guid>(type: "uuid", nullable: false),
                    PermissionTemplateId = table.Column<Guid>(type: "uuid", nullable: false),
                    ActorUserId = table.Column<Guid>(type: "uuid", nullable: true),
                    EventType = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: false),
                    AssignmentStatus = table.Column<string>(type: "character varying(32)", maxLength: 32, nullable: false),
                    RoleKey = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: false),
                    RoleName = table.Column<string>(type: "character varying(128)", maxLength: 128, nullable: false),
                    PermissionKey = table.Column<string>(type: "character varying(128)", maxLength: 128, nullable: false),
                    PermissionName = table.Column<string>(type: "character varying(128)", maxLength: 128, nullable: false),
                    ScopeType = table.Column<string>(type: "character varying(32)", maxLength: 32, nullable: false),
                    ScopeValue = table.Column<string>(type: "character varying(128)", maxLength: 128, nullable: true),
                    OccurredAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false)
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
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "staffarr_permission_history_events");
        }
    }
}
