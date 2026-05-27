using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace TrainArr.Api.Migrations
{
    /// <inheritdoc />
    public partial class TrainArrSignoffsEvaluations : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "trainarr_training_evaluations",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    TenantId = table.Column<Guid>(type: "uuid", nullable: false),
                    TrainingAssignmentId = table.Column<Guid>(type: "uuid", nullable: false),
                    Result = table.Column<string>(type: "character varying(32)", maxLength: 32, nullable: false),
                    Score = table.Column<decimal>(type: "numeric", nullable: true),
                    Notes = table.Column<string>(type: "character varying(2048)", maxLength: 2048, nullable: true),
                    EvaluatorUserId = table.Column<Guid>(type: "uuid", nullable: false),
                    EvaluatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    CreatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_trainarr_training_evaluations", x => x.Id);
                    table.ForeignKey(
                        name: "FK_trainarr_training_evaluations_trainarr_training_assignments~",
                        column: x => x.TrainingAssignmentId,
                        principalTable: "trainarr_training_assignments",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "trainarr_training_signoffs",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    TenantId = table.Column<Guid>(type: "uuid", nullable: false),
                    TrainingAssignmentId = table.Column<Guid>(type: "uuid", nullable: false),
                    SignoffRole = table.Column<string>(type: "character varying(32)", maxLength: 32, nullable: false),
                    SignedByUserId = table.Column<Guid>(type: "uuid", nullable: false),
                    Notes = table.Column<string>(type: "character varying(1024)", maxLength: 1024, nullable: true),
                    SignedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    CreatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_trainarr_training_signoffs", x => x.Id);
                    table.ForeignKey(
                        name: "FK_trainarr_training_signoffs_trainarr_training_assignments_Tr~",
                        column: x => x.TrainingAssignmentId,
                        principalTable: "trainarr_training_assignments",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_trainarr_training_evaluations_TenantId",
                table: "trainarr_training_evaluations",
                column: "TenantId");

            migrationBuilder.CreateIndex(
                name: "IX_trainarr_training_evaluations_TenantId_TrainingAssignmentId",
                table: "trainarr_training_evaluations",
                columns: new[] { "TenantId", "TrainingAssignmentId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_trainarr_training_evaluations_TrainingAssignmentId",
                table: "trainarr_training_evaluations",
                column: "TrainingAssignmentId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_trainarr_training_signoffs_TenantId",
                table: "trainarr_training_signoffs",
                column: "TenantId");

            migrationBuilder.CreateIndex(
                name: "IX_trainarr_training_signoffs_TenantId_TrainingAssignmentId_Si~",
                table: "trainarr_training_signoffs",
                columns: new[] { "TenantId", "TrainingAssignmentId", "SignoffRole" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_trainarr_training_signoffs_TrainingAssignmentId",
                table: "trainarr_training_signoffs",
                column: "TrainingAssignmentId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "trainarr_training_evaluations");

            migrationBuilder.DropTable(
                name: "trainarr_training_signoffs");
        }
    }
}
