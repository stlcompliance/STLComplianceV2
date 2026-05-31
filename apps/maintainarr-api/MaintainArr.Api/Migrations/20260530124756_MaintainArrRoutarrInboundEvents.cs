using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace MaintainArr.Api.Migrations
{
    /// <inheritdoc />
    public partial class MaintainArrRoutarrInboundEvents : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "maintainarr_inbound_platform_events",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    TenantId = table.Column<Guid>(type: "uuid", nullable: false),
                    SourceProduct = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: false),
                    SourceEventId = table.Column<Guid>(type: "uuid", nullable: false),
                    EventKind = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: false),
                    RelatedEntityType = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: false),
                    RelatedEntityId = table.Column<Guid>(type: "uuid", nullable: false),
                    CorrelationId = table.Column<Guid>(type: "uuid", nullable: false),
                    PayloadJson = table.Column<string>(type: "text", nullable: false),
                    Outcome = table.Column<string>(type: "character varying(32)", maxLength: 32, nullable: false),
                    CreatedDefectId = table.Column<Guid>(type: "uuid", nullable: true),
                    OccurredAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    CreatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_maintainarr_inbound_platform_events", x => x.Id);
                    table.ForeignKey(
                        name: "FK_maintainarr_inbound_platform_events_maintainarr_defects_Cre~",
                        column: x => x.CreatedDefectId,
                        principalTable: "maintainarr_defects",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                });

            migrationBuilder.CreateIndex(
                name: "IX_maintainarr_inbound_platform_events_CreatedDefectId",
                table: "maintainarr_inbound_platform_events",
                column: "CreatedDefectId");

            migrationBuilder.CreateIndex(
                name: "IX_maintainarr_inbound_platform_events_TenantId",
                table: "maintainarr_inbound_platform_events",
                column: "TenantId");

            migrationBuilder.CreateIndex(
                name: "IX_maintainarr_inbound_platform_events_TenantId_CreatedDefectId",
                table: "maintainarr_inbound_platform_events",
                columns: new[] { "TenantId", "CreatedDefectId" });

            migrationBuilder.CreateIndex(
                name: "IX_maintainarr_inbound_platform_events_TenantId_EventKind_Crea~",
                table: "maintainarr_inbound_platform_events",
                columns: new[] { "TenantId", "EventKind", "CreatedAt" });

            migrationBuilder.CreateIndex(
                name: "IX_maintainarr_inbound_platform_events_TenantId_SourceProduct_~",
                table: "maintainarr_inbound_platform_events",
                columns: new[] { "TenantId", "SourceProduct", "SourceEventId" },
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "maintainarr_inbound_platform_events");
        }
    }
}
