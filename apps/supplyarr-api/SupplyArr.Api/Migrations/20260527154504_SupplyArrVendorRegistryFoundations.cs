using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SupplyArr.Api.Migrations
{
    /// <inheritdoc />
    public partial class SupplyArrVendorRegistryFoundations : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "supplyarr_audit_events",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    TenantId = table.Column<Guid>(type: "uuid", nullable: false),
                    ActorUserId = table.Column<Guid>(type: "uuid", nullable: true),
                    Action = table.Column<string>(type: "character varying(128)", maxLength: 128, nullable: false),
                    TargetType = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: false),
                    TargetId = table.Column<string>(type: "character varying(128)", maxLength: 128, nullable: true),
                    Result = table.Column<string>(type: "character varying(32)", maxLength: 32, nullable: false),
                    ReasonCode = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: true),
                    CorrelationId = table.Column<Guid>(type: "uuid", nullable: false),
                    OccurredAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_supplyarr_audit_events", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "supplyarr_external_parties",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    TenantId = table.Column<Guid>(type: "uuid", nullable: false),
                    PartyKey = table.Column<string>(type: "character varying(128)", maxLength: 128, nullable: false),
                    PartyType = table.Column<string>(type: "character varying(32)", maxLength: 32, nullable: false),
                    DisplayName = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: false),
                    LegalName = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: false),
                    TaxIdentifier = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: true),
                    ApprovalStatus = table.Column<string>(type: "character varying(32)", maxLength: 32, nullable: false),
                    Status = table.Column<string>(type: "character varying(32)", maxLength: 32, nullable: false),
                    Notes = table.Column<string>(type: "character varying(1024)", maxLength: 1024, nullable: false),
                    CreatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_supplyarr_external_parties", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "supplyarr_party_contacts",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    TenantId = table.Column<Guid>(type: "uuid", nullable: false),
                    ExternalPartyId = table.Column<Guid>(type: "uuid", nullable: false),
                    ContactName = table.Column<string>(type: "character varying(128)", maxLength: 128, nullable: false),
                    Email = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: false),
                    Phone = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: false),
                    RoleLabel = table.Column<string>(type: "character varying(128)", maxLength: 128, nullable: false),
                    IsPrimary = table.Column<bool>(type: "boolean", nullable: false),
                    CreatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_supplyarr_party_contacts", x => x.Id);
                    table.ForeignKey(
                        name: "FK_supplyarr_party_contacts_supplyarr_external_parties_Externa~",
                        column: x => x.ExternalPartyId,
                        principalTable: "supplyarr_external_parties",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_supplyarr_audit_events_TenantId",
                table: "supplyarr_audit_events",
                column: "TenantId");

            migrationBuilder.CreateIndex(
                name: "IX_supplyarr_audit_events_TenantId_OccurredAt",
                table: "supplyarr_audit_events",
                columns: new[] { "TenantId", "OccurredAt" });

            migrationBuilder.CreateIndex(
                name: "IX_supplyarr_external_parties_TenantId",
                table: "supplyarr_external_parties",
                column: "TenantId");

            migrationBuilder.CreateIndex(
                name: "IX_supplyarr_external_parties_TenantId_ApprovalStatus",
                table: "supplyarr_external_parties",
                columns: new[] { "TenantId", "ApprovalStatus" });

            migrationBuilder.CreateIndex(
                name: "IX_supplyarr_external_parties_TenantId_PartyKey",
                table: "supplyarr_external_parties",
                columns: new[] { "TenantId", "PartyKey" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_supplyarr_external_parties_TenantId_PartyType_Status",
                table: "supplyarr_external_parties",
                columns: new[] { "TenantId", "PartyType", "Status" });

            migrationBuilder.CreateIndex(
                name: "IX_supplyarr_party_contacts_ExternalPartyId",
                table: "supplyarr_party_contacts",
                column: "ExternalPartyId");

            migrationBuilder.CreateIndex(
                name: "IX_supplyarr_party_contacts_TenantId",
                table: "supplyarr_party_contacts",
                column: "TenantId");

            migrationBuilder.CreateIndex(
                name: "IX_supplyarr_party_contacts_TenantId_ExternalPartyId",
                table: "supplyarr_party_contacts",
                columns: new[] { "TenantId", "ExternalPartyId" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "supplyarr_audit_events");

            migrationBuilder.DropTable(
                name: "supplyarr_party_contacts");

            migrationBuilder.DropTable(
                name: "supplyarr_external_parties");
        }
    }
}
