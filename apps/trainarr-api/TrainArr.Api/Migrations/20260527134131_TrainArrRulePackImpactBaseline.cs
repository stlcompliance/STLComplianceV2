using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace TrainArr.Api.Migrations
{
    /// <inheritdoc />
    public partial class TrainArrRulePackImpactBaseline : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "KnownStatus",
                table: "trainarr_training_rule_pack_requirements",
                type: "character varying(32)",
                maxLength: 32,
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "KnownVersionNumber",
                table: "trainarr_training_rule_pack_requirements",
                type: "integer",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_trainarr_training_rule_pack_requirements_TenantId_RulePackK~",
                table: "trainarr_training_rule_pack_requirements",
                columns: new[] { "TenantId", "RulePackKey" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_trainarr_training_rule_pack_requirements_TenantId_RulePackK~",
                table: "trainarr_training_rule_pack_requirements");

            migrationBuilder.DropColumn(
                name: "KnownStatus",
                table: "trainarr_training_rule_pack_requirements");

            migrationBuilder.DropColumn(
                name: "KnownVersionNumber",
                table: "trainarr_training_rule_pack_requirements");
        }
    }
}
