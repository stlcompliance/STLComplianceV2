using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace StaffArr.Api.Migrations
{
    /// <inheritdoc />
    public partial class _20260603170000_StaffArrIncidentNotesAttachments : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "staffarr_incident_attachments",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    TenantId = table.Column<Guid>(type: "uuid", nullable: false),
                    IncidentId = table.Column<Guid>(type: "uuid", nullable: false),
                    Title = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    FileName = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: false),
                    ContentType = table.Column<string>(type: "character varying(128)", maxLength: 128, nullable: false),
                    SizeBytes = table.Column<long>(type: "bigint", nullable: false),
                    StorageKey = table.Column<string>(type: "character varying(512)", maxLength: 512, nullable: false),
                    Description = table.Column<string>(type: "character varying(1024)", maxLength: 1024, nullable: true),
                    UploadedByUserId = table.Column<Guid>(type: "uuid", nullable: false),
                    CreatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_staffarr_incident_attachments", x => x.Id);
                    table.ForeignKey(
                        name: "FK_staffarr_incident_attachments_staffarr_personnel_incidents_~",
                        column: x => x.IncidentId,
                        principalTable: "staffarr_personnel_incidents",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "staffarr_incident_notes",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    TenantId = table.Column<Guid>(type: "uuid", nullable: false),
                    IncidentId = table.Column<Guid>(type: "uuid", nullable: false),
                    NoteTypeKey = table.Column<string>(type: "character varying(32)", maxLength: 32, nullable: false),
                    Subject = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    Body = table.Column<string>(type: "character varying(8192)", maxLength: 8192, nullable: false),
                    Status = table.Column<string>(type: "character varying(32)", maxLength: 32, nullable: false),
                    DueAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    CompletedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    CreatedByUserId = table.Column<Guid>(type: "uuid", nullable: false),
                    CreatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_staffarr_incident_notes", x => x.Id);
                    table.ForeignKey(
                        name: "FK_staffarr_incident_notes_staffarr_personnel_incidents_Incide~",
                        column: x => x.IncidentId,
                        principalTable: "staffarr_personnel_incidents",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_staffarr_incident_attachments_IncidentId",
                table: "staffarr_incident_attachments",
                column: "IncidentId");

            migrationBuilder.CreateIndex(
                name: "IX_staffarr_incident_attachments_TenantId",
                table: "staffarr_incident_attachments",
                column: "TenantId");

            migrationBuilder.CreateIndex(
                name: "IX_staffarr_incident_attachments_TenantId_IncidentId_CreatedAt",
                table: "staffarr_incident_attachments",
                columns: new[] { "TenantId", "IncidentId", "CreatedAt" });

            migrationBuilder.CreateIndex(
                name: "IX_staffarr_incident_notes_IncidentId",
                table: "staffarr_incident_notes",
                column: "IncidentId");

            migrationBuilder.CreateIndex(
                name: "IX_staffarr_incident_notes_TenantId",
                table: "staffarr_incident_notes",
                column: "TenantId");

            migrationBuilder.CreateIndex(
                name: "IX_staffarr_incident_notes_TenantId_IncidentId_CreatedAt",
                table: "staffarr_incident_notes",
                columns: new[] { "TenantId", "IncidentId", "CreatedAt" });

            migrationBuilder.CreateIndex(
                name: "IX_staffarr_incident_notes_TenantId_IncidentId_Status",
                table: "staffarr_incident_notes",
                columns: new[] { "TenantId", "IncidentId", "Status" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "staffarr_incident_attachments");

            migrationBuilder.DropTable(
                name: "staffarr_incident_notes");
        }
    }
}
