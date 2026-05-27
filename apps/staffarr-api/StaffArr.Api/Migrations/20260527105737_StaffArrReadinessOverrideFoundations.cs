using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace StaffArr.Api.Migrations
{
    /// <inheritdoc />
    public partial class StaffArrReadinessOverrideFoundations : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "staffarr_person_readiness_overrides",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    TenantId = table.Column<Guid>(type: "uuid", nullable: false),
                    PersonId = table.Column<Guid>(type: "uuid", nullable: false),
                    Status = table.Column<string>(type: "character varying(32)", maxLength: 32, nullable: false),
                    Reason = table.Column<string>(type: "character varying(1024)", maxLength: 1024, nullable: false),
                    GrantedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    ExpiresAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    GrantedByUserId = table.Column<Guid>(type: "uuid", nullable: false),
                    ClearedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    ClearedByUserId = table.Column<Guid>(type: "uuid", nullable: true),
                    CreatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_staffarr_person_readiness_overrides", x => x.Id);
                    table.ForeignKey(
                        name: "FK_staffarr_person_readiness_overrides_staffarr_people_PersonId",
                        column: x => x.PersonId,
                        principalTable: "staffarr_people",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_staffarr_person_readiness_overrides_PersonId",
                table: "staffarr_person_readiness_overrides",
                column: "PersonId");

            migrationBuilder.CreateIndex(
                name: "IX_staffarr_person_readiness_overrides_TenantId",
                table: "staffarr_person_readiness_overrides",
                column: "TenantId");

            migrationBuilder.CreateIndex(
                name: "IX_staffarr_person_readiness_overrides_TenantId_PersonId_Grant~",
                table: "staffarr_person_readiness_overrides",
                columns: new[] { "TenantId", "PersonId", "GrantedAt" });

            migrationBuilder.CreateIndex(
                name: "IX_staffarr_person_readiness_overrides_TenantId_PersonId_Status",
                table: "staffarr_person_readiness_overrides",
                columns: new[] { "TenantId", "PersonId", "Status" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "staffarr_person_readiness_overrides");
        }
    }
}
