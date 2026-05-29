using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace TrainArr.Api.Migrations
{
    /// <inheritdoc />
    public partial class TrainArrDefinitionStepBranches : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "trainarr_training_definition_step_branches",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    TenantId = table.Column<Guid>(type: "uuid", nullable: false),
                    TrainingDefinitionStepId = table.Column<Guid>(type: "uuid", nullable: false),
                    BranchKey = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: false),
                    BranchType = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: false),
                    Label = table.Column<string>(type: "character varying(128)", maxLength: 128, nullable: false),
                    ConfigJson = table.Column<string>(type: "text", nullable: false),
                    SortOrder = table.Column<int>(type: "integer", nullable: false),
                    CreatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_trainarr_training_definition_step_branches", x => x.Id);
                    table.ForeignKey(
                        name: "FK_trainarr_training_definition_step_branches_trainarr_trainin~",
                        column: x => x.TrainingDefinitionStepId,
                        principalTable: "trainarr_training_definition_steps",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_trainarr_training_definition_step_branches_TenantId",
                table: "trainarr_training_definition_step_branches",
                column: "TenantId");

            migrationBuilder.CreateIndex(
                name: "IX_trainarr_training_definition_step_branches_TenantId_Trainin~",
                table: "trainarr_training_definition_step_branches",
                columns: new[] { "TenantId", "TrainingDefinitionStepId", "BranchKey" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_trainarr_training_definition_step_branches_TrainingDefiniti~",
                table: "trainarr_training_definition_step_branches",
                column: "TrainingDefinitionStepId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "trainarr_training_definition_step_branches");
        }
    }
}
