using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace StaffArr.Api.Migrations
{
    /// <inheritdoc />
    public partial class StaffArrPersonPermissionProjections : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "staffarr_person_permission_projections",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    TenantId = table.Column<Guid>(type: "uuid", nullable: false),
                    PersonId = table.Column<Guid>(type: "uuid", nullable: false),
                    PermissionCount = table.Column<int>(type: "integer", nullable: false),
                    ComputedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    CreatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_staffarr_person_permission_projections", x => x.Id);
                    table.ForeignKey(
                        name: "FK_staffarr_person_permission_projections_staffarr_people_Pers~",
                        column: x => x.PersonId,
                        principalTable: "staffarr_people",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "staffarr_person_permission_projection_entries",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    TenantId = table.Column<Guid>(type: "uuid", nullable: false),
                    PersonId = table.Column<Guid>(type: "uuid", nullable: false),
                    ProjectionId = table.Column<Guid>(type: "uuid", nullable: false),
                    PermissionKey = table.Column<string>(type: "character varying(128)", maxLength: 128, nullable: false),
                    PermissionName = table.Column<string>(type: "character varying(128)", maxLength: 128, nullable: false),
                    ScopeType = table.Column<string>(type: "character varying(32)", maxLength: 32, nullable: false),
                    ScopeValue = table.Column<string>(type: "character varying(128)", maxLength: 128, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_staffarr_person_permission_projection_entries", x => x.Id);
                    table.ForeignKey(
                        name: "FK_staffarr_person_permission_projection_entries_staffarr_peop~",
                        column: x => x.PersonId,
                        principalTable: "staffarr_people",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_staffarr_person_permission_projection_entries_staffarr_pers~",
                        column: x => x.ProjectionId,
                        principalTable: "staffarr_person_permission_projections",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_staffarr_person_permission_projection_entries_PersonId",
                table: "staffarr_person_permission_projection_entries",
                column: "PersonId");

            migrationBuilder.CreateIndex(
                name: "IX_staffarr_person_permission_projection_entries_ProjectionId",
                table: "staffarr_person_permission_projection_entries",
                column: "ProjectionId");

            migrationBuilder.CreateIndex(
                name: "IX_staffarr_person_permission_projection_entries_TenantId",
                table: "staffarr_person_permission_projection_entries",
                column: "TenantId");

            migrationBuilder.CreateIndex(
                name: "IX_staffarr_person_permission_projection_entries_TenantId_Pers~",
                table: "staffarr_person_permission_projection_entries",
                columns: new[] { "TenantId", "PersonId", "PermissionKey", "ScopeType", "ScopeValue" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_staffarr_person_permission_projections_PersonId",
                table: "staffarr_person_permission_projections",
                column: "PersonId");

            migrationBuilder.CreateIndex(
                name: "IX_staffarr_person_permission_projections_TenantId",
                table: "staffarr_person_permission_projections",
                column: "TenantId");

            migrationBuilder.CreateIndex(
                name: "IX_staffarr_person_permission_projections_TenantId_ComputedAt",
                table: "staffarr_person_permission_projections",
                columns: new[] { "TenantId", "ComputedAt" });

            migrationBuilder.CreateIndex(
                name: "IX_staffarr_person_permission_projections_TenantId_PersonId",
                table: "staffarr_person_permission_projections",
                columns: new[] { "TenantId", "PersonId" },
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "staffarr_person_permission_projection_entries");

            migrationBuilder.DropTable(
                name: "staffarr_person_permission_projections");
        }
    }
}
