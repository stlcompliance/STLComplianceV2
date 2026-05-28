using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace StaffArr.Api.Migrations
{
    /// <inheritdoc />
    public partial class StaffArrTrainingAcknowledgements : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "staffarr_person_training_acknowledgements",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    TenantId = table.Column<Guid>(type: "uuid", nullable: false),
                    PersonId = table.Column<Guid>(type: "uuid", nullable: false),
                    TrainarrAcknowledgementRequestId = table.Column<Guid>(type: "uuid", nullable: false),
                    TrainarrAssignmentId = table.Column<Guid>(type: "uuid", nullable: false),
                    TrainingTitle = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: false),
                    AssignmentReason = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: false),
                    Summary = table.Column<string>(type: "character varying(2048)", maxLength: 2048, nullable: false),
                    Status = table.Column<string>(type: "character varying(32)", maxLength: 32, nullable: false),
                    DueAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    RequestedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    AcknowledgedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    AcknowledgedByUserId = table.Column<Guid>(type: "uuid", nullable: true),
                    CreatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_staffarr_person_training_acknowledgements", x => x.Id);
                    table.ForeignKey(
                        name: "FK_staffarr_person_training_acknowledgements_staffarr_people_P~",
                        column: x => x.PersonId,
                        principalTable: "staffarr_people",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_staffarr_person_training_acknowledgements_PersonId",
                table: "staffarr_person_training_acknowledgements",
                column: "PersonId");

            migrationBuilder.CreateIndex(
                name: "IX_staffarr_person_training_acknowledgements_TenantId",
                table: "staffarr_person_training_acknowledgements",
                column: "TenantId");

            migrationBuilder.CreateIndex(
                name: "IX_staffarr_person_training_acknowledgements_TenantId_PersonId~",
                table: "staffarr_person_training_acknowledgements",
                columns: new[] { "TenantId", "PersonId", "Status" });

            migrationBuilder.CreateIndex(
                name: "IX_staffarr_person_training_acknowledgements_TenantId_Trainarr~",
                table: "staffarr_person_training_acknowledgements",
                columns: new[] { "TenantId", "TrainarrAcknowledgementRequestId" },
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "staffarr_person_training_acknowledgements");
        }
    }
}
