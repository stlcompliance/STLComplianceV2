using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace TrainArr.Api.Migrations
{
    /// <inheritdoc />
    public partial class TrainArrCitationAttachments : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "trainarr_training_citation_attachments",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    TenantId = table.Column<Guid>(type: "uuid", nullable: false),
                    EntityType = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: false),
                    EntityId = table.Column<Guid>(type: "uuid", nullable: false),
                    ComplianceCoreCitationId = table.Column<Guid>(type: "uuid", nullable: false),
                    CitationKey = table.Column<string>(type: "character varying(128)", maxLength: 128, nullable: false),
                    CitationVersion = table.Column<int>(type: "integer", nullable: false),
                    AttachedByUserId = table.Column<Guid>(type: "uuid", nullable: true),
                    CreatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_trainarr_training_citation_attachments", x => x.Id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_trainarr_training_citation_attachments_TenantId",
                table: "trainarr_training_citation_attachments",
                column: "TenantId");

            migrationBuilder.CreateIndex(
                name: "IX_trainarr_training_citation_attachments_TenantId_EntityType_~",
                table: "trainarr_training_citation_attachments",
                columns: new[] { "TenantId", "EntityType", "EntityId", "CitationKey" });

            migrationBuilder.CreateIndex(
                name: "IX_trainarr_training_citation_attachments_TenantId_EntityType~1",
                table: "trainarr_training_citation_attachments",
                columns: new[] { "TenantId", "EntityType", "EntityId", "ComplianceCoreCitationId" },
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "trainarr_training_citation_attachments");
        }
    }
}
