using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace StaffArr.Api.Migrations
{
    /// <inheritdoc />
    public partial class StaffArrPeopleDirectorySlice : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "staffarr_org_units",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    TenantId = table.Column<Guid>(type: "uuid", nullable: false),
                    UnitType = table.Column<string>(type: "character varying(32)", maxLength: 32, nullable: false),
                    Name = table.Column<string>(type: "character varying(128)", maxLength: 128, nullable: false),
                    ParentOrgUnitId = table.Column<Guid>(type: "uuid", nullable: true),
                    CreatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_staffarr_org_units", x => x.Id);
                    table.ForeignKey(
                        name: "FK_staffarr_org_units_staffarr_org_units_ParentOrgUnitId",
                        column: x => x.ParentOrgUnitId,
                        principalTable: "staffarr_org_units",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "staffarr_people",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    TenantId = table.Column<Guid>(type: "uuid", nullable: false),
                    ExternalUserId = table.Column<Guid>(type: "uuid", nullable: true),
                    GivenName = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    FamilyName = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    DisplayName = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    PrimaryEmail = table.Column<string>(type: "character varying(320)", maxLength: 320, nullable: false),
                    EmploymentStatus = table.Column<string>(type: "character varying(32)", maxLength: 32, nullable: false),
                    PrimaryOrgUnitId = table.Column<Guid>(type: "uuid", nullable: true),
                    ManagerPersonId = table.Column<Guid>(type: "uuid", nullable: true),
                    JobTitle = table.Column<string>(type: "character varying(128)", maxLength: 128, nullable: true),
                    CreatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_staffarr_people", x => x.Id);
                    table.ForeignKey(
                        name: "FK_staffarr_people_staffarr_org_units_PrimaryOrgUnitId",
                        column: x => x.PrimaryOrgUnitId,
                        principalTable: "staffarr_org_units",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_staffarr_people_staffarr_people_ManagerPersonId",
                        column: x => x.ManagerPersonId,
                        principalTable: "staffarr_people",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateIndex(
                name: "IX_staffarr_org_units_ParentOrgUnitId",
                table: "staffarr_org_units",
                column: "ParentOrgUnitId");

            migrationBuilder.CreateIndex(
                name: "IX_staffarr_org_units_TenantId",
                table: "staffarr_org_units",
                column: "TenantId");

            migrationBuilder.CreateIndex(
                name: "IX_staffarr_org_units_TenantId_UnitType_Name",
                table: "staffarr_org_units",
                columns: new[] { "TenantId", "UnitType", "Name" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_staffarr_people_ManagerPersonId",
                table: "staffarr_people",
                column: "ManagerPersonId");

            migrationBuilder.CreateIndex(
                name: "IX_staffarr_people_PrimaryOrgUnitId",
                table: "staffarr_people",
                column: "PrimaryOrgUnitId");

            migrationBuilder.CreateIndex(
                name: "IX_staffarr_people_TenantId",
                table: "staffarr_people",
                column: "TenantId");

            migrationBuilder.CreateIndex(
                name: "IX_staffarr_people_TenantId_ExternalUserId",
                table: "staffarr_people",
                columns: new[] { "TenantId", "ExternalUserId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_staffarr_people_TenantId_PrimaryEmail",
                table: "staffarr_people",
                columns: new[] { "TenantId", "PrimaryEmail" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "staffarr_people");

            migrationBuilder.DropTable(
                name: "staffarr_org_units");
        }
    }
}
