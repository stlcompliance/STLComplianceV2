using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace StaffArr.Api.Migrations
{
    /// <inheritdoc />
    public partial class StaffArrPersonOffboarding : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "staffarr_person_offboarding_records",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    TenantId = table.Column<Guid>(type: "uuid", nullable: false),
                    PersonId = table.Column<Guid>(type: "uuid", nullable: false),
                    Status = table.Column<string>(type: "character varying(32)", maxLength: 32, nullable: false),
                    SeparationDate = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    SeparationReason = table.Column<string>(type: "character varying(512)", maxLength: 512, nullable: true),
                    TargetEmploymentStatus = table.Column<string>(type: "character varying(32)", maxLength: 32, nullable: false),
                    DisableLoginRequested = table.Column<bool>(type: "boolean", nullable: false),
                    NewManagerPersonIdForReports = table.Column<Guid>(type: "uuid", nullable: true),
                    StartedByUserId = table.Column<Guid>(type: "uuid", nullable: false),
                    StartedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    CompletedByUserId = table.Column<Guid>(type: "uuid", nullable: true),
                    CompletedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    CreatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_staffarr_person_offboarding_records", x => x.Id);
                    table.ForeignKey(
                        name: "FK_staffarr_person_offboarding_records_staffarr_people_PersonId",
                        column: x => x.PersonId,
                        principalTable: "staffarr_people",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "staffarr_person_offboarding_steps",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    TenantId = table.Column<Guid>(type: "uuid", nullable: false),
                    OffboardingRecordId = table.Column<Guid>(type: "uuid", nullable: false),
                    StepKey = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: false),
                    Title = table.Column<string>(type: "character varying(128)", maxLength: 128, nullable: false),
                    Detail = table.Column<string>(type: "character varying(2048)", maxLength: 2048, nullable: false),
                    Status = table.Column<string>(type: "character varying(32)", maxLength: 32, nullable: false),
                    BlockerDetail = table.Column<string>(type: "character varying(1024)", maxLength: 1024, nullable: true),
                    SortOrder = table.Column<int>(type: "integer", nullable: false),
                    CompletedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    CompletedByUserId = table.Column<Guid>(type: "uuid", nullable: true),
                    CreatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_staffarr_person_offboarding_steps", x => x.Id);
                    table.ForeignKey(
                        name: "FK_staffarr_person_offboarding_steps_staffarr_person_offboardi~",
                        column: x => x.OffboardingRecordId,
                        principalTable: "staffarr_person_offboarding_records",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_staffarr_person_offboarding_records_PersonId",
                table: "staffarr_person_offboarding_records",
                column: "PersonId");

            migrationBuilder.CreateIndex(
                name: "IX_staffarr_person_offboarding_records_TenantId",
                table: "staffarr_person_offboarding_records",
                column: "TenantId");

            migrationBuilder.CreateIndex(
                name: "IX_staffarr_person_offboarding_records_TenantId_PersonId_Start~",
                table: "staffarr_person_offboarding_records",
                columns: new[] { "TenantId", "PersonId", "StartedAt" });

            migrationBuilder.CreateIndex(
                name: "IX_staffarr_person_offboarding_records_TenantId_PersonId_Status",
                table: "staffarr_person_offboarding_records",
                columns: new[] { "TenantId", "PersonId", "Status" });

            migrationBuilder.CreateIndex(
                name: "IX_staffarr_person_offboarding_steps_OffboardingRecordId",
                table: "staffarr_person_offboarding_steps",
                column: "OffboardingRecordId");

            migrationBuilder.CreateIndex(
                name: "IX_staffarr_person_offboarding_steps_TenantId",
                table: "staffarr_person_offboarding_steps",
                column: "TenantId");

            migrationBuilder.CreateIndex(
                name: "IX_staffarr_person_offboarding_steps_TenantId_OffboardingRecor~",
                table: "staffarr_person_offboarding_steps",
                columns: new[] { "TenantId", "OffboardingRecordId", "StepKey" },
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "staffarr_person_offboarding_steps");

            migrationBuilder.DropTable(
                name: "staffarr_person_offboarding_records");
        }
    }
}
