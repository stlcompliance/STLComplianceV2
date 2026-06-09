using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace NexArr.Api.Migrations
{
    /// <inheritdoc />
    public partial class NexArrReferenceData : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "reference_audit_events",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    ActorPersonId = table.Column<Guid>(type: "uuid", nullable: true),
                    Action = table.Column<string>(type: "character varying(128)", maxLength: 128, nullable: false),
                    EntityType = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: false),
                    EntityId = table.Column<Guid>(type: "uuid", nullable: true),
                    BeforeSnapshotJson = table.Column<string>(type: "jsonb", nullable: true),
                    AfterSnapshotJson = table.Column<string>(type: "jsonb", nullable: true),
                    CreatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_reference_audit_events", x => x.Id);
                    table.ForeignKey(
                        name: "FK_reference_audit_events_platform_users_ActorPersonId",
                        column: x => x.ActorPersonId,
                        principalTable: "platform_users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                });

            migrationBuilder.CreateTable(
                name: "reference_datasets",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    Key = table.Column<string>(type: "character varying(128)", maxLength: 128, nullable: false),
                    Name = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    Category = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: false),
                    OwnerService = table.Column<string>(type: "character varying(128)", maxLength: 128, nullable: false),
                    Status = table.Column<string>(type: "character varying(32)", maxLength: 32, nullable: false),
                    CurrentPublishedVersion = table.Column<string>(type: "character varying(32)", maxLength: 32, nullable: true),
                    CreatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_reference_datasets", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "reference_sources",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    Key = table.Column<string>(type: "character varying(128)", maxLength: 128, nullable: false),
                    Name = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    SourceType = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: false),
                    ConnectorType = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: false),
                    AuthorityRank = table.Column<int>(type: "integer", nullable: false),
                    RefreshCadence = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: false),
                    TermsNotes = table.Column<string>(type: "character varying(512)", maxLength: 512, nullable: true),
                    Enabled = table.Column<bool>(type: "boolean", nullable: false),
                    CreatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_reference_sources", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "reference_publish_events",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    DatasetId = table.Column<Guid>(type: "uuid", nullable: false),
                    PublishedVersion = table.Column<string>(type: "character varying(32)", maxLength: 32, nullable: false),
                    PublishedByPersonId = table.Column<Guid>(type: "uuid", nullable: true),
                    Summary = table.Column<string>(type: "character varying(1024)", maxLength: 1024, nullable: false),
                    CreatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_reference_publish_events", x => x.Id);
                    table.ForeignKey(
                        name: "FK_reference_publish_events_platform_users_PublishedByPersonId",
                        column: x => x.PublishedByPersonId,
                        principalTable: "platform_users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                    table.ForeignKey(
                        name: "FK_reference_publish_events_reference_datasets_DatasetId",
                        column: x => x.DatasetId,
                        principalTable: "reference_datasets",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "ingestion_jobs",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    DatasetId = table.Column<Guid>(type: "uuid", nullable: false),
                    SourceId = table.Column<Guid>(type: "uuid", nullable: false),
                    TenantId = table.Column<Guid>(type: "uuid", nullable: true),
                    RequestedByPersonId = table.Column<Guid>(type: "uuid", nullable: true),
                    Status = table.Column<string>(type: "character varying(32)", maxLength: 32, nullable: false),
                    RawObjectKey = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: true),
                    FileName = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: true),
                    StartedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    CompletedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    ErrorSummary = table.Column<string>(type: "character varying(1024)", maxLength: 1024, nullable: true),
                    CreatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ingestion_jobs", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ingestion_jobs_platform_users_RequestedByPersonId",
                        column: x => x.RequestedByPersonId,
                        principalTable: "platform_users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                    table.ForeignKey(
                        name: "FK_ingestion_jobs_reference_datasets_DatasetId",
                        column: x => x.DatasetId,
                        principalTable: "reference_datasets",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_ingestion_jobs_reference_sources_SourceId",
                        column: x => x.SourceId,
                        principalTable: "reference_sources",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "product_mappings",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    TenantId = table.Column<Guid>(type: "uuid", nullable: false),
                    ProductCode = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: false),
                    ReferenceEntityId = table.Column<Guid>(type: "uuid", nullable: false),
                    LocalEntityType = table.Column<string>(type: "character varying(128)", maxLength: 128, nullable: false),
                    LocalEntityId = table.Column<string>(type: "character varying(128)", maxLength: 128, nullable: false),
                    MappingStatus = table.Column<string>(type: "character varying(32)", maxLength: 32, nullable: false),
                    CreatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_product_mappings", x => x.Id);
                    table.ForeignKey(
                        name: "FK_product_mappings_tenants_TenantId",
                        column: x => x.TenantId,
                        principalTable: "tenants",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "reference_crosswalks",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    ReferenceEntityId = table.Column<Guid>(type: "uuid", nullable: false),
                    ExternalSystem = table.Column<string>(type: "character varying(128)", maxLength: 128, nullable: false),
                    ExternalKey = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: false),
                    SourceId = table.Column<Guid>(type: "uuid", nullable: true),
                    Confidence = table.Column<decimal>(type: "numeric", nullable: false),
                    Status = table.Column<string>(type: "character varying(32)", maxLength: 32, nullable: false),
                    CreatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_reference_crosswalks", x => x.Id);
                    table.ForeignKey(
                        name: "FK_reference_crosswalks_reference_sources_SourceId",
                        column: x => x.SourceId,
                        principalTable: "reference_sources",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                });

            migrationBuilder.CreateTable(
                name: "reference_entities",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    DatasetId = table.Column<Guid>(type: "uuid", nullable: false),
                    EntityType = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: false),
                    CanonicalKey = table.Column<string>(type: "character varying(128)", maxLength: 128, nullable: false),
                    DisplayName = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: false),
                    Status = table.Column<string>(type: "character varying(32)", maxLength: 32, nullable: false),
                    NormalizedFieldsJson = table.Column<string>(type: "jsonb", nullable: false),
                    FirstSeenSourceId = table.Column<Guid>(type: "uuid", nullable: true),
                    CurrentVersionId = table.Column<Guid>(type: "uuid", nullable: true),
                    CreatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_reference_entities", x => x.Id);
                    table.ForeignKey(
                        name: "FK_reference_entities_reference_datasets_DatasetId",
                        column: x => x.DatasetId,
                        principalTable: "reference_datasets",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_reference_entities_reference_sources_FirstSeenSourceId",
                        column: x => x.FirstSeenSourceId,
                        principalTable: "reference_sources",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                });

            migrationBuilder.CreateTable(
                name: "reference_entity_versions",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    ReferenceEntityId = table.Column<Guid>(type: "uuid", nullable: false),
                    Version = table.Column<int>(type: "integer", nullable: false),
                    FieldsJson = table.Column<string>(type: "jsonb", nullable: false),
                    SourceEvidenceJson = table.Column<string>(type: "jsonb", nullable: false),
                    EffectiveDate = table.Column<DateOnly>(type: "date", nullable: true),
                    PublishedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    SupersededByVersionId = table.Column<Guid>(type: "uuid", nullable: true),
                    CreatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_reference_entity_versions", x => x.Id);
                    table.ForeignKey(
                        name: "FK_reference_entity_versions_reference_entities_ReferenceEntit~",
                        column: x => x.ReferenceEntityId,
                        principalTable: "reference_entities",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_reference_entity_versions_reference_entity_versions_Superse~",
                        column: x => x.SupersededByVersionId,
                        principalTable: "reference_entity_versions",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                });

            migrationBuilder.CreateTable(
                name: "staging_records",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    JobId = table.Column<Guid>(type: "uuid", nullable: false),
                    RowNumber = table.Column<int>(type: "integer", nullable: true),
                    RawPayloadJson = table.Column<string>(type: "jsonb", nullable: false),
                    NormalizedPayloadJson = table.Column<string>(type: "jsonb", nullable: false),
                    ProposedEntityType = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: false),
                    ProposedCanonicalKey = table.Column<string>(type: "character varying(128)", maxLength: 128, nullable: true),
                    Confidence = table.Column<decimal>(type: "numeric", nullable: false),
                    Status = table.Column<string>(type: "character varying(32)", maxLength: 32, nullable: false),
                    ReviewReason = table.Column<string>(type: "character varying(1024)", maxLength: 1024, nullable: true),
                    ReviewerPersonId = table.Column<Guid>(type: "uuid", nullable: true),
                    ReviewedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    ReferenceEntityId = table.Column<Guid>(type: "uuid", nullable: true),
                    CreatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_staging_records", x => x.Id);
                    table.ForeignKey(
                        name: "FK_staging_records_ingestion_jobs_JobId",
                        column: x => x.JobId,
                        principalTable: "ingestion_jobs",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_staging_records_platform_users_ReviewerPersonId",
                        column: x => x.ReviewerPersonId,
                        principalTable: "platform_users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                    table.ForeignKey(
                        name: "FK_staging_records_reference_entities_ReferenceEntityId",
                        column: x => x.ReferenceEntityId,
                        principalTable: "reference_entities",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                });

            migrationBuilder.CreateTable(
                name: "tenant_reference_overlays",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    TenantId = table.Column<Guid>(type: "uuid", nullable: false),
                    ReferenceEntityId = table.Column<Guid>(type: "uuid", nullable: false),
                    LocalName = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: true),
                    LocalStatus = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: true),
                    Hidden = table.Column<bool>(type: "boolean", nullable: false),
                    Notes = table.Column<string>(type: "character varying(1024)", maxLength: 1024, nullable: true),
                    CreatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_tenant_reference_overlays", x => x.Id);
                    table.ForeignKey(
                        name: "FK_tenant_reference_overlays_reference_entities_ReferenceEntit~",
                        column: x => x.ReferenceEntityId,
                        principalTable: "reference_entities",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_tenant_reference_overlays_tenants_TenantId",
                        column: x => x.TenantId,
                        principalTable: "tenants",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_ingestion_jobs_DatasetId_SourceId_CreatedAt",
                table: "ingestion_jobs",
                columns: new[] { "DatasetId", "SourceId", "CreatedAt" });

            migrationBuilder.CreateIndex(
                name: "IX_ingestion_jobs_RequestedByPersonId",
                table: "ingestion_jobs",
                column: "RequestedByPersonId");

            migrationBuilder.CreateIndex(
                name: "IX_ingestion_jobs_SourceId",
                table: "ingestion_jobs",
                column: "SourceId");

            migrationBuilder.CreateIndex(
                name: "IX_product_mappings_ReferenceEntityId",
                table: "product_mappings",
                column: "ReferenceEntityId");

            migrationBuilder.CreateIndex(
                name: "IX_product_mappings_TenantId_ProductCode_LocalEntityType_Local~",
                table: "product_mappings",
                columns: new[] { "TenantId", "ProductCode", "LocalEntityType", "LocalEntityId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_reference_audit_events_ActorPersonId",
                table: "reference_audit_events",
                column: "ActorPersonId");

            migrationBuilder.CreateIndex(
                name: "IX_reference_audit_events_EntityType_EntityId_CreatedAt",
                table: "reference_audit_events",
                columns: new[] { "EntityType", "EntityId", "CreatedAt" });

            migrationBuilder.CreateIndex(
                name: "IX_reference_crosswalks_ExternalSystem_ExternalKey",
                table: "reference_crosswalks",
                columns: new[] { "ExternalSystem", "ExternalKey" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_reference_crosswalks_ReferenceEntityId",
                table: "reference_crosswalks",
                column: "ReferenceEntityId");

            migrationBuilder.CreateIndex(
                name: "IX_reference_crosswalks_SourceId",
                table: "reference_crosswalks",
                column: "SourceId");

            migrationBuilder.CreateIndex(
                name: "IX_reference_datasets_Key",
                table: "reference_datasets",
                column: "Key",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_reference_entities_CurrentVersionId",
                table: "reference_entities",
                column: "CurrentVersionId");

            migrationBuilder.CreateIndex(
                name: "IX_reference_entities_DatasetId_EntityType_CanonicalKey",
                table: "reference_entities",
                columns: new[] { "DatasetId", "EntityType", "CanonicalKey" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_reference_entities_DatasetId_Status",
                table: "reference_entities",
                columns: new[] { "DatasetId", "Status" });

            migrationBuilder.CreateIndex(
                name: "IX_reference_entities_FirstSeenSourceId",
                table: "reference_entities",
                column: "FirstSeenSourceId");

            migrationBuilder.CreateIndex(
                name: "IX_reference_entity_versions_ReferenceEntityId_Version",
                table: "reference_entity_versions",
                columns: new[] { "ReferenceEntityId", "Version" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_reference_entity_versions_SupersededByVersionId",
                table: "reference_entity_versions",
                column: "SupersededByVersionId");

            migrationBuilder.CreateIndex(
                name: "IX_reference_publish_events_DatasetId_CreatedAt",
                table: "reference_publish_events",
                columns: new[] { "DatasetId", "CreatedAt" });

            migrationBuilder.CreateIndex(
                name: "IX_reference_publish_events_PublishedByPersonId",
                table: "reference_publish_events",
                column: "PublishedByPersonId");

            migrationBuilder.CreateIndex(
                name: "IX_reference_sources_Key",
                table: "reference_sources",
                column: "Key",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_staging_records_JobId",
                table: "staging_records",
                column: "JobId");

            migrationBuilder.CreateIndex(
                name: "IX_staging_records_ReferenceEntityId",
                table: "staging_records",
                column: "ReferenceEntityId");

            migrationBuilder.CreateIndex(
                name: "IX_staging_records_ReviewerPersonId",
                table: "staging_records",
                column: "ReviewerPersonId");

            migrationBuilder.CreateIndex(
                name: "IX_staging_records_Status_Confidence",
                table: "staging_records",
                columns: new[] { "Status", "Confidence" });

            migrationBuilder.CreateIndex(
                name: "IX_tenant_reference_overlays_ReferenceEntityId",
                table: "tenant_reference_overlays",
                column: "ReferenceEntityId");

            migrationBuilder.CreateIndex(
                name: "IX_tenant_reference_overlays_TenantId_ReferenceEntityId",
                table: "tenant_reference_overlays",
                columns: new[] { "TenantId", "ReferenceEntityId" },
                unique: true);

            migrationBuilder.AddForeignKey(
                name: "FK_product_mappings_reference_entities_ReferenceEntityId",
                table: "product_mappings",
                column: "ReferenceEntityId",
                principalTable: "reference_entities",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_reference_crosswalks_reference_entities_ReferenceEntityId",
                table: "reference_crosswalks",
                column: "ReferenceEntityId",
                principalTable: "reference_entities",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_reference_entities_reference_entity_versions_CurrentVersion~",
                table: "reference_entities",
                column: "CurrentVersionId",
                principalTable: "reference_entity_versions",
                principalColumn: "Id",
                onDelete: ReferentialAction.SetNull);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_reference_entities_reference_datasets_DatasetId",
                table: "reference_entities");

            migrationBuilder.DropForeignKey(
                name: "FK_reference_entities_reference_sources_FirstSeenSourceId",
                table: "reference_entities");

            migrationBuilder.DropForeignKey(
                name: "FK_reference_entity_versions_reference_entities_ReferenceEntit~",
                table: "reference_entity_versions");

            migrationBuilder.DropTable(
                name: "product_mappings");

            migrationBuilder.DropTable(
                name: "reference_audit_events");

            migrationBuilder.DropTable(
                name: "reference_crosswalks");

            migrationBuilder.DropTable(
                name: "reference_publish_events");

            migrationBuilder.DropTable(
                name: "staging_records");

            migrationBuilder.DropTable(
                name: "tenant_reference_overlays");

            migrationBuilder.DropTable(
                name: "ingestion_jobs");

            migrationBuilder.DropTable(
                name: "reference_datasets");

            migrationBuilder.DropTable(
                name: "reference_sources");

            migrationBuilder.DropTable(
                name: "reference_entities");

            migrationBuilder.DropTable(
                name: "reference_entity_versions");
        }
    }
}
