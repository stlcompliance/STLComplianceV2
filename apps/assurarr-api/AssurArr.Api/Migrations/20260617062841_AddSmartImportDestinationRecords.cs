using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace AssurArr.Api.Migrations
{
    /// <inheritdoc />
    public partial class AddSmartImportDestinationRecords : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
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
                name: "smart_import_destination_records");
        }
    }
}
