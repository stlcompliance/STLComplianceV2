using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace StaffArr.Api.Migrations
{
    /// <inheritdoc />
    public partial class StaffArrTrainingBlockerDisplay : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "staffarr_person_training_blockers",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    TenantId = table.Column<Guid>(type: "uuid", nullable: false),
                    PersonId = table.Column<Guid>(type: "uuid", nullable: false),
                    TrainarrPublicationId = table.Column<Guid>(type: "uuid", nullable: false),
                    QualificationKey = table.Column<string>(type: "character varying(128)", maxLength: 128, nullable: false),
                    QualificationName = table.Column<string>(type: "character varying(128)", maxLength: 128, nullable: false),
                    BlockerType = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: false),
                    Message = table.Column<string>(type: "character varying(1024)", maxLength: 1024, nullable: false),
                    Status = table.Column<string>(type: "character varying(32)", maxLength: 32, nullable: false),
                    PublishedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    ExpiresAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    ClearedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    CreatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_staffarr_person_training_blockers", x => x.Id);
                    table.ForeignKey(
                        name: "FK_staffarr_person_training_blockers_staffarr_people_PersonId",
                        column: x => x.PersonId,
                        principalTable: "staffarr_people",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_staffarr_person_training_blockers_PersonId",
                table: "staffarr_person_training_blockers",
                column: "PersonId");

            migrationBuilder.CreateIndex(
                name: "IX_staffarr_person_training_blockers_TenantId",
                table: "staffarr_person_training_blockers",
                column: "TenantId");

            migrationBuilder.CreateIndex(
                name: "IX_staffarr_person_training_blockers_TenantId_PersonId_Status",
                table: "staffarr_person_training_blockers",
                columns: new[] { "TenantId", "PersonId", "Status" });

            migrationBuilder.CreateIndex(
                name: "IX_staffarr_person_training_blockers_TenantId_TrainarrPublicat~",
                table: "staffarr_person_training_blockers",
                columns: new[] { "TenantId", "TrainarrPublicationId" },
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "staffarr_person_training_blockers");
        }
    }
}
