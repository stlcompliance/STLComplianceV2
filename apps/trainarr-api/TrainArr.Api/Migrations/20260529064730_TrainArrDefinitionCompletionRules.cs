using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace TrainArr.Api.Migrations
{
    /// <inheritdoc />
    public partial class TrainArrDefinitionCompletionRules : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "trainarr_training_definition_completion_rules",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    TenantId = table.Column<Guid>(type: "uuid", nullable: false),
                    TrainingDefinitionId = table.Column<Guid>(type: "uuid", nullable: false),
                    RuleKey = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: false),
                    RuleType = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: false),
                    Label = table.Column<string>(type: "character varying(128)", maxLength: 128, nullable: false),
                    ConfigJson = table.Column<string>(type: "text", nullable: false),
                    SortOrder = table.Column<int>(type: "integer", nullable: false),
                    CreatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_trainarr_training_definition_completion_rules", x => x.Id);
                    table.ForeignKey(
                        name: "FK_trainarr_training_definition_completion_rules_trainarr_trai~",
                        column: x => x.TrainingDefinitionId,
                        principalTable: "trainarr_training_definitions",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_trainarr_training_definition_completion_rules_TenantId",
                table: "trainarr_training_definition_completion_rules",
                column: "TenantId");

            migrationBuilder.CreateIndex(
                name: "IX_trainarr_training_definition_completion_rules_TenantId_Trai~",
                table: "trainarr_training_definition_completion_rules",
                columns: new[] { "TenantId", "TrainingDefinitionId", "RuleKey" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_trainarr_training_definition_completion_rules_TrainingDefin~",
                table: "trainarr_training_definition_completion_rules",
                column: "TrainingDefinitionId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "trainarr_training_definition_completion_rules");
        }
    }
}
