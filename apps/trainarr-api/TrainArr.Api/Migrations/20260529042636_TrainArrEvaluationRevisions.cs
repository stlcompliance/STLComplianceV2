using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace TrainArr.Api.Migrations
{
    /// <inheritdoc />
    public partial class TrainArrEvaluationRevisions : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "trainarr_training_evaluation_revisions",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    TenantId = table.Column<Guid>(type: "uuid", nullable: false),
                    TrainingAssignmentId = table.Column<Guid>(type: "uuid", nullable: false),
                    TrainingEvaluationId = table.Column<Guid>(type: "uuid", nullable: false),
                    Result = table.Column<string>(type: "character varying(32)", maxLength: 32, nullable: false),
                    Score = table.Column<decimal>(type: "numeric", nullable: true),
                    Notes = table.Column<string>(type: "character varying(2048)", maxLength: 2048, nullable: true),
                    EvaluatorUserId = table.Column<Guid>(type: "uuid", nullable: false),
                    EvaluatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    SupersededAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    SupersededByUserId = table.Column<Guid>(type: "uuid", nullable: false),
                    CreatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_trainarr_training_evaluation_revisions", x => x.Id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_trainarr_training_evaluation_revisions_TenantId",
                table: "trainarr_training_evaluation_revisions",
                column: "TenantId");

            migrationBuilder.CreateIndex(
                name: "IX_trainarr_training_evaluation_revisions_TenantId_TrainingAss~",
                table: "trainarr_training_evaluation_revisions",
                columns: new[] { "TenantId", "TrainingAssignmentId", "SupersededAt" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "trainarr_training_evaluation_revisions");
        }
    }
}
