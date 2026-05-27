using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ComplianceCore.Api.Migrations
{
    /// <inheritdoc />
    public partial class ComplianceCoreVocabularySpine : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "compliancecore_audit_events",
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
                    table.PrimaryKey("PK_compliancecore_audit_events", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "compliancecore_compliance_keys",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    TenantId = table.Column<Guid>(type: "uuid", nullable: false),
                    Key = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: false),
                    Label = table.Column<string>(type: "character varying(128)", maxLength: 128, nullable: false),
                    Category = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: false),
                    Description = table.Column<string>(type: "character varying(1024)", maxLength: 1024, nullable: false),
                    IsActive = table.Column<bool>(type: "boolean", nullable: false),
                    CreatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_compliancecore_compliance_keys", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "compliancecore_material_keys",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    TenantId = table.Column<Guid>(type: "uuid", nullable: false),
                    Key = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: false),
                    Label = table.Column<string>(type: "character varying(128)", maxLength: 128, nullable: false),
                    Category = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: false),
                    Description = table.Column<string>(type: "character varying(1024)", maxLength: 1024, nullable: false),
                    IsActive = table.Column<bool>(type: "boolean", nullable: false),
                    CreatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_compliancecore_material_keys", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "compliancecore_vocabulary_terms",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    TenantId = table.Column<Guid>(type: "uuid", nullable: false),
                    TermKey = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: false),
                    Label = table.Column<string>(type: "character varying(128)", maxLength: 128, nullable: false),
                    VocabularyTypeKey = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: false),
                    Description = table.Column<string>(type: "character varying(1024)", maxLength: 1024, nullable: false),
                    IsActive = table.Column<bool>(type: "boolean", nullable: false),
                    CreatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_compliancecore_vocabulary_terms", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "compliancecore_vocabulary_types",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    TypeKey = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: false),
                    Label = table.Column<string>(type: "character varying(128)", maxLength: 128, nullable: false),
                    Description = table.Column<string>(type: "character varying(1024)", maxLength: 1024, nullable: false),
                    SortOrder = table.Column<int>(type: "integer", nullable: false),
                    IsActive = table.Column<bool>(type: "boolean", nullable: false),
                    CreatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_compliancecore_vocabulary_types", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "compliancecore_vocabulary_aliases",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    TenantId = table.Column<Guid>(type: "uuid", nullable: false),
                    VocabularyTermId = table.Column<Guid>(type: "uuid", nullable: false),
                    AliasText = table.Column<string>(type: "character varying(128)", maxLength: 128, nullable: false),
                    IsActive = table.Column<bool>(type: "boolean", nullable: false),
                    CreatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_compliancecore_vocabulary_aliases", x => x.Id);
                    table.ForeignKey(
                        name: "FK_compliancecore_vocabulary_aliases_compliancecore_vocabulary~",
                        column: x => x.VocabularyTermId,
                        principalTable: "compliancecore_vocabulary_terms",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_compliancecore_audit_events_OccurredAt",
                table: "compliancecore_audit_events",
                column: "OccurredAt");

            migrationBuilder.CreateIndex(
                name: "IX_compliancecore_audit_events_TenantId",
                table: "compliancecore_audit_events",
                column: "TenantId");

            migrationBuilder.CreateIndex(
                name: "IX_compliancecore_compliance_keys_TenantId",
                table: "compliancecore_compliance_keys",
                column: "TenantId");

            migrationBuilder.CreateIndex(
                name: "IX_compliancecore_compliance_keys_TenantId_Key",
                table: "compliancecore_compliance_keys",
                columns: new[] { "TenantId", "Key" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_compliancecore_material_keys_TenantId",
                table: "compliancecore_material_keys",
                column: "TenantId");

            migrationBuilder.CreateIndex(
                name: "IX_compliancecore_material_keys_TenantId_Key",
                table: "compliancecore_material_keys",
                columns: new[] { "TenantId", "Key" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_compliancecore_vocabulary_aliases_TenantId",
                table: "compliancecore_vocabulary_aliases",
                column: "TenantId");

            migrationBuilder.CreateIndex(
                name: "IX_compliancecore_vocabulary_aliases_TenantId_VocabularyTermId~",
                table: "compliancecore_vocabulary_aliases",
                columns: new[] { "TenantId", "VocabularyTermId", "AliasText" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_compliancecore_vocabulary_aliases_VocabularyTermId",
                table: "compliancecore_vocabulary_aliases",
                column: "VocabularyTermId");

            migrationBuilder.CreateIndex(
                name: "IX_compliancecore_vocabulary_terms_TenantId",
                table: "compliancecore_vocabulary_terms",
                column: "TenantId");

            migrationBuilder.CreateIndex(
                name: "IX_compliancecore_vocabulary_terms_TenantId_TermKey",
                table: "compliancecore_vocabulary_terms",
                columns: new[] { "TenantId", "TermKey" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_compliancecore_vocabulary_terms_TenantId_VocabularyTypeKey",
                table: "compliancecore_vocabulary_terms",
                columns: new[] { "TenantId", "VocabularyTypeKey" });

            migrationBuilder.CreateIndex(
                name: "IX_compliancecore_vocabulary_types_SortOrder",
                table: "compliancecore_vocabulary_types",
                column: "SortOrder");

            migrationBuilder.CreateIndex(
                name: "IX_compliancecore_vocabulary_types_TypeKey",
                table: "compliancecore_vocabulary_types",
                column: "TypeKey",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "compliancecore_audit_events");

            migrationBuilder.DropTable(
                name: "compliancecore_compliance_keys");

            migrationBuilder.DropTable(
                name: "compliancecore_material_keys");

            migrationBuilder.DropTable(
                name: "compliancecore_vocabulary_aliases");

            migrationBuilder.DropTable(
                name: "compliancecore_vocabulary_types");

            migrationBuilder.DropTable(
                name: "compliancecore_vocabulary_terms");
        }
    }
}
