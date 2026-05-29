using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace TrainArr.Api.Migrations
{
    /// <inheritdoc />
    public partial class TrainArrQualificationCheckRecords : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<Guid>(
                name: "AuthorizationQualificationCheckId",
                table: "trainarr_training_assignments",
                type: "uuid",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "trainarr_qualification_check_records",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    TenantId = table.Column<Guid>(type: "uuid", nullable: false),
                    StaffarrPersonId = table.Column<Guid>(type: "uuid", nullable: false),
                    QualificationKey = table.Column<string>(type: "character varying(128)", maxLength: 128, nullable: false),
                    Outcome = table.Column<string>(type: "character varying(16)", maxLength: 16, nullable: false),
                    ReasonCode = table.Column<string>(type: "character varying(128)", maxLength: 128, nullable: false),
                    Message = table.Column<string>(type: "character varying(2048)", maxLength: 2048, nullable: false),
                    RulePackKey = table.Column<string>(type: "character varying(128)", maxLength: 128, nullable: true),
                    TrainingDefinitionId = table.Column<Guid>(type: "uuid", nullable: true),
                    TrainingProgramId = table.Column<Guid>(type: "uuid", nullable: true),
                    ActorUserId = table.Column<Guid>(type: "uuid", nullable: true),
                    BatchId = table.Column<Guid>(type: "uuid", nullable: true),
                    CheckedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_trainarr_qualification_check_records", x => x.Id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_trainarr_qualification_check_records_TenantId",
                table: "trainarr_qualification_check_records",
                column: "TenantId");

            migrationBuilder.CreateIndex(
                name: "IX_trainarr_qualification_check_records_TenantId_BatchId",
                table: "trainarr_qualification_check_records",
                columns: new[] { "TenantId", "BatchId" });

            migrationBuilder.CreateIndex(
                name: "IX_trainarr_qualification_check_records_TenantId_Qualification~",
                table: "trainarr_qualification_check_records",
                columns: new[] { "TenantId", "QualificationKey", "CheckedAt" });

            migrationBuilder.CreateIndex(
                name: "IX_trainarr_qualification_check_records_TenantId_StaffarrPerso~",
                table: "trainarr_qualification_check_records",
                columns: new[] { "TenantId", "StaffarrPersonId", "CheckedAt" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "trainarr_qualification_check_records");

            migrationBuilder.DropColumn(
                name: "AuthorizationQualificationCheckId",
                table: "trainarr_training_assignments");
        }
    }
}
