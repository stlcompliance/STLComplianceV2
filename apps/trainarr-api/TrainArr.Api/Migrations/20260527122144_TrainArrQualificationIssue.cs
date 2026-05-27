using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace TrainArr.Api.Migrations
{
    /// <inheritdoc />
    public partial class TrainArrQualificationIssue : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "trainarr_qualification_issues",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    TenantId = table.Column<Guid>(type: "uuid", nullable: false),
                    TrainingAssignmentId = table.Column<Guid>(type: "uuid", nullable: false),
                    StaffarrPersonId = table.Column<Guid>(type: "uuid", nullable: false),
                    QualificationKey = table.Column<string>(type: "character varying(128)", maxLength: 128, nullable: false),
                    QualificationName = table.Column<string>(type: "character varying(128)", maxLength: 128, nullable: false),
                    GrantPublicationId = table.Column<Guid>(type: "uuid", nullable: false),
                    Status = table.Column<string>(type: "character varying(32)", maxLength: 32, nullable: false),
                    IssuedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    CreatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_trainarr_qualification_issues", x => x.Id);
                    table.ForeignKey(
                        name: "FK_trainarr_qualification_issues_trainarr_training_assignments~",
                        column: x => x.TrainingAssignmentId,
                        principalTable: "trainarr_training_assignments",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_trainarr_qualification_issues_TenantId",
                table: "trainarr_qualification_issues",
                column: "TenantId");

            migrationBuilder.CreateIndex(
                name: "IX_trainarr_qualification_issues_TenantId_GrantPublicationId",
                table: "trainarr_qualification_issues",
                columns: new[] { "TenantId", "GrantPublicationId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_trainarr_qualification_issues_TenantId_TrainingAssignmentId",
                table: "trainarr_qualification_issues",
                columns: new[] { "TenantId", "TrainingAssignmentId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_trainarr_qualification_issues_TrainingAssignmentId",
                table: "trainarr_qualification_issues",
                column: "TrainingAssignmentId",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "trainarr_qualification_issues");
        }
    }
}
