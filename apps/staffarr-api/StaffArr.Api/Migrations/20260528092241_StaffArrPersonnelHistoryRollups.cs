using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace StaffArr.Api.Migrations
{
    /// <inheritdoc />
    public partial class StaffArrPersonnelHistoryRollups : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "staffarr_personnel_history_rollups",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    TenantId = table.Column<Guid>(type: "uuid", nullable: false),
                    PersonId = table.Column<Guid>(type: "uuid", nullable: false),
                    EventCount = table.Column<int>(type: "integer", nullable: false),
                    IncidentCount = table.Column<int>(type: "integer", nullable: false),
                    CertificationCount = table.Column<int>(type: "integer", nullable: false),
                    PermissionCount = table.Column<int>(type: "integer", nullable: false),
                    ReadinessCount = table.Column<int>(type: "integer", nullable: false),
                    TrainingBlockerCount = table.Column<int>(type: "integer", nullable: false),
                    PersonnelNoteCount = table.Column<int>(type: "integer", nullable: false),
                    PersonnelDocumentCount = table.Column<int>(type: "integer", nullable: false),
                    LastEventAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    ComputedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    CreatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_staffarr_personnel_history_rollups", x => x.Id);
                    table.ForeignKey(
                        name: "FK_staffarr_personnel_history_rollups_staffarr_people_PersonId",
                        column: x => x.PersonId,
                        principalTable: "staffarr_people",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "staffarr_personnel_history_events",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    TenantId = table.Column<Guid>(type: "uuid", nullable: false),
                    PersonId = table.Column<Guid>(type: "uuid", nullable: false),
                    RollupId = table.Column<Guid>(type: "uuid", nullable: false),
                    EntryId = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: false),
                    Category = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: false),
                    EventType = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: false),
                    Title = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: false),
                    Detail = table.Column<string>(type: "character varying(2048)", maxLength: 2048, nullable: true),
                    OccurredAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    ActorUserId = table.Column<Guid>(type: "uuid", nullable: true),
                    SourceEntityType = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: false),
                    SourceEntityId = table.Column<string>(type: "character varying(128)", maxLength: 128, nullable: false),
                    ExternalReferenceId = table.Column<string>(type: "character varying(128)", maxLength: 128, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_staffarr_personnel_history_events", x => x.Id);
                    table.ForeignKey(
                        name: "FK_staffarr_personnel_history_events_staffarr_people_PersonId",
                        column: x => x.PersonId,
                        principalTable: "staffarr_people",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_staffarr_personnel_history_events_staffarr_personnel_histor~",
                        column: x => x.RollupId,
                        principalTable: "staffarr_personnel_history_rollups",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_staffarr_personnel_history_events_PersonId",
                table: "staffarr_personnel_history_events",
                column: "PersonId");

            migrationBuilder.CreateIndex(
                name: "IX_staffarr_personnel_history_events_RollupId",
                table: "staffarr_personnel_history_events",
                column: "RollupId");

            migrationBuilder.CreateIndex(
                name: "IX_staffarr_personnel_history_events_TenantId",
                table: "staffarr_personnel_history_events",
                column: "TenantId");

            migrationBuilder.CreateIndex(
                name: "IX_staffarr_personnel_history_events_TenantId_PersonId_EntryId",
                table: "staffarr_personnel_history_events",
                columns: new[] { "TenantId", "PersonId", "EntryId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_staffarr_personnel_history_events_TenantId_PersonId_Occurre~",
                table: "staffarr_personnel_history_events",
                columns: new[] { "TenantId", "PersonId", "OccurredAt" });

            migrationBuilder.CreateIndex(
                name: "IX_staffarr_personnel_history_rollups_PersonId",
                table: "staffarr_personnel_history_rollups",
                column: "PersonId");

            migrationBuilder.CreateIndex(
                name: "IX_staffarr_personnel_history_rollups_TenantId",
                table: "staffarr_personnel_history_rollups",
                column: "TenantId");

            migrationBuilder.CreateIndex(
                name: "IX_staffarr_personnel_history_rollups_TenantId_ComputedAt",
                table: "staffarr_personnel_history_rollups",
                columns: new[] { "TenantId", "ComputedAt" });

            migrationBuilder.CreateIndex(
                name: "IX_staffarr_personnel_history_rollups_TenantId_PersonId",
                table: "staffarr_personnel_history_rollups",
                columns: new[] { "TenantId", "PersonId" },
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "staffarr_personnel_history_events");

            migrationBuilder.DropTable(
                name: "staffarr_personnel_history_rollups");
        }
    }
}
