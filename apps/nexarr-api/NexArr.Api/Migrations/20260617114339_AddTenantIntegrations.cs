using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace NexArr.Api.Migrations
{
    /// <inheritdoc />
    public partial class AddTenantIntegrations : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "nexarr_tenant_integration_connections",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    TenantId = table.Column<Guid>(type: "uuid", nullable: false),
                    ProviderKey = table.Column<string>(type: "character varying(96)", maxLength: 96, nullable: false),
                    Status = table.Column<string>(type: "character varying(32)", maxLength: 32, nullable: false),
                    SyncDirection = table.Column<string>(type: "character varying(32)", maxLength: 32, nullable: false),
                    WritebacksEnabled = table.Column<bool>(type: "boolean", nullable: false),
                    ManualMappingRequired = table.Column<bool>(type: "boolean", nullable: false),
                    ConfigurationJson = table.Column<string>(type: "jsonb", nullable: false),
                    LastSuccessfulSyncAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    LastFailedSyncAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    LastErrorCategory = table.Column<string>(type: "character varying(128)", maxLength: 128, nullable: true),
                    LastErrorMessage = table.Column<string>(type: "character varying(1024)", maxLength: 1024, nullable: true),
                    CreatedByUserId = table.Column<Guid>(type: "uuid", nullable: false),
                    ModifiedByUserId = table.Column<Guid>(type: "uuid", nullable: false),
                    CreatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_nexarr_tenant_integration_connections", x => x.Id);
                    table.ForeignKey(
                        name: "FK_nexarr_tenant_integration_connections_tenants_TenantId",
                        column: x => x.TenantId,
                        principalTable: "tenants",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "nexarr_tenant_integration_intake_attempts",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    TenantId = table.Column<Guid>(type: "uuid", nullable: true),
                    ConnectionId = table.Column<Guid>(type: "uuid", nullable: true),
                    ProviderKey = table.Column<string>(type: "character varying(96)", maxLength: 96, nullable: false),
                    IntakeKind = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: false),
                    IdempotencyKey = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: false),
                    Status = table.Column<string>(type: "character varying(32)", maxLength: 32, nullable: false),
                    SourceRoute = table.Column<string>(type: "character varying(512)", maxLength: 512, nullable: false),
                    PayloadHash = table.Column<string>(type: "character varying(128)", maxLength: 128, nullable: false),
                    FileName = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: true),
                    ContentType = table.Column<string>(type: "character varying(128)", maxLength: 128, nullable: true),
                    ReceivedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    ProcessedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    ErrorCategory = table.Column<string>(type: "character varying(128)", maxLength: 128, nullable: true),
                    ErrorMessage = table.Column<string>(type: "character varying(1024)", maxLength: 1024, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_nexarr_tenant_integration_intake_attempts", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "nexarr_tenant_integration_credentials",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    TenantId = table.Column<Guid>(type: "uuid", nullable: false),
                    ConnectionId = table.Column<Guid>(type: "uuid", nullable: false),
                    ProviderKey = table.Column<string>(type: "character varying(96)", maxLength: 96, nullable: false),
                    CredentialKind = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: false),
                    EncryptedPayload = table.Column<string>(type: "character varying(16000)", maxLength: 16000, nullable: false),
                    EncryptionKeyId = table.Column<string>(type: "character varying(128)", maxLength: 128, nullable: false),
                    RedactedLabel = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: false),
                    ExpiresAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    LastValidatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    CreatedByUserId = table.Column<Guid>(type: "uuid", nullable: false),
                    ModifiedByUserId = table.Column<Guid>(type: "uuid", nullable: false),
                    CreatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_nexarr_tenant_integration_credentials", x => x.Id);
                    table.ForeignKey(
                        name: "FK_nexarr_tenant_integration_credentials_nexarr_tenant_integra~",
                        column: x => x.ConnectionId,
                        principalTable: "nexarr_tenant_integration_connections",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "nexarr_tenant_integration_external_mappings",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    TenantId = table.Column<Guid>(type: "uuid", nullable: false),
                    ConnectionId = table.Column<Guid>(type: "uuid", nullable: false),
                    ProviderKey = table.Column<string>(type: "character varying(96)", maxLength: 96, nullable: false),
                    OwningProductKey = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: false),
                    StlEntityType = table.Column<string>(type: "character varying(128)", maxLength: 128, nullable: false),
                    StlEntityId = table.Column<string>(type: "character varying(128)", maxLength: 128, nullable: false),
                    ExternalEntityType = table.Column<string>(type: "character varying(128)", maxLength: 128, nullable: false),
                    ExternalId = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: false),
                    MappingStatus = table.Column<string>(type: "character varying(32)", maxLength: 32, nullable: false),
                    SyncDirection = table.Column<string>(type: "character varying(32)", maxLength: 32, nullable: false),
                    LastVerifiedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    LastSyncAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    LastError = table.Column<string>(type: "character varying(1024)", maxLength: 1024, nullable: true),
                    CreatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_nexarr_tenant_integration_external_mappings", x => x.Id);
                    table.ForeignKey(
                        name: "FK_nexarr_tenant_integration_external_mappings_nexarr_tenant_i~",
                        column: x => x.ConnectionId,
                        principalTable: "nexarr_tenant_integration_connections",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "nexarr_tenant_integration_mapping_templates",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    TenantId = table.Column<Guid>(type: "uuid", nullable: false),
                    ConnectionId = table.Column<Guid>(type: "uuid", nullable: false),
                    ProviderKey = table.Column<string>(type: "character varying(96)", maxLength: 96, nullable: false),
                    TemplateName = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: false),
                    SourceEntityType = table.Column<string>(type: "character varying(128)", maxLength: 128, nullable: false),
                    TargetProductKey = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: false),
                    TargetEntityType = table.Column<string>(type: "character varying(128)", maxLength: 128, nullable: false),
                    MappingJson = table.Column<string>(type: "jsonb", nullable: false),
                    IsActive = table.Column<bool>(type: "boolean", nullable: false),
                    CreatedByUserId = table.Column<Guid>(type: "uuid", nullable: false),
                    ModifiedByUserId = table.Column<Guid>(type: "uuid", nullable: false),
                    CreatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_nexarr_tenant_integration_mapping_templates", x => x.Id);
                    table.ForeignKey(
                        name: "FK_nexarr_tenant_integration_mapping_templates_nexarr_tenant_i~",
                        column: x => x.ConnectionId,
                        principalTable: "nexarr_tenant_integration_connections",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "nexarr_tenant_integration_provider_health",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    TenantId = table.Column<Guid>(type: "uuid", nullable: false),
                    ConnectionId = table.Column<Guid>(type: "uuid", nullable: false),
                    ProviderKey = table.Column<string>(type: "character varying(96)", maxLength: 96, nullable: false),
                    Status = table.Column<string>(type: "character varying(32)", maxLength: 32, nullable: false),
                    CheckedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    LatencyMs = table.Column<double>(type: "double precision", nullable: true),
                    ErrorCategory = table.Column<string>(type: "character varying(128)", maxLength: 128, nullable: true),
                    ErrorMessage = table.Column<string>(type: "character varying(1024)", maxLength: 1024, nullable: true),
                    MetadataJson = table.Column<string>(type: "jsonb", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_nexarr_tenant_integration_provider_health", x => x.Id);
                    table.ForeignKey(
                        name: "FK_nexarr_tenant_integration_provider_health_nexarr_tenant_int~",
                        column: x => x.ConnectionId,
                        principalTable: "nexarr_tenant_integration_connections",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "nexarr_tenant_integration_sync_runs",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    TenantId = table.Column<Guid>(type: "uuid", nullable: false),
                    ConnectionId = table.Column<Guid>(type: "uuid", nullable: false),
                    ProviderKey = table.Column<string>(type: "character varying(96)", maxLength: 96, nullable: false),
                    Status = table.Column<string>(type: "character varying(32)", maxLength: 32, nullable: false),
                    Direction = table.Column<string>(type: "character varying(32)", maxLength: 32, nullable: false),
                    TriggeredBy = table.Column<string>(type: "character varying(32)", maxLength: 32, nullable: false),
                    TriggeredByUserId = table.Column<Guid>(type: "uuid", nullable: true),
                    IdempotencyKey = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: false),
                    AttemptCount = table.Column<int>(type: "integer", nullable: false),
                    StartedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    CompletedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    NextRetryAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    SnapshotCount = table.Column<int>(type: "integer", nullable: false),
                    MappingCount = table.Column<int>(type: "integer", nullable: false),
                    ErrorCategory = table.Column<string>(type: "character varying(128)", maxLength: 128, nullable: true),
                    ErrorMessage = table.Column<string>(type: "character varying(1024)", maxLength: 1024, nullable: true),
                    DestinationProductsJson = table.Column<string>(type: "jsonb", nullable: false),
                    ResultSummaryJson = table.Column<string>(type: "jsonb", nullable: false),
                    CreatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_nexarr_tenant_integration_sync_runs", x => x.Id);
                    table.ForeignKey(
                        name: "FK_nexarr_tenant_integration_sync_runs_nexarr_tenant_integrati~",
                        column: x => x.ConnectionId,
                        principalTable: "nexarr_tenant_integration_connections",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_nexarr_tenant_integration_connections_Status_UpdatedAt",
                table: "nexarr_tenant_integration_connections",
                columns: new[] { "Status", "UpdatedAt" });

            migrationBuilder.CreateIndex(
                name: "IX_nexarr_tenant_integration_connections_TenantId_ProviderKey",
                table: "nexarr_tenant_integration_connections",
                columns: new[] { "TenantId", "ProviderKey" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_nexarr_tenant_integration_credentials_ConnectionId",
                table: "nexarr_tenant_integration_credentials",
                column: "ConnectionId");

            migrationBuilder.CreateIndex(
                name: "IX_nexarr_tenant_integration_credentials_TenantId_ProviderKey",
                table: "nexarr_tenant_integration_credentials",
                columns: new[] { "TenantId", "ProviderKey" });

            migrationBuilder.CreateIndex(
                name: "IX_nexarr_tenant_integration_external_mappings_ConnectionId",
                table: "nexarr_tenant_integration_external_mappings",
                column: "ConnectionId");

            migrationBuilder.CreateIndex(
                name: "IX_nexarr_tenant_integration_external_mappings_TenantId_Owning~",
                table: "nexarr_tenant_integration_external_mappings",
                columns: new[] { "TenantId", "OwningProductKey", "StlEntityType", "StlEntityId" });

            migrationBuilder.CreateIndex(
                name: "IX_nexarr_tenant_integration_external_mappings_TenantId_Provid~",
                table: "nexarr_tenant_integration_external_mappings",
                columns: new[] { "TenantId", "ProviderKey", "ExternalEntityType", "ExternalId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_nexarr_tenant_integration_intake_attempts_ProviderKey_Idemp~",
                table: "nexarr_tenant_integration_intake_attempts",
                columns: new[] { "ProviderKey", "IdempotencyKey" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_nexarr_tenant_integration_intake_attempts_TenantId_Provider~",
                table: "nexarr_tenant_integration_intake_attempts",
                columns: new[] { "TenantId", "ProviderKey", "ReceivedAt" });

            migrationBuilder.CreateIndex(
                name: "IX_nexarr_tenant_integration_mapping_templates_ConnectionId",
                table: "nexarr_tenant_integration_mapping_templates",
                column: "ConnectionId");

            migrationBuilder.CreateIndex(
                name: "IX_nexarr_tenant_integration_mapping_templates_TenantId_Provi~1",
                table: "nexarr_tenant_integration_mapping_templates",
                columns: new[] { "TenantId", "ProviderKey", "TemplateName" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_nexarr_tenant_integration_mapping_templates_TenantId_Provid~",
                table: "nexarr_tenant_integration_mapping_templates",
                columns: new[] { "TenantId", "ProviderKey", "TargetProductKey" });

            migrationBuilder.CreateIndex(
                name: "IX_nexarr_tenant_integration_provider_health_ConnectionId",
                table: "nexarr_tenant_integration_provider_health",
                column: "ConnectionId");

            migrationBuilder.CreateIndex(
                name: "IX_nexarr_tenant_integration_provider_health_TenantId_Provider~",
                table: "nexarr_tenant_integration_provider_health",
                columns: new[] { "TenantId", "ProviderKey", "CheckedAt" });

            migrationBuilder.CreateIndex(
                name: "IX_nexarr_tenant_integration_sync_runs_ConnectionId",
                table: "nexarr_tenant_integration_sync_runs",
                column: "ConnectionId");

            migrationBuilder.CreateIndex(
                name: "IX_nexarr_tenant_integration_sync_runs_Status_NextRetryAt_Star~",
                table: "nexarr_tenant_integration_sync_runs",
                columns: new[] { "Status", "NextRetryAt", "StartedAt" });

            migrationBuilder.CreateIndex(
                name: "IX_nexarr_tenant_integration_sync_runs_TenantId_ProviderKey_Id~",
                table: "nexarr_tenant_integration_sync_runs",
                columns: new[] { "TenantId", "ProviderKey", "IdempotencyKey" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_nexarr_tenant_integration_sync_runs_TenantId_ProviderKey_St~",
                table: "nexarr_tenant_integration_sync_runs",
                columns: new[] { "TenantId", "ProviderKey", "StartedAt" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "nexarr_tenant_integration_credentials");

            migrationBuilder.DropTable(
                name: "nexarr_tenant_integration_external_mappings");

            migrationBuilder.DropTable(
                name: "nexarr_tenant_integration_intake_attempts");

            migrationBuilder.DropTable(
                name: "nexarr_tenant_integration_mapping_templates");

            migrationBuilder.DropTable(
                name: "nexarr_tenant_integration_provider_health");

            migrationBuilder.DropTable(
                name: "nexarr_tenant_integration_sync_runs");

            migrationBuilder.DropTable(
                name: "nexarr_tenant_integration_connections");
        }
    }
}
