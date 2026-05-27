using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace TrainArr.Api.Migrations
{
    /// <inheritdoc />
    public partial class TrainArrCertificationPublicationFoundations : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "trainarr_certification_publications",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    TenantId = table.Column<Guid>(type: "uuid", nullable: false),
                    StaffarrPersonId = table.Column<Guid>(type: "uuid", nullable: false),
                    QualificationKey = table.Column<string>(type: "character varying(128)", maxLength: 128, nullable: false),
                    QualificationName = table.Column<string>(type: "character varying(128)", maxLength: 128, nullable: false),
                    PublicationType = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: false),
                    BlockerType = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: false),
                    Message = table.Column<string>(type: "character varying(1024)", maxLength: 1024, nullable: false),
                    Status = table.Column<string>(type: "character varying(32)", maxLength: 32, nullable: false),
                    ExpiresAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    PublishedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    CreatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_trainarr_certification_publications", x => x.Id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_trainarr_certification_publications_TenantId",
                table: "trainarr_certification_publications",
                column: "TenantId");

            migrationBuilder.CreateIndex(
                name: "IX_trainarr_certification_publications_TenantId_StaffarrPerson~",
                table: "trainarr_certification_publications",
                columns: new[] { "TenantId", "StaffarrPersonId", "PublishedAt" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "trainarr_certification_publications");
        }
    }
}
