using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SupplyArr.Api.Migrations
{
    /// <inheritdoc />
    public partial class AddVendorOrderReadiness : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "supplyarr_tenant_vendor_order_settings",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    TenantId = table.Column<Guid>(type: "uuid", nullable: false),
                    AllowDestinationSummaryInVendorPortal = table.Column<bool>(type: "boolean", nullable: false),
                    MagicLinkTtlHours = table.Column<int>(type: "integer", nullable: false),
                    UpdatedByUserId = table.Column<Guid>(type: "uuid", nullable: true),
                    CreatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_supplyarr_tenant_vendor_order_settings", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "supplyarr_vendor_orders",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    TenantId = table.Column<Guid>(type: "uuid", nullable: false),
                    BrokerOrderId = table.Column<Guid>(type: "uuid", nullable: true),
                    BrokerOrderNumberSnapshot = table.Column<string>(type: "character varying(128)", maxLength: 128, nullable: true),
                    VendorId = table.Column<Guid>(type: "uuid", nullable: false),
                    VendorNameSnapshot = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: false),
                    VendorLocationId = table.Column<Guid>(type: "uuid", nullable: true),
                    PickupLocationNameSnapshot = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: true),
                    PickupAddressSnapshot = table.Column<string>(type: "character varying(1024)", maxLength: 1024, nullable: false),
                    CustomerIdSnapshot = table.Column<string>(type: "character varying(128)", maxLength: 128, nullable: true),
                    DeliveryLocationNameSnapshot = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: true),
                    DeliveryAddressSnapshot = table.Column<string>(type: "character varying(1024)", maxLength: 1024, nullable: true),
                    ItemDescription = table.Column<string>(type: "character varying(512)", maxLength: 512, nullable: false),
                    OrderedQuantity = table.Column<decimal>(type: "numeric(18,4)", precision: 18, scale: 4, nullable: false),
                    QuantityReady = table.Column<decimal>(type: "numeric(18,4)", precision: 18, scale: 4, nullable: false),
                    QuantityRemaining = table.Column<decimal>(type: "numeric(18,4)", precision: 18, scale: 4, nullable: false),
                    QuantityUom = table.Column<string>(type: "character varying(32)", maxLength: 32, nullable: false),
                    ExpectedReadyAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    ConfirmedReadyAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    PickupWindowStart = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    PickupWindowEnd = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    PickupInstructions = table.Column<string>(type: "character varying(2048)", maxLength: 2048, nullable: true),
                    Status = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: false),
                    CreatedByPersonId = table.Column<string>(type: "character varying(128)", maxLength: 128, nullable: true),
                    ParentVendorOrderId = table.Column<Guid>(type: "uuid", nullable: true),
                    SplitReason = table.Column<string>(type: "character varying(512)", maxLength: 512, nullable: true),
                    SplitFromStatusUpdateId = table.Column<Guid>(type: "uuid", nullable: true),
                    CreatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    CancelledAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    ClosedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_supplyarr_vendor_orders", x => x.Id);
                    table.ForeignKey(
                        name: "FK_supplyarr_vendor_orders_supplyarr_external_parties_VendorId",
                        column: x => x.VendorId,
                        principalTable: "supplyarr_external_parties",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_supplyarr_vendor_orders_supplyarr_vendor_orders_ParentVendo~",
                        column: x => x.ParentVendorOrderId,
                        principalTable: "supplyarr_vendor_orders",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "supplyarr_vendor_order_broker_decisions",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    TenantId = table.Column<Guid>(type: "uuid", nullable: false),
                    VendorOrderId = table.Column<Guid>(type: "uuid", nullable: false),
                    DecisionType = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: false),
                    AuthorizedQuantity = table.Column<decimal>(type: "numeric(18,4)", precision: 18, scale: 4, nullable: true),
                    SelectedTripId = table.Column<Guid>(type: "uuid", nullable: true),
                    Note = table.Column<string>(type: "character varying(1024)", maxLength: 1024, nullable: true),
                    DecidedByPersonId = table.Column<string>(type: "character varying(128)", maxLength: 128, nullable: true),
                    CreatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_supplyarr_vendor_order_broker_decisions", x => x.Id);
                    table.ForeignKey(
                        name: "FK_supplyarr_vendor_order_broker_decisions_supplyarr_vendor_or~",
                        column: x => x.VendorOrderId,
                        principalTable: "supplyarr_vendor_orders",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "supplyarr_vendor_order_document_links",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    TenantId = table.Column<Guid>(type: "uuid", nullable: false),
                    VendorOrderId = table.Column<Guid>(type: "uuid", nullable: false),
                    DocumentType = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: false),
                    FileName = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: false),
                    ContentType = table.Column<string>(type: "character varying(128)", maxLength: 128, nullable: false),
                    StorageProvider = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: true),
                    StorageKey = table.Column<string>(type: "character varying(512)", maxLength: 512, nullable: true),
                    RecordArrRecordId = table.Column<string>(type: "character varying(128)", maxLength: 128, nullable: false),
                    RecordArrRecordNumberSnapshot = table.Column<string>(type: "character varying(128)", maxLength: 128, nullable: false),
                    RecordArrFileId = table.Column<string>(type: "character varying(128)", maxLength: 128, nullable: false),
                    UploadedByVendorContactId = table.Column<string>(type: "character varying(128)", maxLength: 128, nullable: true),
                    UploadedByPersonId = table.Column<string>(type: "character varying(128)", maxLength: 128, nullable: true),
                    UploadedByMagicLinkId = table.Column<Guid>(type: "uuid", nullable: true),
                    UploadedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    DeletedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_supplyarr_vendor_order_document_links", x => x.Id);
                    table.ForeignKey(
                        name: "FK_supplyarr_vendor_order_document_links_supplyarr_vendor_orde~",
                        column: x => x.VendorOrderId,
                        principalTable: "supplyarr_vendor_orders",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "supplyarr_vendor_order_magic_links",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    TenantId = table.Column<Guid>(type: "uuid", nullable: false),
                    VendorOrderId = table.Column<Guid>(type: "uuid", nullable: false),
                    VendorId = table.Column<Guid>(type: "uuid", nullable: false),
                    TokenHash = table.Column<string>(type: "character varying(128)", maxLength: 128, nullable: false),
                    ExpiresAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    RevokedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    LastUsedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    CreatedByPersonId = table.Column<string>(type: "character varying(128)", maxLength: 128, nullable: true),
                    CreatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_supplyarr_vendor_order_magic_links", x => x.Id);
                    table.ForeignKey(
                        name: "FK_supplyarr_vendor_order_magic_links_supplyarr_vendor_orders_~",
                        column: x => x.VendorOrderId,
                        principalTable: "supplyarr_vendor_orders",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "supplyarr_vendor_order_status_updates",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    TenantId = table.Column<Guid>(type: "uuid", nullable: false),
                    VendorOrderId = table.Column<Guid>(type: "uuid", nullable: false),
                    PreviousStatus = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: true),
                    NewStatus = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: false),
                    OrderedQuantitySnapshot = table.Column<decimal>(type: "numeric(18,4)", precision: 18, scale: 4, nullable: false),
                    QuantityReady = table.Column<decimal>(type: "numeric(18,4)", precision: 18, scale: 4, nullable: false),
                    QuantityRemaining = table.Column<decimal>(type: "numeric(18,4)", precision: 18, scale: 4, nullable: false),
                    EstimatedReadyAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    ConfirmedReadyAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    PickupWindowStart = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    PickupWindowEnd = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    Note = table.Column<string>(type: "character varying(2048)", maxLength: 2048, nullable: true),
                    ExceptionReason = table.Column<string>(type: "character varying(1024)", maxLength: 1024, nullable: true),
                    Source = table.Column<string>(type: "character varying(32)", maxLength: 32, nullable: false),
                    SubmittedByVendorContactId = table.Column<Guid>(type: "uuid", nullable: true),
                    SubmittedByPersonId = table.Column<string>(type: "character varying(128)", maxLength: 128, nullable: true),
                    SubmittedByMagicLinkId = table.Column<Guid>(type: "uuid", nullable: true),
                    SubmittedIpHash = table.Column<string>(type: "character varying(128)", maxLength: 128, nullable: true),
                    SubmittedUserAgentHash = table.Column<string>(type: "character varying(128)", maxLength: 128, nullable: true),
                    CreatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_supplyarr_vendor_order_status_updates", x => x.Id);
                    table.ForeignKey(
                        name: "FK_supplyarr_vendor_order_status_updates_supplyarr_vendor_orde~",
                        column: x => x.VendorOrderId,
                        principalTable: "supplyarr_vendor_orders",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_supplyarr_tenant_vendor_order_settings_TenantId",
                table: "supplyarr_tenant_vendor_order_settings",
                column: "TenantId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_supplyarr_vendor_order_broker_decisions_TenantId",
                table: "supplyarr_vendor_order_broker_decisions",
                column: "TenantId");

            migrationBuilder.CreateIndex(
                name: "IX_supplyarr_vendor_order_broker_decisions_TenantId_VendorOrde~",
                table: "supplyarr_vendor_order_broker_decisions",
                columns: new[] { "TenantId", "VendorOrderId", "CreatedAt" });

            migrationBuilder.CreateIndex(
                name: "IX_supplyarr_vendor_order_broker_decisions_VendorOrderId",
                table: "supplyarr_vendor_order_broker_decisions",
                column: "VendorOrderId");

            migrationBuilder.CreateIndex(
                name: "IX_supplyarr_vendor_order_document_links_TenantId",
                table: "supplyarr_vendor_order_document_links",
                column: "TenantId");

            migrationBuilder.CreateIndex(
                name: "IX_supplyarr_vendor_order_document_links_TenantId_VendorOrderI~",
                table: "supplyarr_vendor_order_document_links",
                columns: new[] { "TenantId", "VendorOrderId", "UploadedAt" });

            migrationBuilder.CreateIndex(
                name: "IX_supplyarr_vendor_order_document_links_VendorOrderId",
                table: "supplyarr_vendor_order_document_links",
                column: "VendorOrderId");

            migrationBuilder.CreateIndex(
                name: "IX_supplyarr_vendor_order_magic_links_TenantId",
                table: "supplyarr_vendor_order_magic_links",
                column: "TenantId");

            migrationBuilder.CreateIndex(
                name: "IX_supplyarr_vendor_order_magic_links_TenantId_ExpiresAt",
                table: "supplyarr_vendor_order_magic_links",
                columns: new[] { "TenantId", "ExpiresAt" });

            migrationBuilder.CreateIndex(
                name: "IX_supplyarr_vendor_order_magic_links_TenantId_TokenHash",
                table: "supplyarr_vendor_order_magic_links",
                columns: new[] { "TenantId", "TokenHash" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_supplyarr_vendor_order_magic_links_TenantId_VendorOrderId",
                table: "supplyarr_vendor_order_magic_links",
                columns: new[] { "TenantId", "VendorOrderId" });

            migrationBuilder.CreateIndex(
                name: "IX_supplyarr_vendor_order_magic_links_VendorOrderId",
                table: "supplyarr_vendor_order_magic_links",
                column: "VendorOrderId");

            migrationBuilder.CreateIndex(
                name: "IX_supplyarr_vendor_order_status_updates_TenantId",
                table: "supplyarr_vendor_order_status_updates",
                column: "TenantId");

            migrationBuilder.CreateIndex(
                name: "IX_supplyarr_vendor_order_status_updates_TenantId_VendorOrderI~",
                table: "supplyarr_vendor_order_status_updates",
                columns: new[] { "TenantId", "VendorOrderId", "CreatedAt" });

            migrationBuilder.CreateIndex(
                name: "IX_supplyarr_vendor_order_status_updates_VendorOrderId",
                table: "supplyarr_vendor_order_status_updates",
                column: "VendorOrderId");

            migrationBuilder.CreateIndex(
                name: "IX_supplyarr_vendor_orders_ParentVendorOrderId",
                table: "supplyarr_vendor_orders",
                column: "ParentVendorOrderId");

            migrationBuilder.CreateIndex(
                name: "IX_supplyarr_vendor_orders_TenantId",
                table: "supplyarr_vendor_orders",
                column: "TenantId");

            migrationBuilder.CreateIndex(
                name: "IX_supplyarr_vendor_orders_TenantId_BrokerOrderId",
                table: "supplyarr_vendor_orders",
                columns: new[] { "TenantId", "BrokerOrderId" });

            migrationBuilder.CreateIndex(
                name: "IX_supplyarr_vendor_orders_TenantId_ParentVendorOrderId",
                table: "supplyarr_vendor_orders",
                columns: new[] { "TenantId", "ParentVendorOrderId" });

            migrationBuilder.CreateIndex(
                name: "IX_supplyarr_vendor_orders_TenantId_VendorId_Status_UpdatedAt",
                table: "supplyarr_vendor_orders",
                columns: new[] { "TenantId", "VendorId", "Status", "UpdatedAt" });

            migrationBuilder.CreateIndex(
                name: "IX_supplyarr_vendor_orders_VendorId",
                table: "supplyarr_vendor_orders",
                column: "VendorId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "supplyarr_tenant_vendor_order_settings");

            migrationBuilder.DropTable(
                name: "supplyarr_vendor_order_broker_decisions");

            migrationBuilder.DropTable(
                name: "supplyarr_vendor_order_document_links");

            migrationBuilder.DropTable(
                name: "supplyarr_vendor_order_magic_links");

            migrationBuilder.DropTable(
                name: "supplyarr_vendor_order_status_updates");

            migrationBuilder.DropTable(
                name: "supplyarr_vendor_orders");
        }
    }
}
