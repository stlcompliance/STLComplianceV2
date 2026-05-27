using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace TrainArr.Api.Migrations
{
    /// <inheritdoc />
    public partial class TrainArrRulePackRequirements : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "trainarr_training_rule_pack_requirements",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    TenantId = table.Column<Guid>(type: "uuid", nullable: false),
                    EntityType = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: false),
                    EntityId = table.Column<Guid>(type: "uuid", nullable: false),
                    RulePackKey = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: false),
                    AttachedByUserId = table.Column<Guid>(type: "uuid", nullable: true),
                    CreatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_trainarr_training_rule_pack_requirements", x => x.Id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_trainarr_training_rule_pack_requirements_TenantId",
                table: "trainarr_training_rule_pack_requirements",
                column: "TenantId");

            migrationBuilder.CreateIndex(
                name: "IX_trainarr_training_rule_pack_requirements_TenantId_EntityTyp~",
                table: "trainarr_training_rule_pack_requirements",
                columns: new[] { "TenantId", "EntityType", "EntityId", "RulePackKey" },
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "trainarr_training_rule_pack_requirements");
        }
    }
}
