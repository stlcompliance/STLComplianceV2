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

            migrationBuilder.Sql(
                """
                CREATE TABLE IF NOT EXISTS print_export_logs (
                    "Id" uuid NOT NULL,
                    "TenantId" uuid NOT NULL,
                    "ProductKey" character varying(64) NOT NULL,
                    "SourceEntityType" character varying(128) NOT NULL,
                    "SourceEntityId" character varying(256) NOT NULL,
                    "SourceDisplayRef" character varying(256) NOT NULL,
                    "TemplateKey" character varying(160) NOT NULL,
                    "TemplateVersion" character varying(64) NOT NULL,
                    "Action" character varying(32) NOT NULL,
                    "DocumentStatus" character varying(32) NOT NULL,
                    "RequestedByPersonId" uuid NOT NULL,
                    "RequestedAtUtc" timestamp with time zone NOT NULL,
                    "CompletedAtUtc" timestamp with time zone NULL,
                    "RecordArrDocumentId" character varying(128) NULL,
                    "FileName" character varying(256) NULL,
                    "ContentHash" character varying(128) NULL,
                    "ReprintReason" character varying(1024) NULL,
                    "FailureReason" character varying(1024) NULL,
                    "MetadataJson" jsonb NULL,
                    CONSTRAINT "PK_print_export_logs" PRIMARY KEY ("Id")
                );

                CREATE TABLE IF NOT EXISTS smart_import_destination_records (
                    "Id" uuid NOT NULL,
                    "TenantId" uuid NOT NULL,
                    "ActorPersonId" uuid NOT NULL,
                    "ApprovedByPersonId" uuid NOT NULL,
                    "ImportBatchId" uuid NOT NULL,
                    "CommitPlanId" uuid NOT NULL,
                    "CommitStepId" uuid NOT NULL,
                    "DestinationProduct" character varying(64) NOT NULL,
                    "EntityType" character varying(128) NOT NULL,
                    "Operation" character varying(32) NOT NULL,
                    "IdempotencyKey" character varying(256) NOT NULL,
                    "PayloadJson" jsonb NOT NULL,
                    "RecordArrSourceRecordId" character varying(128) NULL,
                    "DisplayName" character varying(256) NOT NULL,
                    "Status" character varying(32) NOT NULL,
                    "CreatedAt" timestamp with time zone NOT NULL,
                    "UpdatedAt" timestamp with time zone NOT NULL,
                    CONSTRAINT "PK_smart_import_destination_records" PRIMARY KEY ("Id")
                );
                """);

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

            migrationBuilder.Sql(
                """
                CREATE INDEX IF NOT EXISTS "IX_print_export_logs_TenantId"
                    ON print_export_logs ("TenantId");

                CREATE INDEX IF NOT EXISTS "IX_print_export_logs_lookup"
                    ON print_export_logs ("TenantId", "ProductKey", "SourceEntityType", "SourceEntityId", "RequestedAtUtc");

                CREATE INDEX IF NOT EXISTS "IX_print_export_logs_action_lookup"
                    ON print_export_logs ("TenantId", "ProductKey", "Action", "RequestedAtUtc");

                CREATE INDEX IF NOT EXISTS "IX_smart_import_destination_records_TenantId"
                    ON smart_import_destination_records ("TenantId");

                CREATE UNIQUE INDEX IF NOT EXISTS "IX_smart_import_destination_records_idempotency"
                    ON smart_import_destination_records ("TenantId", "DestinationProduct", "IdempotencyKey");

                CREATE INDEX IF NOT EXISTS "IX_smart_import_destination_records_product_entity_created"
                    ON smart_import_destination_records ("TenantId", "DestinationProduct", "EntityType", "CreatedAt");
                """);
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
