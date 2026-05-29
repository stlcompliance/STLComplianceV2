using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace MaintainArr.Api.Migrations
{
    /// <inheritdoc />
    public partial class MaintainArrStaffPersonRefs : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "maintainarr_staff_person_refs",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    TenantId = table.Column<Guid>(type: "uuid", nullable: false),
                    StaffarrPersonId = table.Column<string>(type: "character varying(128)", maxLength: 128, nullable: false),
                    DisplayNameSnapshot = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: false),
                    ActiveStatusSnapshot = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: true),
                    PrimarySiteSnapshot = table.Column<string>(type: "character varying(128)", maxLength: 128, nullable: true),
                    LastSeenAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    SourceCorrelationId = table.Column<string>(type: "character varying(128)", maxLength: 128, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_maintainarr_staff_person_refs", x => x.Id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_maintainarr_staff_person_refs_TenantId",
                table: "maintainarr_staff_person_refs",
                column: "TenantId");

            migrationBuilder.CreateIndex(
                name: "IX_maintainarr_staff_person_refs_TenantId_StaffarrPersonId",
                table: "maintainarr_staff_person_refs",
                columns: new[] { "TenantId", "StaffarrPersonId" },
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "maintainarr_staff_person_refs");
        }
    }
}
