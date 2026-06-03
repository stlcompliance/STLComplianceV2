using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace TrainArr.Api.Migrations
{
    /// <inheritdoc />
    public partial class TrainArrProgramContentReferences : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "trainarr_training_program_content_references",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    TenantId = table.Column<Guid>(type: "uuid", nullable: false),
                    TrainingProgramId = table.Column<Guid>(type: "uuid", nullable: false),
                    ContentType = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: false),
                    Title = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    ReferenceValue = table.Column<string>(type: "character varying(1024)", maxLength: 1024, nullable: false),
                    Notes = table.Column<string>(type: "character varying(1024)", maxLength: 1024, nullable: true),
                    CreatedByUserId = table.Column<Guid>(type: "uuid", nullable: true),
                    CreatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_trainarr_training_program_content_references", x => x.Id);
                    table.ForeignKey(
                        name: "FK_trainarr_training_program_content_references_trainarr_train~",
                        column: x => x.TrainingProgramId,
                        principalTable: "trainarr_training_programs",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_trainarr_training_program_content_references_TenantId",
                table: "trainarr_training_program_content_references",
                column: "TenantId");

            migrationBuilder.CreateIndex(
                name: "IX_trainarr_training_program_content_references_TenantId_Trai~1",
                table: "trainarr_training_program_content_references",
                columns: new[] { "TenantId", "TrainingProgramId", "ContentType", "ReferenceValue" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_trainarr_training_program_content_references_TenantId_Train~",
                table: "trainarr_training_program_content_references",
                columns: new[] { "TenantId", "TrainingProgramId", "CreatedAt" });

            migrationBuilder.CreateIndex(
                name: "IX_trainarr_training_program_content_references_TrainingProgra~",
                table: "trainarr_training_program_content_references",
                column: "TrainingProgramId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "trainarr_training_program_content_references");
        }
    }
}
