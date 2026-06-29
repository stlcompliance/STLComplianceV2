using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace RecordArr.Api.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddRecordArrObjectStoreMetadataFixityHistory : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "recordarr_object_store_fixity_observations",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    TenantId = table.Column<Guid>(type: "uuid", nullable: false),
                    FixityObservationId = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: false),
                    FileId = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: false),
                    RecordId = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: false),
                    StorageProvider = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: false),
                    StorageKey = table.Column<string>(type: "character varying(512)", maxLength: 512, nullable: false),
                    SizeBytes = table.Column<long>(type: "bigint", nullable: false),
                    ExpectedChecksumSha256 = table.Column<string>(type: "character varying(128)", maxLength: 128, nullable: false),
                    ObservedChecksumSha256 = table.Column<string>(type: "character varying(128)", maxLength: 128, nullable: true),
                    Status = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: false),
                    ObservationSource = table.Column<string>(type: "character varying(96)", maxLength: 96, nullable: false),
                    IntegrityCheckRef = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: true),
                    ReconciliationRef = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: true),
                    ObservedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    ObservedByPersonId = table.Column<string>(type: "character varying(128)", maxLength: 128, nullable: false),
                    FailureReason = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: true),
                    PayloadJson = table.Column<string>(type: "jsonb", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_recordarr_object_store_fixity_observations", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "recordarr_object_store_objects",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    TenantId = table.Column<Guid>(type: "uuid", nullable: false),
                    ObjectStoreObjectId = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: false),
                    FileId = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: false),
                    RecordId = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: false),
                    StorageProvider = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: false),
                    StorageKey = table.Column<string>(type: "character varying(512)", maxLength: 512, nullable: false),
                    SizeBytes = table.Column<long>(type: "bigint", nullable: false),
                    ExpectedChecksumSha256 = table.Column<string>(type: "character varying(128)", maxLength: 128, nullable: false),
                    LastObservedChecksumSha256 = table.Column<string>(type: "character varying(128)", maxLength: 128, nullable: true),
                    Status = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: false),
                    LastObservationSource = table.Column<string>(type: "character varying(96)", maxLength: 96, nullable: false),
                    LastIntegrityCheckRef = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: true),
                    LastReconciliationRef = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: true),
                    LastObservedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    LastObservedByPersonId = table.Column<string>(type: "character varying(128)", maxLength: 128, nullable: false),
                    FailureReason = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: true),
                    PayloadJson = table.Column<string>(type: "jsonb", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_recordarr_object_store_objects", x => x.Id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_recordarr_object_store_fixity_observations_TenantId",
                table: "recordarr_object_store_fixity_observations",
                column: "TenantId");

            migrationBuilder.CreateIndex(
                name: "IX_recordarr_object_store_fixity_observations_TenantId_FileId_~",
                table: "recordarr_object_store_fixity_observations",
                columns: new[] { "TenantId", "FileId", "ObservedAt" });

            migrationBuilder.CreateIndex(
                name: "IX_recordarr_object_store_fixity_observations_TenantId_FixityO~",
                table: "recordarr_object_store_fixity_observations",
                columns: new[] { "TenantId", "FixityObservationId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_recordarr_object_store_fixity_observations_TenantId_Reconci~",
                table: "recordarr_object_store_fixity_observations",
                columns: new[] { "TenantId", "ReconciliationRef" });

            migrationBuilder.CreateIndex(
                name: "IX_recordarr_object_store_fixity_observations_TenantId_RecordI~",
                table: "recordarr_object_store_fixity_observations",
                columns: new[] { "TenantId", "RecordId", "ObservedAt" });

            migrationBuilder.CreateIndex(
                name: "IX_recordarr_object_store_fixity_observations_TenantId_Status_~",
                table: "recordarr_object_store_fixity_observations",
                columns: new[] { "TenantId", "Status", "ObservedAt" });

            migrationBuilder.CreateIndex(
                name: "IX_recordarr_object_store_objects_TenantId",
                table: "recordarr_object_store_objects",
                column: "TenantId");

            migrationBuilder.CreateIndex(
                name: "IX_recordarr_object_store_objects_TenantId_FileId",
                table: "recordarr_object_store_objects",
                columns: new[] { "TenantId", "FileId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_recordarr_object_store_objects_TenantId_ObjectStoreObjectId",
                table: "recordarr_object_store_objects",
                columns: new[] { "TenantId", "ObjectStoreObjectId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_recordarr_object_store_objects_TenantId_RecordId_LastObserv~",
                table: "recordarr_object_store_objects",
                columns: new[] { "TenantId", "RecordId", "LastObservedAt" });

            migrationBuilder.CreateIndex(
                name: "IX_recordarr_object_store_objects_TenantId_Status_LastObserved~",
                table: "recordarr_object_store_objects",
                columns: new[] { "TenantId", "Status", "LastObservedAt" });

            migrationBuilder.CreateIndex(
                name: "IX_recordarr_object_store_objects_TenantId_StorageProvider_Sto~",
                table: "recordarr_object_store_objects",
                columns: new[] { "TenantId", "StorageProvider", "StorageKey" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "recordarr_object_store_fixity_observations");

            migrationBuilder.DropTable(
                name: "recordarr_object_store_objects");
        }
    }
}
