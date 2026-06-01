using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace MaintainArr.Api.Migrations
{
    /// <inheritdoc />
    public partial class MaintainArrControlledFieldsetsCutover : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "maintainarr_asset_assignment_history",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    TenantId = table.Column<Guid>(type: "uuid", nullable: false),
                    AssetId = table.Column<Guid>(type: "uuid", nullable: false),
                    AssignmentFieldKey = table.Column<string>(type: "character varying(128)", maxLength: 128, nullable: false),
                    PersonId = table.Column<string>(type: "character varying(128)", maxLength: 128, nullable: false),
                    ChangedByPersonId = table.Column<string>(type: "character varying(128)", maxLength: 128, nullable: true),
                    EffectiveAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    CreatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_maintainarr_asset_assignment_history", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "maintainarr_asset_compliance_state",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    TenantId = table.Column<Guid>(type: "uuid", nullable: false),
                    AssetId = table.Column<Guid>(type: "uuid", nullable: false),
                    GoverningBodyKeysJson = table.Column<string>(type: "text", nullable: false),
                    RulepackApplicabilityKeysJson = table.Column<string>(type: "text", nullable: false),
                    ComplianceCategoryKeysJson = table.Column<string>(type: "text", nullable: false),
                    UpdatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_maintainarr_asset_compliance_state", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "maintainarr_asset_components",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    TenantId = table.Column<Guid>(type: "uuid", nullable: false),
                    AssetId = table.Column<Guid>(type: "uuid", nullable: false),
                    ComponentKey = table.Column<string>(type: "character varying(128)", maxLength: 128, nullable: false),
                    ValueJson = table.Column<string>(type: "text", nullable: false),
                    CreatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_maintainarr_asset_components", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "maintainarr_asset_custom_field_values",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    TenantId = table.Column<Guid>(type: "uuid", nullable: false),
                    AssetId = table.Column<Guid>(type: "uuid", nullable: false),
                    FieldKey = table.Column<string>(type: "character varying(128)", maxLength: 128, nullable: false),
                    ValueJson = table.Column<string>(type: "text", nullable: false),
                    CreatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_maintainarr_asset_custom_field_values", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "maintainarr_asset_documents",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    TenantId = table.Column<Guid>(type: "uuid", nullable: false),
                    AssetId = table.Column<Guid>(type: "uuid", nullable: false),
                    DocumentTypeKey = table.Column<string>(type: "character varying(128)", maxLength: 128, nullable: false),
                    ExternalDocumentId = table.Column<string>(type: "character varying(128)", maxLength: 128, nullable: false),
                    StatusKey = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: false),
                    CreatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_maintainarr_asset_documents", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "maintainarr_asset_external_mappings",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    TenantId = table.Column<Guid>(type: "uuid", nullable: false),
                    AssetId = table.Column<Guid>(type: "uuid", nullable: false),
                    SourceSystem = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: false),
                    ExternalEntityType = table.Column<string>(type: "character varying(128)", maxLength: 128, nullable: false),
                    ExternalId = table.Column<string>(type: "character varying(128)", maxLength: 128, nullable: false),
                    ExternalKey = table.Column<string>(type: "character varying(128)", maxLength: 128, nullable: true),
                    MetadataJson = table.Column<string>(type: "character varying(4096)", maxLength: 4096, nullable: true),
                    CreatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_maintainarr_asset_external_mappings", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "maintainarr_asset_location_history",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    TenantId = table.Column<Guid>(type: "uuid", nullable: false),
                    AssetId = table.Column<Guid>(type: "uuid", nullable: false),
                    SiteId = table.Column<string>(type: "character varying(128)", maxLength: 128, nullable: true),
                    HomeLocationId = table.Column<string>(type: "character varying(128)", maxLength: 128, nullable: true),
                    CurrentLocationId = table.Column<string>(type: "character varying(128)", maxLength: 128, nullable: true),
                    Yard = table.Column<string>(type: "character varying(128)", maxLength: 128, nullable: true),
                    Bay = table.Column<string>(type: "character varying(128)", maxLength: 128, nullable: true),
                    ParkingSpot = table.Column<string>(type: "character varying(128)", maxLength: 128, nullable: true),
                    ChangedByPersonId = table.Column<string>(type: "character varying(128)", maxLength: 128, nullable: true),
                    EffectiveAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    CreatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_maintainarr_asset_location_history", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "maintainarr_asset_readiness_state",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    TenantId = table.Column<Guid>(type: "uuid", nullable: false),
                    AssetId = table.Column<Guid>(type: "uuid", nullable: false),
                    ReadinessStatusKey = table.Column<string>(type: "character varying(128)", maxLength: 128, nullable: false),
                    OperationalStatusKey = table.Column<string>(type: "character varying(128)", maxLength: 128, nullable: false),
                    AvailabilityStatusKey = table.Column<string>(type: "character varying(128)", maxLength: 128, nullable: false),
                    Basis = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: true),
                    Notes = table.Column<string>(type: "character varying(1024)", maxLength: 1024, nullable: true),
                    UpdatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_maintainarr_asset_readiness_state", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "maintainarr_asset_specs",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    TenantId = table.Column<Guid>(type: "uuid", nullable: false),
                    AssetId = table.Column<Guid>(type: "uuid", nullable: false),
                    SpecKey = table.Column<string>(type: "character varying(128)", maxLength: 128, nullable: false),
                    ValueJson = table.Column<string>(type: "text", nullable: false),
                    CreatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_maintainarr_asset_specs", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "maintainarr_asset_status_history",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    TenantId = table.Column<Guid>(type: "uuid", nullable: false),
                    AssetId = table.Column<Guid>(type: "uuid", nullable: false),
                    StatusFieldKey = table.Column<string>(type: "character varying(128)", maxLength: 128, nullable: false),
                    StatusValueKey = table.Column<string>(type: "character varying(128)", maxLength: 128, nullable: false),
                    ChangedByPersonId = table.Column<string>(type: "character varying(128)", maxLength: 128, nullable: true),
                    Notes = table.Column<string>(type: "character varying(1024)", maxLength: 1024, nullable: true),
                    ChangedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    CreatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_maintainarr_asset_status_history", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "maintainarr_catalog_option_dependencies",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    TenantId = table.Column<Guid>(type: "uuid", nullable: false),
                    CatalogOptionId = table.Column<Guid>(type: "uuid", nullable: false),
                    DependsOnCatalogKey = table.Column<string>(type: "character varying(128)", maxLength: 128, nullable: false),
                    DependsOnOptionKey = table.Column<string>(type: "character varying(128)", maxLength: 128, nullable: false),
                    RuleJson = table.Column<string>(type: "text", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_maintainarr_catalog_option_dependencies", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "maintainarr_catalog_options",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    TenantId = table.Column<Guid>(type: "uuid", nullable: false),
                    CatalogId = table.Column<Guid>(type: "uuid", nullable: false),
                    Key = table.Column<string>(type: "character varying(128)", maxLength: 128, nullable: false),
                    Label = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: false),
                    Description = table.Column<string>(type: "text", nullable: false),
                    SortOrder = table.Column<int>(type: "integer", nullable: false),
                    ParentOptionId = table.Column<Guid>(type: "uuid", nullable: true),
                    MetadataJson = table.Column<string>(type: "text", nullable: false),
                    IsSystem = table.Column<bool>(type: "boolean", nullable: false),
                    IsTenantSpecific = table.Column<bool>(type: "boolean", nullable: false),
                    OptionTenantId = table.Column<Guid>(type: "uuid", nullable: true),
                    IsActive = table.Column<bool>(type: "boolean", nullable: false),
                    CreatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_maintainarr_catalog_options", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "maintainarr_catalogs",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    TenantId = table.Column<Guid>(type: "uuid", nullable: false),
                    Key = table.Column<string>(type: "character varying(128)", maxLength: 128, nullable: false),
                    Label = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: false),
                    Description = table.Column<string>(type: "text", nullable: false),
                    Owner = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: false),
                    Scope = table.Column<string>(type: "character varying(32)", maxLength: 32, nullable: false),
                    IsSystem = table.Column<bool>(type: "boolean", nullable: false),
                    IsTenantExtendable = table.Column<bool>(type: "boolean", nullable: false),
                    IsActive = table.Column<bool>(type: "boolean", nullable: false),
                    CreatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_maintainarr_catalogs", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "maintainarr_fieldset_definitions",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    TenantId = table.Column<Guid>(type: "uuid", nullable: false),
                    Key = table.Column<string>(type: "character varying(128)", maxLength: 128, nullable: false),
                    Label = table.Column<string>(type: "text", nullable: false),
                    EntityType = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: false),
                    Purpose = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: false),
                    Description = table.Column<string>(type: "text", nullable: false),
                    IsActive = table.Column<bool>(type: "boolean", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_maintainarr_fieldset_definitions", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "maintainarr_fieldset_fields",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    TenantId = table.Column<Guid>(type: "uuid", nullable: false),
                    FieldsetId = table.Column<Guid>(type: "uuid", nullable: false),
                    Key = table.Column<string>(type: "character varying(128)", maxLength: 128, nullable: false),
                    Label = table.Column<string>(type: "text", nullable: false),
                    Description = table.Column<string>(type: "text", nullable: false),
                    DataType = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: false),
                    ControlType = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: false),
                    Required = table.Column<bool>(type: "boolean", nullable: false),
                    CatalogKey = table.Column<string>(type: "text", nullable: true),
                    ReferenceKey = table.Column<string>(type: "text", nullable: true),
                    SourceType = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: false),
                    SourceOfTruth = table.Column<string>(type: "character varying(128)", maxLength: 128, nullable: false),
                    SortOrder = table.Column<int>(type: "integer", nullable: false),
                    SectionKey = table.Column<string>(type: "text", nullable: false),
                    DependencyJson = table.Column<string>(type: "text", nullable: false),
                    ValidationJson = table.Column<string>(type: "text", nullable: false),
                    DefaultValueJson = table.Column<string>(type: "text", nullable: false),
                    VisibilityJson = table.Column<string>(type: "text", nullable: false),
                    AllowCustom = table.Column<bool>(type: "boolean", nullable: false),
                    CustomRequiresApproval = table.Column<bool>(type: "boolean", nullable: false),
                    DrivesLogic = table.Column<bool>(type: "boolean", nullable: false),
                    DrivesInspectionBranching = table.Column<bool>(type: "boolean", nullable: false),
                    DrivesPMApplicability = table.Column<bool>(type: "boolean", nullable: false),
                    DrivesCompliance = table.Column<bool>(type: "boolean", nullable: false),
                    DrivesReporting = table.Column<bool>(type: "boolean", nullable: false),
                    DrivesReadiness = table.Column<bool>(type: "boolean", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_maintainarr_fieldset_fields", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "maintainarr_pending_catalog_values",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    TenantId = table.Column<Guid>(type: "uuid", nullable: false),
                    CatalogKey = table.Column<string>(type: "character varying(128)", maxLength: 128, nullable: false),
                    ProposedKey = table.Column<string>(type: "character varying(128)", maxLength: 128, nullable: false),
                    ProposedLabel = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: false),
                    ProposedByPersonId = table.Column<string>(type: "character varying(128)", maxLength: 128, nullable: false),
                    SourceEntityType = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: false),
                    SourceEntityId = table.Column<string>(type: "character varying(128)", maxLength: 128, nullable: false),
                    Status = table.Column<string>(type: "character varying(32)", maxLength: 32, nullable: false),
                    ReviewedByPersonId = table.Column<string>(type: "text", nullable: true),
                    ReviewedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    CreatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_maintainarr_pending_catalog_values", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "maintainarr_reference_cache_entries",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    TenantId = table.Column<Guid>(type: "uuid", nullable: false),
                    SourceOfTruth = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: false),
                    ReferenceKey = table.Column<string>(type: "character varying(128)", maxLength: 128, nullable: false),
                    ExternalKey = table.Column<string>(type: "character varying(128)", maxLength: 128, nullable: false),
                    ExternalId = table.Column<string>(type: "character varying(128)", maxLength: 128, nullable: true),
                    Label = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: false),
                    Description = table.Column<string>(type: "text", nullable: true),
                    MetadataJson = table.Column<string>(type: "text", nullable: false),
                    IsActive = table.Column<bool>(type: "boolean", nullable: false),
                    LastSyncedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    CreatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_maintainarr_reference_cache_entries", x => x.Id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_maintainarr_asset_assignment_history_TenantId_AssetId_Assig~",
                table: "maintainarr_asset_assignment_history",
                columns: new[] { "TenantId", "AssetId", "AssignmentFieldKey", "EffectiveAt" });

            migrationBuilder.CreateIndex(
                name: "IX_maintainarr_asset_compliance_state_TenantId_AssetId",
                table: "maintainarr_asset_compliance_state",
                columns: new[] { "TenantId", "AssetId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_maintainarr_asset_components_TenantId_AssetId_ComponentKey",
                table: "maintainarr_asset_components",
                columns: new[] { "TenantId", "AssetId", "ComponentKey" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_maintainarr_asset_custom_field_values_TenantId_AssetId_Fiel~",
                table: "maintainarr_asset_custom_field_values",
                columns: new[] { "TenantId", "AssetId", "FieldKey" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_maintainarr_asset_external_mappings_TenantId_AssetId",
                table: "maintainarr_asset_external_mappings",
                columns: new[] { "TenantId", "AssetId" });

            migrationBuilder.CreateIndex(
                name: "IX_maintainarr_asset_external_mappings_TenantId_SourceSystem_E~",
                table: "maintainarr_asset_external_mappings",
                columns: new[] { "TenantId", "SourceSystem", "ExternalEntityType", "ExternalId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_maintainarr_asset_location_history_TenantId_AssetId_Effecti~",
                table: "maintainarr_asset_location_history",
                columns: new[] { "TenantId", "AssetId", "EffectiveAt" });

            migrationBuilder.CreateIndex(
                name: "IX_maintainarr_asset_readiness_state_TenantId_AssetId",
                table: "maintainarr_asset_readiness_state",
                columns: new[] { "TenantId", "AssetId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_maintainarr_asset_specs_TenantId_AssetId_SpecKey",
                table: "maintainarr_asset_specs",
                columns: new[] { "TenantId", "AssetId", "SpecKey" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_maintainarr_asset_status_history_TenantId_AssetId_ChangedAt",
                table: "maintainarr_asset_status_history",
                columns: new[] { "TenantId", "AssetId", "ChangedAt" });

            migrationBuilder.CreateIndex(
                name: "IX_maintainarr_catalog_options_TenantId_CatalogId_Key_OptionTe~",
                table: "maintainarr_catalog_options",
                columns: new[] { "TenantId", "CatalogId", "Key", "OptionTenantId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_maintainarr_catalogs_TenantId_Scope_Key",
                table: "maintainarr_catalogs",
                columns: new[] { "TenantId", "Scope", "Key" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_maintainarr_fieldset_definitions_TenantId_Key_Purpose",
                table: "maintainarr_fieldset_definitions",
                columns: new[] { "TenantId", "Key", "Purpose" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_maintainarr_fieldset_fields_TenantId_FieldsetId_Key",
                table: "maintainarr_fieldset_fields",
                columns: new[] { "TenantId", "FieldsetId", "Key" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_maintainarr_reference_cache_entries_TenantId_SourceOfTruth_~",
                table: "maintainarr_reference_cache_entries",
                columns: new[] { "TenantId", "SourceOfTruth", "ReferenceKey", "ExternalKey" },
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "maintainarr_asset_assignment_history");

            migrationBuilder.DropTable(
                name: "maintainarr_asset_compliance_state");

            migrationBuilder.DropTable(
                name: "maintainarr_asset_components");

            migrationBuilder.DropTable(
                name: "maintainarr_asset_custom_field_values");

            migrationBuilder.DropTable(
                name: "maintainarr_asset_documents");

            migrationBuilder.DropTable(
                name: "maintainarr_asset_external_mappings");

            migrationBuilder.DropTable(
                name: "maintainarr_asset_location_history");

            migrationBuilder.DropTable(
                name: "maintainarr_asset_readiness_state");

            migrationBuilder.DropTable(
                name: "maintainarr_asset_specs");

            migrationBuilder.DropTable(
                name: "maintainarr_asset_status_history");

            migrationBuilder.DropTable(
                name: "maintainarr_catalog_option_dependencies");

            migrationBuilder.DropTable(
                name: "maintainarr_catalog_options");

            migrationBuilder.DropTable(
                name: "maintainarr_catalogs");

            migrationBuilder.DropTable(
                name: "maintainarr_fieldset_definitions");

            migrationBuilder.DropTable(
                name: "maintainarr_fieldset_fields");

            migrationBuilder.DropTable(
                name: "maintainarr_pending_catalog_values");

            migrationBuilder.DropTable(
                name: "maintainarr_reference_cache_entries");
        }
    }
}
