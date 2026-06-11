using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace NexArr.Api.Migrations
{
    /// <inheritdoc />
    public partial class AddAiAssistanceSmartImport : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "nexarr_ai_action_proposals",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    SessionId = table.Column<Guid>(type: "uuid", nullable: false),
                    TenantId = table.Column<Guid>(type: "uuid", nullable: false),
                    ActorPersonId = table.Column<Guid>(type: "uuid", nullable: false),
                    ProductKey = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: false),
                    ActionCategory = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: false),
                    Status = table.Column<string>(type: "character varying(32)", maxLength: 32, nullable: false),
                    ProposalJson = table.Column<string>(type: "jsonb", nullable: false),
                    RequiredPermissionsJson = table.Column<string>(type: "jsonb", nullable: false),
                    ReviewReasonsJson = table.Column<string>(type: "jsonb", nullable: false),
                    CreatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    ConfirmedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_nexarr_ai_action_proposals", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "nexarr_ai_audit_events",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    TenantId = table.Column<Guid>(type: "uuid", nullable: false),
                    ActorPersonId = table.Column<Guid>(type: "uuid", nullable: true),
                    EventType = table.Column<string>(type: "character varying(128)", maxLength: 128, nullable: false),
                    TargetType = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: false),
                    TargetId = table.Column<string>(type: "character varying(128)", maxLength: 128, nullable: true),
                    Result = table.Column<string>(type: "character varying(32)", maxLength: 32, nullable: false),
                    ReasonCode = table.Column<string>(type: "character varying(128)", maxLength: 128, nullable: true),
                    MetadataJson = table.Column<string>(type: "jsonb", nullable: false),
                    CorrelationId = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: true),
                    OccurredAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_nexarr_ai_audit_events", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "nexarr_ai_messages",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    SessionId = table.Column<Guid>(type: "uuid", nullable: false),
                    TenantId = table.Column<Guid>(type: "uuid", nullable: false),
                    ActorPersonId = table.Column<Guid>(type: "uuid", nullable: false),
                    Role = table.Column<string>(type: "character varying(32)", maxLength: 32, nullable: false),
                    Category = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: false),
                    UserInputRedacted = table.Column<string>(type: "character varying(12000)", maxLength: 12000, nullable: false),
                    OutputRedacted = table.Column<string>(type: "character varying(12000)", maxLength: 12000, nullable: false),
                    ContextJson = table.Column<string>(type: "jsonb", nullable: false),
                    Outcome = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: false),
                    ProviderResponseId = table.Column<string>(type: "character varying(128)", maxLength: 128, nullable: true),
                    ProviderRequestId = table.Column<string>(type: "character varying(128)", maxLength: 128, nullable: true),
                    InputTokens = table.Column<int>(type: "integer", nullable: true),
                    OutputTokens = table.Column<int>(type: "integer", nullable: true),
                    TotalTokens = table.Column<int>(type: "integer", nullable: true),
                    ErrorCode = table.Column<string>(type: "character varying(128)", maxLength: 128, nullable: true),
                    SafeMessage = table.Column<string>(type: "character varying(512)", maxLength: 512, nullable: true),
                    CreatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_nexarr_ai_messages", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "nexarr_ai_sessions",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    TenantId = table.Column<Guid>(type: "uuid", nullable: false),
                    ActorPersonId = table.Column<Guid>(type: "uuid", nullable: false),
                    ProductKey = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: false),
                    Surface = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: false),
                    Route = table.Column<string>(type: "character varying(512)", maxLength: 512, nullable: false),
                    Category = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: false),
                    Title = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: false),
                    Status = table.Column<string>(type: "character varying(32)", maxLength: 32, nullable: false),
                    CreatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_nexarr_ai_sessions", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "nexarr_import_audit_events",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    ImportBatchId = table.Column<Guid>(type: "uuid", nullable: false),
                    TenantId = table.Column<Guid>(type: "uuid", nullable: false),
                    EventType = table.Column<string>(type: "character varying(128)", maxLength: 128, nullable: false),
                    ActorType = table.Column<string>(type: "character varying(32)", maxLength: 32, nullable: false),
                    ActorPersonId = table.Column<Guid>(type: "uuid", nullable: true),
                    Result = table.Column<string>(type: "character varying(32)", maxLength: 32, nullable: false),
                    ReasonCode = table.Column<string>(type: "character varying(128)", maxLength: 128, nullable: true),
                    MetadataJson = table.Column<string>(type: "jsonb", nullable: false),
                    OccurredAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_nexarr_import_audit_events", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "nexarr_import_batches",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    TenantId = table.Column<Guid>(type: "uuid", nullable: false),
                    ActorPersonId = table.Column<Guid>(type: "uuid", nullable: false),
                    Status = table.Column<string>(type: "character varying(32)", maxLength: 32, nullable: false),
                    DestinationProductHint = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: false),
                    SourceLabel = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: false),
                    ReviewPolicyJson = table.Column<string>(type: "jsonb", nullable: false),
                    ErrorCode = table.Column<string>(type: "character varying(128)", maxLength: 128, nullable: true),
                    ErrorMessage = table.Column<string>(type: "character varying(1024)", maxLength: 1024, nullable: true),
                    CreatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    ProcessingStartedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    ProcessingCompletedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_nexarr_import_batches", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "nexarr_import_classifications",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    ImportBatchId = table.Column<Guid>(type: "uuid", nullable: false),
                    ImportFileId = table.Column<Guid>(type: "uuid", nullable: false),
                    TenantId = table.Column<Guid>(type: "uuid", nullable: false),
                    DestinationProduct = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: false),
                    EntityType = table.Column<string>(type: "character varying(128)", maxLength: 128, nullable: false),
                    Confidence = table.Column<decimal>(type: "numeric", nullable: false),
                    RequiresReview = table.Column<bool>(type: "boolean", nullable: false),
                    ReviewReasonsJson = table.Column<string>(type: "jsonb", nullable: false),
                    Notes = table.Column<string>(type: "character varying(1024)", maxLength: 1024, nullable: true),
                    ProviderOutcome = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: false),
                    CreatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_nexarr_import_classifications", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "nexarr_import_commit_plans",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    ImportBatchId = table.Column<Guid>(type: "uuid", nullable: false),
                    TenantId = table.Column<Guid>(type: "uuid", nullable: false),
                    CreatedByPersonId = table.Column<Guid>(type: "uuid", nullable: false),
                    ApprovedByPersonId = table.Column<Guid>(type: "uuid", nullable: true),
                    Status = table.Column<string>(type: "character varying(32)", maxLength: 32, nullable: false),
                    CreatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    ApprovedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    CommittedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_nexarr_import_commit_plans", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "nexarr_import_commit_steps",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    ImportCommitPlanId = table.Column<Guid>(type: "uuid", nullable: false),
                    ImportBatchId = table.Column<Guid>(type: "uuid", nullable: false),
                    ImportProposedRecordId = table.Column<Guid>(type: "uuid", nullable: false),
                    TenantId = table.Column<Guid>(type: "uuid", nullable: false),
                    StepOrder = table.Column<int>(type: "integer", nullable: false),
                    DestinationProduct = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: false),
                    EntityType = table.Column<string>(type: "character varying(128)", maxLength: 128, nullable: false),
                    Operation = table.Column<string>(type: "character varying(32)", maxLength: 32, nullable: false),
                    Status = table.Column<string>(type: "character varying(32)", maxLength: 32, nullable: false),
                    IdempotencyKey = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: false),
                    PayloadJson = table.Column<string>(type: "jsonb", nullable: false),
                    ResultEntityId = table.Column<string>(type: "character varying(128)", maxLength: 128, nullable: true),
                    ResultDisplayName = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: true),
                    ErrorCode = table.Column<string>(type: "character varying(128)", maxLength: 128, nullable: true),
                    ErrorMessage = table.Column<string>(type: "character varying(1024)", maxLength: 1024, nullable: true),
                    Retryable = table.Column<bool>(type: "boolean", nullable: false),
                    CreatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    CompletedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_nexarr_import_commit_steps", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "nexarr_import_extracted_fields",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    ImportBatchId = table.Column<Guid>(type: "uuid", nullable: false),
                    ImportFileId = table.Column<Guid>(type: "uuid", nullable: false),
                    TenantId = table.Column<Guid>(type: "uuid", nullable: false),
                    FieldKey = table.Column<string>(type: "character varying(128)", maxLength: 128, nullable: false),
                    RawValue = table.Column<string>(type: "character varying(4096)", maxLength: 4096, nullable: true),
                    NormalizedValue = table.Column<string>(type: "character varying(4096)", maxLength: 4096, nullable: true),
                    Confidence = table.Column<decimal>(type: "numeric", nullable: false),
                    RequiresReview = table.Column<bool>(type: "boolean", nullable: false),
                    ReviewReasonsJson = table.Column<string>(type: "jsonb", nullable: false),
                    SourceLocationJson = table.Column<string>(type: "jsonb", nullable: false),
                    CreatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_nexarr_import_extracted_fields", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "nexarr_import_files",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    ImportBatchId = table.Column<Guid>(type: "uuid", nullable: false),
                    TenantId = table.Column<Guid>(type: "uuid", nullable: false),
                    FileName = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: false),
                    ContentType = table.Column<string>(type: "character varying(128)", maxLength: 128, nullable: false),
                    SizeBytes = table.Column<long>(type: "bigint", nullable: false),
                    Sha256 = table.Column<string>(type: "character varying(128)", maxLength: 128, nullable: false),
                    RecordArrRecordId = table.Column<string>(type: "character varying(128)", maxLength: 128, nullable: true),
                    RecordArrFileId = table.Column<string>(type: "character varying(128)", maxLength: 128, nullable: true),
                    RecordArrStorageKey = table.Column<string>(type: "character varying(512)", maxLength: 512, nullable: true),
                    PageCount = table.Column<int>(type: "integer", nullable: true),
                    SheetCount = table.Column<int>(type: "integer", nullable: true),
                    RowCount = table.Column<int>(type: "integer", nullable: true),
                    Status = table.Column<string>(type: "character varying(32)", maxLength: 32, nullable: false),
                    CreatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_nexarr_import_files", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "nexarr_import_mapping_templates",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    TenantId = table.Column<Guid>(type: "uuid", nullable: false),
                    DestinationProduct = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: false),
                    EntityType = table.Column<string>(type: "character varying(128)", maxLength: 128, nullable: false),
                    TemplateName = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: false),
                    MappingJson = table.Column<string>(type: "jsonb", nullable: false),
                    Active = table.Column<bool>(type: "boolean", nullable: false),
                    CreatedByPersonId = table.Column<Guid>(type: "uuid", nullable: false),
                    CreatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_nexarr_import_mapping_templates", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "nexarr_import_match_candidates",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    ImportBatchId = table.Column<Guid>(type: "uuid", nullable: false),
                    ImportProposedRecordId = table.Column<Guid>(type: "uuid", nullable: false),
                    TenantId = table.Column<Guid>(type: "uuid", nullable: false),
                    SourceProduct = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: false),
                    EntityType = table.Column<string>(type: "character varying(128)", maxLength: 128, nullable: false),
                    EntityId = table.Column<string>(type: "character varying(128)", maxLength: 128, nullable: false),
                    DisplayName = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: false),
                    Confidence = table.Column<decimal>(type: "numeric", nullable: false),
                    MatchReasonsJson = table.Column<string>(type: "jsonb", nullable: false),
                    CreatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_nexarr_import_match_candidates", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "nexarr_import_proposed_records",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    ImportBatchId = table.Column<Guid>(type: "uuid", nullable: false),
                    TenantId = table.Column<Guid>(type: "uuid", nullable: false),
                    DestinationProduct = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: false),
                    EntityType = table.Column<string>(type: "character varying(128)", maxLength: 128, nullable: false),
                    Operation = table.Column<string>(type: "character varying(32)", maxLength: 32, nullable: false),
                    Confidence = table.Column<decimal>(type: "numeric", nullable: false),
                    ReviewStatus = table.Column<string>(type: "character varying(32)", maxLength: 32, nullable: false),
                    RequiresReview = table.Column<bool>(type: "boolean", nullable: false),
                    ReviewReasonsJson = table.Column<string>(type: "jsonb", nullable: false),
                    ProposedPayloadJson = table.Column<string>(type: "jsonb", nullable: false),
                    DeterministicPayloadJson = table.Column<string>(type: "jsonb", nullable: true),
                    CreatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_nexarr_import_proposed_records", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "nexarr_import_review_decisions",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    ImportBatchId = table.Column<Guid>(type: "uuid", nullable: false),
                    ImportProposedRecordId = table.Column<Guid>(type: "uuid", nullable: false),
                    TenantId = table.Column<Guid>(type: "uuid", nullable: false),
                    ReviewerPersonId = table.Column<Guid>(type: "uuid", nullable: false),
                    Decision = table.Column<string>(type: "character varying(32)", maxLength: 32, nullable: false),
                    Notes = table.Column<string>(type: "character varying(1024)", maxLength: 1024, nullable: true),
                    CorrectedPayloadJson = table.Column<string>(type: "jsonb", nullable: true),
                    DecidedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_nexarr_import_review_decisions", x => x.Id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_nexarr_ai_action_proposals_TenantId_SessionId_Status",
                table: "nexarr_ai_action_proposals",
                columns: new[] { "TenantId", "SessionId", "Status" });

            migrationBuilder.CreateIndex(
                name: "IX_nexarr_ai_audit_events_TargetType_TargetId",
                table: "nexarr_ai_audit_events",
                columns: new[] { "TargetType", "TargetId" });

            migrationBuilder.CreateIndex(
                name: "IX_nexarr_ai_audit_events_TenantId_OccurredAt",
                table: "nexarr_ai_audit_events",
                columns: new[] { "TenantId", "OccurredAt" });

            migrationBuilder.CreateIndex(
                name: "IX_nexarr_ai_messages_TenantId_SessionId_CreatedAt",
                table: "nexarr_ai_messages",
                columns: new[] { "TenantId", "SessionId", "CreatedAt" });

            migrationBuilder.CreateIndex(
                name: "IX_nexarr_ai_sessions_TenantId_ActorPersonId_UpdatedAt",
                table: "nexarr_ai_sessions",
                columns: new[] { "TenantId", "ActorPersonId", "UpdatedAt" });

            migrationBuilder.CreateIndex(
                name: "IX_nexarr_import_audit_events_TenantId_ImportBatchId_OccurredAt",
                table: "nexarr_import_audit_events",
                columns: new[] { "TenantId", "ImportBatchId", "OccurredAt" });

            migrationBuilder.CreateIndex(
                name: "IX_nexarr_import_batches_TenantId_ActorPersonId_UpdatedAt",
                table: "nexarr_import_batches",
                columns: new[] { "TenantId", "ActorPersonId", "UpdatedAt" });

            migrationBuilder.CreateIndex(
                name: "IX_nexarr_import_batches_TenantId_Status_CreatedAt",
                table: "nexarr_import_batches",
                columns: new[] { "TenantId", "Status", "CreatedAt" });

            migrationBuilder.CreateIndex(
                name: "IX_nexarr_import_classifications_TenantId_ImportBatchId",
                table: "nexarr_import_classifications",
                columns: new[] { "TenantId", "ImportBatchId" });

            migrationBuilder.CreateIndex(
                name: "IX_nexarr_import_commit_plans_TenantId_ImportBatchId_Status",
                table: "nexarr_import_commit_plans",
                columns: new[] { "TenantId", "ImportBatchId", "Status" });

            migrationBuilder.CreateIndex(
                name: "IX_nexarr_import_commit_steps_TenantId_DestinationProduct_Idem~",
                table: "nexarr_import_commit_steps",
                columns: new[] { "TenantId", "DestinationProduct", "IdempotencyKey" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_nexarr_import_commit_steps_TenantId_ImportCommitPlanId_Step~",
                table: "nexarr_import_commit_steps",
                columns: new[] { "TenantId", "ImportCommitPlanId", "StepOrder" });

            migrationBuilder.CreateIndex(
                name: "IX_nexarr_import_extracted_fields_TenantId_ImportBatchId",
                table: "nexarr_import_extracted_fields",
                columns: new[] { "TenantId", "ImportBatchId" });

            migrationBuilder.CreateIndex(
                name: "IX_nexarr_import_files_TenantId_ImportBatchId",
                table: "nexarr_import_files",
                columns: new[] { "TenantId", "ImportBatchId" });

            migrationBuilder.CreateIndex(
                name: "IX_nexarr_import_files_TenantId_Sha256",
                table: "nexarr_import_files",
                columns: new[] { "TenantId", "Sha256" });

            migrationBuilder.CreateIndex(
                name: "IX_nexarr_import_mapping_templates_TenantId_DestinationProduct~",
                table: "nexarr_import_mapping_templates",
                columns: new[] { "TenantId", "DestinationProduct", "EntityType", "TemplateName" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_nexarr_import_match_candidates_TenantId_ImportProposedRecor~",
                table: "nexarr_import_match_candidates",
                columns: new[] { "TenantId", "ImportProposedRecordId" });

            migrationBuilder.CreateIndex(
                name: "IX_nexarr_import_proposed_records_TenantId_ImportBatchId_Revie~",
                table: "nexarr_import_proposed_records",
                columns: new[] { "TenantId", "ImportBatchId", "ReviewStatus" });

            migrationBuilder.CreateIndex(
                name: "IX_nexarr_import_review_decisions_TenantId_ImportProposedRecor~",
                table: "nexarr_import_review_decisions",
                columns: new[] { "TenantId", "ImportProposedRecordId", "DecidedAt" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "nexarr_ai_action_proposals");

            migrationBuilder.DropTable(
                name: "nexarr_ai_audit_events");

            migrationBuilder.DropTable(
                name: "nexarr_ai_messages");

            migrationBuilder.DropTable(
                name: "nexarr_ai_sessions");

            migrationBuilder.DropTable(
                name: "nexarr_import_audit_events");

            migrationBuilder.DropTable(
                name: "nexarr_import_batches");

            migrationBuilder.DropTable(
                name: "nexarr_import_classifications");

            migrationBuilder.DropTable(
                name: "nexarr_import_commit_plans");

            migrationBuilder.DropTable(
                name: "nexarr_import_commit_steps");

            migrationBuilder.DropTable(
                name: "nexarr_import_extracted_fields");

            migrationBuilder.DropTable(
                name: "nexarr_import_files");

            migrationBuilder.DropTable(
                name: "nexarr_import_mapping_templates");

            migrationBuilder.DropTable(
                name: "nexarr_import_match_candidates");

            migrationBuilder.DropTable(
                name: "nexarr_import_proposed_records");

            migrationBuilder.DropTable(
                name: "nexarr_import_review_decisions");
        }
    }
}
