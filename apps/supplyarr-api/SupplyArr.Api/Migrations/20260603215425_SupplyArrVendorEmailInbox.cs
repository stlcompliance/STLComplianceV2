using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SupplyArr.Api.Migrations
{
    /// <inheritdoc />
    public partial class SupplyArrVendorEmailInbox : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "supplyarr_vendor_email_inbox_messages",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    TenantId = table.Column<Guid>(type: "uuid", nullable: false),
                    MessageKey = table.Column<string>(type: "character varying(128)", maxLength: 128, nullable: false),
                    MessageKind = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: false),
                    SenderEmail = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: false),
                    SenderName = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: false),
                    Subject = table.Column<string>(type: "character varying(512)", maxLength: 512, nullable: false),
                    BodyPreview = table.Column<string>(type: "character varying(4096)", maxLength: 4096, nullable: false),
                    MatchStatus = table.Column<string>(type: "character varying(32)", maxLength: 32, nullable: false),
                    MatchReason = table.Column<string>(type: "character varying(1024)", maxLength: 1024, nullable: false),
                    VendorPartyId = table.Column<Guid>(type: "uuid", nullable: true),
                    VendorPartyKey = table.Column<string>(type: "character varying(128)", maxLength: 128, nullable: true),
                    VendorDisplayName = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: true),
                    LinkedReferenceType = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: true),
                    LinkedReferenceId = table.Column<Guid>(type: "uuid", nullable: true),
                    LinkedReferenceKey = table.Column<string>(type: "character varying(128)", maxLength: 128, nullable: true),
                    ReceivedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    CreatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    ProcessedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_supplyarr_vendor_email_inbox_messages", x => x.Id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_supplyarr_rfq_vendor_invitations_TenantId_RfqId_PortalAcces~",
                table: "supplyarr_rfq_vendor_invitations",
                columns: new[] { "TenantId", "RfqId", "PortalAccessCode" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_supplyarr_vendor_email_inbox_messages_TenantId",
                table: "supplyarr_vendor_email_inbox_messages",
                column: "TenantId");

            migrationBuilder.CreateIndex(
                name: "IX_supplyarr_vendor_email_inbox_messages_TenantId_LinkedRefere~",
                table: "supplyarr_vendor_email_inbox_messages",
                columns: new[] { "TenantId", "LinkedReferenceType", "LinkedReferenceId" });

            migrationBuilder.CreateIndex(
                name: "IX_supplyarr_vendor_email_inbox_messages_TenantId_MatchStatus_~",
                table: "supplyarr_vendor_email_inbox_messages",
                columns: new[] { "TenantId", "MatchStatus", "ReceivedAt" });

            migrationBuilder.CreateIndex(
                name: "IX_supplyarr_vendor_email_inbox_messages_TenantId_MessageKey",
                table: "supplyarr_vendor_email_inbox_messages",
                columns: new[] { "TenantId", "MessageKey" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_supplyarr_vendor_email_inbox_messages_TenantId_MessageKind_~",
                table: "supplyarr_vendor_email_inbox_messages",
                columns: new[] { "TenantId", "MessageKind", "ReceivedAt" });

            migrationBuilder.CreateIndex(
                name: "IX_supplyarr_vendor_email_inbox_messages_TenantId_VendorPartyI~",
                table: "supplyarr_vendor_email_inbox_messages",
                columns: new[] { "TenantId", "VendorPartyId", "ReceivedAt" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "supplyarr_vendor_email_inbox_messages");
        }
    }
}
