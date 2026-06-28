using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace OrdArr.Api.Migrations
{
    /// <inheritdoc />
    public partial class AddOrdArrDurableOrderStore : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "ordarr_idempotency_records",
                columns: table => new
                {
                    Id = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: false),
                    TenantId = table.Column<Guid>(type: "uuid", nullable: false),
                    OperationKey = table.Column<string>(type: "character varying(128)", maxLength: 128, nullable: false),
                    IdempotencyKey = table.Column<string>(type: "character varying(512)", maxLength: 512, nullable: false),
                    ResourceId = table.Column<string>(type: "character varying(128)", maxLength: 128, nullable: false),
                    CreatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ordarr_idempotency_records", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "ordarr_order_records",
                columns: table => new
                {
                    OrderId = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: false),
                    TenantId = table.Column<Guid>(type: "uuid", nullable: false),
                    OrderNumber = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: false),
                    LifecycleStatus = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: false),
                    CustomerDisplayName = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: false),
                    OwnerPersonId = table.Column<string>(type: "character varying(128)", maxLength: 128, nullable: false),
                    CreatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    PayloadJson = table.Column<string>(type: "jsonb", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ordarr_order_records", x => x.OrderId);
                });

            migrationBuilder.CreateTable(
                name: "platform_metadata",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    TenantId = table.Column<Guid>(type: "uuid", nullable: false),
                    Key = table.Column<string>(type: "character varying(128)", maxLength: 128, nullable: false),
                    Value = table.Column<string>(type: "character varying(2048)", maxLength: 2048, nullable: false),
                    CreatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    ModifiedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    CreatedBy = table.Column<string>(type: "character varying(128)", maxLength: 128, nullable: true),
                    ModifiedBy = table.Column<string>(type: "character varying(128)", maxLength: 128, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_platform_metadata", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "print_export_logs",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    TenantId = table.Column<Guid>(type: "uuid", nullable: false),
                    ProductKey = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: false),
                    SourceEntityType = table.Column<string>(type: "character varying(128)", maxLength: 128, nullable: false),
                    SourceEntityId = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: false),
                    SourceDisplayRef = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: false),
                    TemplateKey = table.Column<string>(type: "character varying(160)", maxLength: 160, nullable: false),
                    TemplateVersion = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: false),
                    Action = table.Column<string>(type: "character varying(32)", maxLength: 32, nullable: false),
                    DocumentStatus = table.Column<string>(type: "character varying(32)", maxLength: 32, nullable: false),
                    RequestedByPersonId = table.Column<Guid>(type: "uuid", nullable: false),
                    RequestedAtUtc = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    CompletedAtUtc = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    RecordArrDocumentId = table.Column<string>(type: "character varying(128)", maxLength: 128, nullable: true),
                    FileName = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: true),
                    ContentHash = table.Column<string>(type: "character varying(128)", maxLength: 128, nullable: true),
                    ReprintReason = table.Column<string>(type: "character varying(1024)", maxLength: 1024, nullable: true),
                    FailureReason = table.Column<string>(type: "character varying(1024)", maxLength: 1024, nullable: true),
                    MetadataJson = table.Column<string>(type: "jsonb", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_print_export_logs", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "smart_import_destination_records",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    TenantId = table.Column<Guid>(type: "uuid", nullable: false),
                    ActorPersonId = table.Column<Guid>(type: "uuid", nullable: false),
                    ApprovedByPersonId = table.Column<Guid>(type: "uuid", nullable: false),
                    ImportBatchId = table.Column<Guid>(type: "uuid", nullable: false),
                    CommitPlanId = table.Column<Guid>(type: "uuid", nullable: false),
                    CommitStepId = table.Column<Guid>(type: "uuid", nullable: false),
                    DestinationProduct = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: false),
                    EntityType = table.Column<string>(type: "character varying(128)", maxLength: 128, nullable: false),
                    Operation = table.Column<string>(type: "character varying(32)", maxLength: 32, nullable: false),
                    IdempotencyKey = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: false),
                    PayloadJson = table.Column<string>(type: "jsonb", nullable: false),
                    RecordArrSourceRecordId = table.Column<string>(type: "character varying(128)", maxLength: 128, nullable: true),
                    DisplayName = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: false),
                    Status = table.Column<string>(type: "character varying(32)", maxLength: 32, nullable: false),
                    CreatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_smart_import_destination_records", x => x.Id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_ordarr_idempotency_records_TenantId_OperationKey_Idempotenc~",
                table: "ordarr_idempotency_records",
                columns: new[] { "TenantId", "OperationKey", "IdempotencyKey" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_ordarr_order_records_TenantId",
                table: "ordarr_order_records",
                column: "TenantId");

            migrationBuilder.CreateIndex(
                name: "IX_ordarr_order_records_TenantId_LifecycleStatus_UpdatedAt",
                table: "ordarr_order_records",
                columns: new[] { "TenantId", "LifecycleStatus", "UpdatedAt" });

            migrationBuilder.CreateIndex(
                name: "IX_ordarr_order_records_TenantId_OrderNumber",
                table: "ordarr_order_records",
                columns: new[] { "TenantId", "OrderNumber" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_ordarr_order_records_TenantId_UpdatedAt",
                table: "ordarr_order_records",
                columns: new[] { "TenantId", "UpdatedAt" });

            migrationBuilder.CreateIndex(
                name: "IX_platform_metadata_TenantId",
                table: "platform_metadata",
                column: "TenantId");

            migrationBuilder.CreateIndex(
                name: "IX_platform_metadata_TenantId_Key",
                table: "platform_metadata",
                columns: new[] { "TenantId", "Key" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_print_export_logs_action_lookup",
                table: "print_export_logs",
                columns: new[] { "TenantId", "ProductKey", "Action", "RequestedAtUtc" });

            migrationBuilder.CreateIndex(
                name: "IX_print_export_logs_lookup",
                table: "print_export_logs",
                columns: new[] { "TenantId", "ProductKey", "SourceEntityType", "SourceEntityId", "RequestedAtUtc" });

            migrationBuilder.CreateIndex(
                name: "IX_print_export_logs_TenantId",
                table: "print_export_logs",
                column: "TenantId");

            migrationBuilder.CreateIndex(
                name: "IX_smart_import_destination_records_idempotency",
                table: "smart_import_destination_records",
                columns: new[] { "TenantId", "DestinationProduct", "IdempotencyKey" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_smart_import_destination_records_product_entity_created",
                table: "smart_import_destination_records",
                columns: new[] { "TenantId", "DestinationProduct", "EntityType", "CreatedAt" });

            migrationBuilder.CreateIndex(
                name: "IX_smart_import_destination_records_TenantId",
                table: "smart_import_destination_records",
                column: "TenantId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "ordarr_idempotency_records");

            migrationBuilder.DropTable(
                name: "ordarr_order_records");

            migrationBuilder.DropTable(
                name: "platform_metadata");

            migrationBuilder.DropTable(
                name: "print_export_logs");

            migrationBuilder.DropTable(
                name: "smart_import_destination_records");
        }
    }
}
