using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace StaffArr.Api.Migrations
{
    /// <inheritdoc />
    public partial class StaffArrPersonnelNotesDocumentsFoundations : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "staffarr_personnel_documents",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    TenantId = table.Column<Guid>(type: "uuid", nullable: false),
                    PersonId = table.Column<Guid>(type: "uuid", nullable: false),
                    DocumentTypeKey = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: false),
                    Title = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    FileName = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: false),
                    ContentType = table.Column<string>(type: "character varying(128)", maxLength: 128, nullable: false),
                    SizeBytes = table.Column<long>(type: "bigint", nullable: false),
                    StorageKey = table.Column<string>(type: "character varying(512)", maxLength: 512, nullable: false),
                    Description = table.Column<string>(type: "character varying(1024)", maxLength: 1024, nullable: true),
                    ExpiresAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    Status = table.Column<string>(type: "character varying(32)", maxLength: 32, nullable: false),
                    UploadedByUserId = table.Column<Guid>(type: "uuid", nullable: false),
                    CreatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_staffarr_personnel_documents", x => x.Id);
                    table.ForeignKey(
                        name: "FK_staffarr_personnel_documents_staffarr_people_PersonId",
                        column: x => x.PersonId,
                        principalTable: "staffarr_people",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "staffarr_personnel_notes",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    TenantId = table.Column<Guid>(type: "uuid", nullable: false),
                    PersonId = table.Column<Guid>(type: "uuid", nullable: false),
                    CategoryKey = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: false),
                    VisibilityKey = table.Column<string>(type: "character varying(32)", maxLength: 32, nullable: false),
                    Subject = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    Body = table.Column<string>(type: "character varying(8192)", maxLength: 8192, nullable: false),
                    Status = table.Column<string>(type: "character varying(32)", maxLength: 32, nullable: false),
                    CreatedByUserId = table.Column<Guid>(type: "uuid", nullable: false),
                    CreatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_staffarr_personnel_notes", x => x.Id);
                    table.ForeignKey(
                        name: "FK_staffarr_personnel_notes_staffarr_people_PersonId",
                        column: x => x.PersonId,
                        principalTable: "staffarr_people",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_staffarr_personnel_documents_PersonId",
                table: "staffarr_personnel_documents",
                column: "PersonId");

            migrationBuilder.CreateIndex(
                name: "IX_staffarr_personnel_documents_TenantId",
                table: "staffarr_personnel_documents",
                column: "TenantId");

            migrationBuilder.CreateIndex(
                name: "IX_staffarr_personnel_documents_TenantId_DocumentTypeKey_Status",
                table: "staffarr_personnel_documents",
                columns: new[] { "TenantId", "DocumentTypeKey", "Status" });

            migrationBuilder.CreateIndex(
                name: "IX_staffarr_personnel_documents_TenantId_PersonId_CreatedAt",
                table: "staffarr_personnel_documents",
                columns: new[] { "TenantId", "PersonId", "CreatedAt" });

            migrationBuilder.CreateIndex(
                name: "IX_staffarr_personnel_notes_PersonId",
                table: "staffarr_personnel_notes",
                column: "PersonId");

            migrationBuilder.CreateIndex(
                name: "IX_staffarr_personnel_notes_TenantId",
                table: "staffarr_personnel_notes",
                column: "TenantId");

            migrationBuilder.CreateIndex(
                name: "IX_staffarr_personnel_notes_TenantId_PersonId_CreatedAt",
                table: "staffarr_personnel_notes",
                columns: new[] { "TenantId", "PersonId", "CreatedAt" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "staffarr_personnel_documents");

            migrationBuilder.DropTable(
                name: "staffarr_personnel_notes");
        }
    }
}
