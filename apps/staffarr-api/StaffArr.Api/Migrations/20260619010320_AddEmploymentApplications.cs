using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace StaffArr.Api.Migrations
{
    /// <inheritdoc />
    public partial class AddEmploymentApplications : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "staffarr_employment_application_templates",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    TenantId = table.Column<Guid>(type: "uuid", nullable: false),
                    TemplateKey = table.Column<string>(type: "character varying(128)", maxLength: 128, nullable: false),
                    TemplateName = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    Title = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    Subtitle = table.Column<string>(type: "character varying(512)", maxLength: 512, nullable: false),
                    SubmitLabel = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: false),
                    Version = table.Column<int>(type: "integer", nullable: false, defaultValue: 1),
                    Status = table.Column<string>(type: "character varying(32)", maxLength: 32, nullable: false),
                    PublicToken = table.Column<string>(type: "character varying(128)", maxLength: 128, nullable: false),
                    PublicLinkExpiresAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    TemplateJson = table.Column<string>(type: "character varying(32768)", maxLength: 32768, nullable: false),
                    CreatedByPersonId = table.Column<string>(type: "character varying(128)", maxLength: 128, nullable: true),
                    UpdatedByPersonId = table.Column<string>(type: "character varying(128)", maxLength: 128, nullable: true),
                    PublishedByPersonId = table.Column<string>(type: "character varying(128)", maxLength: 128, nullable: true),
                    PublishedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    RetiredByPersonId = table.Column<string>(type: "character varying(128)", maxLength: 128, nullable: true),
                    RetiredAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    CreatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_staffarr_employment_application_templates", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "staffarr_employment_application_submissions",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    TenantId = table.Column<Guid>(type: "uuid", nullable: false),
                    EmploymentApplicationTemplateId = table.Column<Guid>(type: "uuid", nullable: false),
                    TemplateKey = table.Column<string>(type: "character varying(128)", maxLength: 128, nullable: false),
                    TemplateVersion = table.Column<int>(type: "integer", nullable: false, defaultValue: 1),
                    CreatedPersonId = table.Column<Guid>(type: "uuid", nullable: true),
                    Status = table.Column<string>(type: "character varying(32)", maxLength: 32, nullable: false),
                    ApplicantDisplayName = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    ApplicantEmail = table.Column<string>(type: "character varying(320)", maxLength: 320, nullable: false),
                    RawAnswersJson = table.Column<string>(type: "character varying(32768)", maxLength: 32768, nullable: false),
                    CreateRequestJson = table.Column<string>(type: "character varying(32768)", maxLength: 32768, nullable: false),
                    EventualProfileJson = table.Column<string>(type: "character varying(32768)", maxLength: 32768, nullable: false),
                    SourceIpAddress = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: true),
                    UserAgent = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: true),
                    ReviewerNotes = table.Column<string>(type: "character varying(1024)", maxLength: 1024, nullable: true),
                    SubmittedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    CreatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_staffarr_employment_application_submissions", x => x.Id);
                    table.ForeignKey(
                        name: "FK_staffarr_employment_application_submissions_staffarr_employment_application_templates_EmploymentApplicationTemplateId",
                        column: x => x.EmploymentApplicationTemplateId,
                        principalTable: "staffarr_employment_application_templates",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_staffarr_employment_application_submissions_staffarr_people_CreatedPersonId",
                        column: x => x.CreatedPersonId,
                        principalTable: "staffarr_people",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateIndex(
                name: "IX_staffarr_employment_application_templates_PublicToken",
                table: "staffarr_employment_application_templates",
                column: "PublicToken",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_staffarr_employment_application_templates_TenantId",
                table: "staffarr_employment_application_templates",
                column: "TenantId");

            migrationBuilder.CreateIndex(
                name: "IX_staffarr_employment_application_templates_TenantId_TemplateKey_Version",
                table: "staffarr_employment_application_templates",
                columns: new[] { "TenantId", "TemplateKey", "Version" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_staffarr_employment_application_submissions_CreatedPersonId",
                table: "staffarr_employment_application_submissions",
                column: "CreatedPersonId");

            migrationBuilder.CreateIndex(
                name: "IX_staffarr_employment_application_submissions_EmploymentApplicationTemplateId",
                table: "staffarr_employment_application_submissions",
                column: "EmploymentApplicationTemplateId");

            migrationBuilder.CreateIndex(
                name: "IX_staffarr_employment_application_submissions_TenantId",
                table: "staffarr_employment_application_submissions",
                column: "TenantId");

            migrationBuilder.CreateIndex(
                name: "IX_staffarr_employment_application_submissions_TenantId_SubmittedAt",
                table: "staffarr_employment_application_submissions",
                columns: new[] { "TenantId", "SubmittedAt" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "staffarr_employment_application_submissions");

            migrationBuilder.DropTable(
                name: "staffarr_employment_application_templates");
        }
    }
}
