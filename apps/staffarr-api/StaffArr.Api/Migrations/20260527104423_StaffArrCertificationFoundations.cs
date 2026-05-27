using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace StaffArr.Api.Migrations
{
    /// <inheritdoc />
    public partial class StaffArrCertificationFoundations : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "staffarr_certification_definitions",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    TenantId = table.Column<Guid>(type: "uuid", nullable: false),
                    CertificationKey = table.Column<string>(type: "character varying(128)", maxLength: 128, nullable: false),
                    Name = table.Column<string>(type: "character varying(128)", maxLength: 128, nullable: false),
                    Description = table.Column<string>(type: "character varying(512)", maxLength: 512, nullable: true),
                    Category = table.Column<string>(type: "character varying(32)", maxLength: 32, nullable: false),
                    DefaultValidityDays = table.Column<int>(type: "integer", nullable: true),
                    Status = table.Column<string>(type: "character varying(32)", maxLength: 32, nullable: false),
                    CreatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_staffarr_certification_definitions", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "staffarr_person_certifications",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    TenantId = table.Column<Guid>(type: "uuid", nullable: false),
                    PersonId = table.Column<Guid>(type: "uuid", nullable: false),
                    CertificationDefinitionId = table.Column<Guid>(type: "uuid", nullable: false),
                    SourceType = table.Column<string>(type: "character varying(32)", maxLength: 32, nullable: false),
                    Status = table.Column<string>(type: "character varying(32)", maxLength: 32, nullable: false),
                    GrantedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    ExpiresAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    Notes = table.Column<string>(type: "character varying(1024)", maxLength: 1024, nullable: true),
                    GrantedByUserId = table.Column<Guid>(type: "uuid", nullable: true),
                    ExternalPublicationId = table.Column<Guid>(type: "uuid", nullable: true),
                    CreatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_staffarr_person_certifications", x => x.Id);
                    table.ForeignKey(
                        name: "FK_staffarr_person_certifications_staffarr_certification_defin~",
                        column: x => x.CertificationDefinitionId,
                        principalTable: "staffarr_certification_definitions",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_staffarr_person_certifications_staffarr_people_PersonId",
                        column: x => x.PersonId,
                        principalTable: "staffarr_people",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_staffarr_certification_definitions_TenantId",
                table: "staffarr_certification_definitions",
                column: "TenantId");

            migrationBuilder.CreateIndex(
                name: "IX_staffarr_certification_definitions_TenantId_CertificationKey",
                table: "staffarr_certification_definitions",
                columns: new[] { "TenantId", "CertificationKey" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_staffarr_person_certifications_CertificationDefinitionId",
                table: "staffarr_person_certifications",
                column: "CertificationDefinitionId");

            migrationBuilder.CreateIndex(
                name: "IX_staffarr_person_certifications_PersonId",
                table: "staffarr_person_certifications",
                column: "PersonId");

            migrationBuilder.CreateIndex(
                name: "IX_staffarr_person_certifications_TenantId",
                table: "staffarr_person_certifications",
                column: "TenantId");

            migrationBuilder.CreateIndex(
                name: "IX_staffarr_person_certifications_TenantId_PersonId_Certificat~",
                table: "staffarr_person_certifications",
                columns: new[] { "TenantId", "PersonId", "CertificationDefinitionId", "Status" });

            migrationBuilder.CreateIndex(
                name: "IX_staffarr_person_certifications_TenantId_PersonId_Status",
                table: "staffarr_person_certifications",
                columns: new[] { "TenantId", "PersonId", "Status" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "staffarr_person_certifications");

            migrationBuilder.DropTable(
                name: "staffarr_certification_definitions");
        }
    }
}
