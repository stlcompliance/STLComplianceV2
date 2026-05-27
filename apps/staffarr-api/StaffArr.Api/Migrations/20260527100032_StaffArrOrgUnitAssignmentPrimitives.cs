using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace StaffArr.Api.Migrations
{
    /// <inheritdoc />
    public partial class StaffArrOrgUnitAssignmentPrimitives : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "staffarr_org_unit_assignments",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    TenantId = table.Column<Guid>(type: "uuid", nullable: false),
                    PersonId = table.Column<Guid>(type: "uuid", nullable: false),
                    SiteOrgUnitId = table.Column<Guid>(type: "uuid", nullable: false),
                    DepartmentOrgUnitId = table.Column<Guid>(type: "uuid", nullable: false),
                    TeamOrgUnitId = table.Column<Guid>(type: "uuid", nullable: false),
                    PositionOrgUnitId = table.Column<Guid>(type: "uuid", nullable: false),
                    Status = table.Column<string>(type: "character varying(32)", maxLength: 32, nullable: false),
                    CreatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_staffarr_org_unit_assignments", x => x.Id);
                    table.ForeignKey(
                        name: "FK_staffarr_org_unit_assignments_staffarr_org_units_Department~",
                        column: x => x.DepartmentOrgUnitId,
                        principalTable: "staffarr_org_units",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_staffarr_org_unit_assignments_staffarr_org_units_PositionOr~",
                        column: x => x.PositionOrgUnitId,
                        principalTable: "staffarr_org_units",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_staffarr_org_unit_assignments_staffarr_org_units_SiteOrgUni~",
                        column: x => x.SiteOrgUnitId,
                        principalTable: "staffarr_org_units",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_staffarr_org_unit_assignments_staffarr_org_units_TeamOrgUni~",
                        column: x => x.TeamOrgUnitId,
                        principalTable: "staffarr_org_units",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_staffarr_org_unit_assignments_staffarr_people_PersonId",
                        column: x => x.PersonId,
                        principalTable: "staffarr_people",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_staffarr_org_unit_assignments_DepartmentOrgUnitId",
                table: "staffarr_org_unit_assignments",
                column: "DepartmentOrgUnitId");

            migrationBuilder.CreateIndex(
                name: "IX_staffarr_org_unit_assignments_PersonId",
                table: "staffarr_org_unit_assignments",
                column: "PersonId");

            migrationBuilder.CreateIndex(
                name: "IX_staffarr_org_unit_assignments_PositionOrgUnitId",
                table: "staffarr_org_unit_assignments",
                column: "PositionOrgUnitId");

            migrationBuilder.CreateIndex(
                name: "IX_staffarr_org_unit_assignments_SiteOrgUnitId",
                table: "staffarr_org_unit_assignments",
                column: "SiteOrgUnitId");

            migrationBuilder.CreateIndex(
                name: "IX_staffarr_org_unit_assignments_TeamOrgUnitId",
                table: "staffarr_org_unit_assignments",
                column: "TeamOrgUnitId");

            migrationBuilder.CreateIndex(
                name: "IX_staffarr_org_unit_assignments_TenantId",
                table: "staffarr_org_unit_assignments",
                column: "TenantId");

            migrationBuilder.CreateIndex(
                name: "IX_staffarr_org_unit_assignments_TenantId_PersonId_SiteOrgUnit~",
                table: "staffarr_org_unit_assignments",
                columns: new[] { "TenantId", "PersonId", "SiteOrgUnitId", "DepartmentOrgUnitId", "TeamOrgUnitId", "PositionOrgUnitId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_staffarr_org_unit_assignments_TenantId_PersonId_Status",
                table: "staffarr_org_unit_assignments",
                columns: new[] { "TenantId", "PersonId", "Status" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "staffarr_org_unit_assignments");
        }
    }
}
