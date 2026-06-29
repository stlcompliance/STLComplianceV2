using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace RecordArr.Api.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddRecordArrDurableControlledDocuments : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "recordarr_controlled_document_versions",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    TenantId = table.Column<Guid>(type: "uuid", nullable: false),
                    VersionId = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: false),
                    ControlledDocumentId = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: false),
                    VersionNumber = table.Column<int>(type: "integer", nullable: false),
                    VersionLabel = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: false),
                    Status = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: false),
                    FileName = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: false),
                    CreatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    CreatedByPersonId = table.Column<string>(type: "character varying(128)", maxLength: 128, nullable: false),
                    SubmittedForReviewAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    ApprovedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    ApprovedByPersonId = table.Column<string>(type: "character varying(128)", maxLength: 128, nullable: true),
                    EffectiveAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    SupersededAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    PreviousVersionRef = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: true),
                    NextVersionRef = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: true),
                    FileRef = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: true),
                    PayloadJson = table.Column<string>(type: "jsonb", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_recordarr_controlled_document_versions", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "recordarr_controlled_documents",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    TenantId = table.Column<Guid>(type: "uuid", nullable: false),
                    ControlledDocumentId = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: false),
                    DocumentNumber = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: false),
                    RecordId = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: false),
                    Title = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: false),
                    DocumentClass = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: false),
                    DocumentType = table.Column<string>(type: "character varying(96)", maxLength: 96, nullable: false),
                    DocumentSubtype = table.Column<string>(type: "character varying(96)", maxLength: 96, nullable: false),
                    ControlledDocumentType = table.Column<string>(type: "character varying(96)", maxLength: 96, nullable: false),
                    Status = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: false),
                    OwnerPersonId = table.Column<string>(type: "character varying(128)", maxLength: 128, nullable: false),
                    DepartmentOrgUnitId = table.Column<string>(type: "character varying(128)", maxLength: 128, nullable: false),
                    StaffarrSiteId = table.Column<string>(type: "character varying(128)", maxLength: 128, nullable: false),
                    CurrentVersionId = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: false),
                    NextReviewAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    EffectiveAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    ExpiresAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    AcknowledgementRequired = table.Column<bool>(type: "boolean", nullable: false),
                    PayloadJson = table.Column<string>(type: "jsonb", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_recordarr_controlled_documents", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "recordarr_document_acknowledgements",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    TenantId = table.Column<Guid>(type: "uuid", nullable: false),
                    AcknowledgementId = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: false),
                    ControlledDocumentId = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: false),
                    VersionId = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: false),
                    PersonId = table.Column<string>(type: "character varying(128)", maxLength: 128, nullable: false),
                    Status = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: false),
                    AcknowledgedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    SignatureRecordRef = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: true),
                    DueAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    PayloadJson = table.Column<string>(type: "jsonb", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_recordarr_document_acknowledgements", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "recordarr_document_distributions",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    TenantId = table.Column<Guid>(type: "uuid", nullable: false),
                    DistributionId = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: false),
                    ControlledDocumentId = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: false),
                    VersionId = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: false),
                    DistributionType = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: false),
                    TargetRef = table.Column<string>(type: "character varying(160)", maxLength: 160, nullable: false),
                    Status = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: false),
                    DistributedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    AcknowledgedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    AcknowledgementRef = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: true),
                    PayloadJson = table.Column<string>(type: "jsonb", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_recordarr_document_distributions", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "recordarr_document_reviews",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    TenantId = table.Column<Guid>(type: "uuid", nullable: false),
                    DocumentReviewId = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: false),
                    ControlledDocumentId = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: false),
                    VersionId = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: false),
                    ReviewType = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: false),
                    Status = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: false),
                    RequestedByPersonId = table.Column<string>(type: "character varying(128)", maxLength: 128, nullable: false),
                    ReviewerPersonId = table.Column<string>(type: "character varying(128)", maxLength: 128, nullable: false),
                    RequestedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    DueAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    ReviewedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    PayloadJson = table.Column<string>(type: "jsonb", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_recordarr_document_reviews", x => x.Id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_recordarr_controlled_document_versions_TenantId",
                table: "recordarr_controlled_document_versions",
                column: "TenantId");

            migrationBuilder.CreateIndex(
                name: "IX_recordarr_controlled_document_versions_TenantId_ControlledD~",
                table: "recordarr_controlled_document_versions",
                columns: new[] { "TenantId", "ControlledDocumentId", "VersionNumber" });

            migrationBuilder.CreateIndex(
                name: "IX_recordarr_controlled_document_versions_TenantId_Status_Crea~",
                table: "recordarr_controlled_document_versions",
                columns: new[] { "TenantId", "Status", "CreatedAt" });

            migrationBuilder.CreateIndex(
                name: "IX_recordarr_controlled_document_versions_TenantId_VersionId",
                table: "recordarr_controlled_document_versions",
                columns: new[] { "TenantId", "VersionId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_recordarr_controlled_documents_TenantId",
                table: "recordarr_controlled_documents",
                column: "TenantId");

            migrationBuilder.CreateIndex(
                name: "IX_recordarr_controlled_documents_TenantId_ControlledDocumentId",
                table: "recordarr_controlled_documents",
                columns: new[] { "TenantId", "ControlledDocumentId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_recordarr_controlled_documents_TenantId_RecordId",
                table: "recordarr_controlled_documents",
                columns: new[] { "TenantId", "RecordId" });

            migrationBuilder.CreateIndex(
                name: "IX_recordarr_controlled_documents_TenantId_Status_NextReviewAt",
                table: "recordarr_controlled_documents",
                columns: new[] { "TenantId", "Status", "NextReviewAt" });

            migrationBuilder.CreateIndex(
                name: "IX_recordarr_document_acknowledgements_TenantId",
                table: "recordarr_document_acknowledgements",
                column: "TenantId");

            migrationBuilder.CreateIndex(
                name: "IX_recordarr_document_acknowledgements_TenantId_Acknowledgemen~",
                table: "recordarr_document_acknowledgements",
                columns: new[] { "TenantId", "AcknowledgementId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_recordarr_document_acknowledgements_TenantId_ControlledDocu~",
                table: "recordarr_document_acknowledgements",
                columns: new[] { "TenantId", "ControlledDocumentId", "Status" });

            migrationBuilder.CreateIndex(
                name: "IX_recordarr_document_acknowledgements_TenantId_PersonId_DueAt",
                table: "recordarr_document_acknowledgements",
                columns: new[] { "TenantId", "PersonId", "DueAt" });

            migrationBuilder.CreateIndex(
                name: "IX_recordarr_document_distributions_TenantId",
                table: "recordarr_document_distributions",
                column: "TenantId");

            migrationBuilder.CreateIndex(
                name: "IX_recordarr_document_distributions_TenantId_ControlledDocumen~",
                table: "recordarr_document_distributions",
                columns: new[] { "TenantId", "ControlledDocumentId", "Status" });

            migrationBuilder.CreateIndex(
                name: "IX_recordarr_document_distributions_TenantId_DistributionId",
                table: "recordarr_document_distributions",
                columns: new[] { "TenantId", "DistributionId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_recordarr_document_distributions_TenantId_TargetRef_Status",
                table: "recordarr_document_distributions",
                columns: new[] { "TenantId", "TargetRef", "Status" });

            migrationBuilder.CreateIndex(
                name: "IX_recordarr_document_reviews_TenantId",
                table: "recordarr_document_reviews",
                column: "TenantId");

            migrationBuilder.CreateIndex(
                name: "IX_recordarr_document_reviews_TenantId_ControlledDocumentId_St~",
                table: "recordarr_document_reviews",
                columns: new[] { "TenantId", "ControlledDocumentId", "Status" });

            migrationBuilder.CreateIndex(
                name: "IX_recordarr_document_reviews_TenantId_DocumentReviewId",
                table: "recordarr_document_reviews",
                columns: new[] { "TenantId", "DocumentReviewId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_recordarr_document_reviews_TenantId_ReviewerPersonId_DueAt",
                table: "recordarr_document_reviews",
                columns: new[] { "TenantId", "ReviewerPersonId", "DueAt" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "recordarr_controlled_document_versions");

            migrationBuilder.DropTable(
                name: "recordarr_controlled_documents");

            migrationBuilder.DropTable(
                name: "recordarr_document_acknowledgements");

            migrationBuilder.DropTable(
                name: "recordarr_document_distributions");

            migrationBuilder.DropTable(
                name: "recordarr_document_reviews");
        }
    }
}
