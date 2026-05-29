using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace TrainArr.Api.Migrations
{
    /// <inheritdoc />
    public partial class TrainArrApplicabilityRequirements : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "trainarr_training_applicability_profiles",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    TenantId = table.Column<Guid>(type: "uuid", nullable: false),
                    ProfileKey = table.Column<string>(type: "character varying(96)", maxLength: 96, nullable: false),
                    Label = table.Column<string>(type: "character varying(128)", maxLength: 128, nullable: false),
                    Description = table.Column<string>(type: "character varying(512)", maxLength: 512, nullable: true),
                    ScopeType = table.Column<string>(type: "character varying(32)", maxLength: 32, nullable: false),
                    ScopeKey = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: false),
                    SourceProduct = table.Column<string>(type: "character varying(32)", maxLength: 32, nullable: true),
                    SourceRecordId = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: true),
                    SourceUpdatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    CreatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_trainarr_training_applicability_profiles", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "trainarr_training_requirements",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    TenantId = table.Column<Guid>(type: "uuid", nullable: false),
                    RequirementKey = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: false),
                    Label = table.Column<string>(type: "character varying(128)", maxLength: 128, nullable: false),
                    Description = table.Column<string>(type: "character varying(512)", maxLength: 512, nullable: true),
                    RequirementSource = table.Column<string>(type: "character varying(32)", maxLength: 32, nullable: false),
                    SourceKey = table.Column<string>(type: "character varying(128)", maxLength: 128, nullable: true),
                    TrainingProgramId = table.Column<Guid>(type: "uuid", nullable: true),
                    TrainingDefinitionId = table.Column<Guid>(type: "uuid", nullable: true),
                    ApplicabilityProfileId = table.Column<Guid>(type: "uuid", nullable: true),
                    RequirementLevel = table.Column<string>(type: "character varying(32)", maxLength: 32, nullable: false),
                    SortOrder = table.Column<int>(type: "integer", nullable: false),
                    Status = table.Column<string>(type: "character varying(32)", maxLength: 32, nullable: false),
                    CreatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_trainarr_training_requirements", x => x.Id);
                    table.ForeignKey(
                        name: "FK_trainarr_training_requirements_trainarr_training_applicabil~",
                        column: x => x.ApplicabilityProfileId,
                        principalTable: "trainarr_training_applicability_profiles",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                    table.ForeignKey(
                        name: "FK_trainarr_training_requirements_trainarr_training_definition~",
                        column: x => x.TrainingDefinitionId,
                        principalTable: "trainarr_training_definitions",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_trainarr_training_requirements_trainarr_training_programs_T~",
                        column: x => x.TrainingProgramId,
                        principalTable: "trainarr_training_programs",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_trainarr_training_applicability_profiles_TenantId",
                table: "trainarr_training_applicability_profiles",
                column: "TenantId");

            migrationBuilder.CreateIndex(
                name: "IX_trainarr_training_applicability_profiles_TenantId_ProfileKey",
                table: "trainarr_training_applicability_profiles",
                columns: new[] { "TenantId", "ProfileKey" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_trainarr_training_requirements_ApplicabilityProfileId",
                table: "trainarr_training_requirements",
                column: "ApplicabilityProfileId");

            migrationBuilder.CreateIndex(
                name: "IX_trainarr_training_requirements_TenantId",
                table: "trainarr_training_requirements",
                column: "TenantId");

            migrationBuilder.CreateIndex(
                name: "IX_trainarr_training_requirements_TenantId_RequirementKey",
                table: "trainarr_training_requirements",
                columns: new[] { "TenantId", "RequirementKey" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_trainarr_training_requirements_TrainingDefinitionId",
                table: "trainarr_training_requirements",
                column: "TrainingDefinitionId");

            migrationBuilder.CreateIndex(
                name: "IX_trainarr_training_requirements_TrainingProgramId",
                table: "trainarr_training_requirements",
                column: "TrainingProgramId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "trainarr_training_requirements");

            migrationBuilder.DropTable(
                name: "trainarr_training_applicability_profiles");
        }
    }
}
