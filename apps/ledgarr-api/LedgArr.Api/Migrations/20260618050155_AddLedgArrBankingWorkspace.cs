using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace LedgArr.Api.Migrations
{
    /// <inheritdoc />
    public partial class AddLedgArrBankingWorkspace : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "BankAccounts",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    TenantId = table.Column<Guid>(type: "uuid", nullable: false),
                    FinancialLegalEntityId = table.Column<Guid>(type: "uuid", nullable: false),
                    BankName = table.Column<string>(type: "text", nullable: false),
                    AccountDisplayName = table.Column<string>(type: "text", nullable: false),
                    AccountType = table.Column<string>(type: "text", nullable: false),
                    MaskedAccountNumber = table.Column<string>(type: "text", nullable: false),
                    CurrencyCode = table.Column<string>(type: "text", nullable: false),
                    GLCashAccountId = table.Column<Guid>(type: "uuid", nullable: false),
                    Status = table.Column<string>(type: "text", nullable: false),
                    ReconciliationEnabled = table.Column<bool>(type: "boolean", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_BankAccounts", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "BankReconciliations",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    TenantId = table.Column<Guid>(type: "uuid", nullable: false),
                    BankAccountId = table.Column<Guid>(type: "uuid", nullable: false),
                    PeriodStartDate = table.Column<DateOnly>(type: "date", nullable: false),
                    PeriodEndDate = table.Column<DateOnly>(type: "date", nullable: false),
                    BeginningBalance = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: false),
                    EndingBalance = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: false),
                    StatementDate = table.Column<DateOnly>(type: "date", nullable: false),
                    ClearedTransactionTotal = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: false),
                    AdjustmentTotal = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: false),
                    MatchedTransactionCount = table.Column<int>(type: "integer", nullable: false),
                    ExceptionCount = table.Column<int>(type: "integer", nullable: false),
                    ApprovalStatus = table.Column<string>(type: "text", nullable: false),
                    LockStatus = table.Column<string>(type: "text", nullable: false),
                    Status = table.Column<string>(type: "text", nullable: false),
                    CreatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    ApprovedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    LockedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_BankReconciliations", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "BankTransactions",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    TenantId = table.Column<Guid>(type: "uuid", nullable: false),
                    BankAccountId = table.Column<Guid>(type: "uuid", nullable: false),
                    TransactionDate = table.Column<DateOnly>(type: "date", nullable: false),
                    Description = table.Column<string>(type: "text", nullable: false),
                    Amount = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: false),
                    Direction = table.Column<string>(type: "text", nullable: false),
                    SourceType = table.Column<string>(type: "text", nullable: false),
                    MatchStatus = table.Column<string>(type: "text", nullable: false),
                    MatchedLedgArrTransactionType = table.Column<string>(type: "text", nullable: true),
                    MatchedLedgArrTransactionId = table.Column<string>(type: "text", nullable: true),
                    ReconciliationStatus = table.Column<string>(type: "text", nullable: false),
                    ReconciliationId = table.Column<Guid>(type: "uuid", nullable: true),
                    JournalEntryId = table.Column<Guid>(type: "uuid", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_BankTransactions", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "LedgArrTenantSettingsAudits",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    TenantId = table.Column<Guid>(type: "uuid", nullable: false),
                    SectionKey = table.Column<string>(type: "text", nullable: false),
                    ChangedAtUtc = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    ChangedByPersonId = table.Column<Guid>(type: "uuid", nullable: false),
                    ChangeReason = table.Column<string>(type: "text", nullable: true),
                    BeforeJson = table.Column<string>(type: "text", nullable: true),
                    AfterJson = table.Column<string>(type: "text", nullable: true),
                    DiffJson = table.Column<string>(type: "text", nullable: true),
                    CorrelationId = table.Column<string>(type: "text", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_LedgArrTenantSettingsAudits", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "LedgArrTenantSettingSections",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    TenantId = table.Column<Guid>(type: "uuid", nullable: false),
                    SectionKey = table.Column<string>(type: "text", nullable: false),
                    SettingsVersion = table.Column<int>(type: "integer", nullable: false),
                    SettingsJson = table.Column<string>(type: "text", nullable: false),
                    CreatedAtUtc = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    CreatedByPersonId = table.Column<Guid>(type: "uuid", nullable: false),
                    UpdatedAtUtc = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    UpdatedByPersonId = table.Column<Guid>(type: "uuid", nullable: false),
                    RowVersion = table.Column<string>(type: "text", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_LedgArrTenantSettingSections", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "PayrollBatches",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    TenantId = table.Column<Guid>(type: "uuid", nullable: false),
                    LegalEntityId = table.Column<Guid>(type: "uuid", nullable: false),
                    PayrollCalendarId = table.Column<Guid>(type: "uuid", nullable: false),
                    PeriodStartDate = table.Column<DateOnly>(type: "date", nullable: false),
                    PeriodEndDate = table.Column<DateOnly>(type: "date", nullable: false),
                    PayDate = table.Column<DateOnly>(type: "date", nullable: false),
                    Status = table.Column<string>(type: "character varying(32)", maxLength: 32, nullable: false),
                    SourceStaffArrSnapshotId = table.Column<Guid>(type: "uuid", nullable: true),
                    SourceSnapshotHash = table.Column<string>(type: "character varying(128)", maxLength: 128, nullable: true),
                    TotalWorkers = table.Column<int>(type: "integer", nullable: false),
                    TotalHours = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: false),
                    TotalGrossEstimate = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: true),
                    ExportProvider = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: false),
                    ExportedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    ApprovedByPersonId = table.Column<Guid>(type: "uuid", nullable: true),
                    ApprovedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    CorrectionReason = table.Column<string>(type: "character varying(2048)", maxLength: 2048, nullable: true),
                    CreatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PayrollBatches", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "PayrollBatchLines",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    TenantId = table.Column<Guid>(type: "uuid", nullable: false),
                    PayrollBatchId = table.Column<Guid>(type: "uuid", nullable: false),
                    PersonId = table.Column<Guid>(type: "uuid", nullable: false),
                    WorkerNumber = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: false),
                    LegalEntityId = table.Column<Guid>(type: "uuid", nullable: false),
                    PayrollCalendarId = table.Column<Guid>(type: "uuid", nullable: false),
                    PayCodeRef = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: false),
                    ProviderEarningCode = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: false),
                    DurationMinutes = table.Column<int>(type: "integer", nullable: false),
                    RateSnapshot = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: true),
                    GrossEstimate = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: true),
                    AllocationSnapshot = table.Column<string>(type: "text", nullable: false),
                    SourceTimesheetPeriodRef = table.Column<string>(type: "character varying(128)", maxLength: 128, nullable: false),
                    SourceTimeEntryRefs = table.Column<string>(type: "text", nullable: false),
                    ValidationStatus = table.Column<string>(type: "character varying(32)", maxLength: 32, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PayrollBatchLines", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "PayrollCalendars",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    TenantId = table.Column<Guid>(type: "uuid", nullable: false),
                    LegalEntityId = table.Column<Guid>(type: "uuid", nullable: false),
                    Name = table.Column<string>(type: "character varying(128)", maxLength: 128, nullable: false),
                    Frequency = table.Column<string>(type: "character varying(32)", maxLength: 32, nullable: false),
                    PeriodStartDate = table.Column<DateOnly>(type: "date", nullable: false),
                    PeriodEndDate = table.Column<DateOnly>(type: "date", nullable: false),
                    PayDate = table.Column<DateOnly>(type: "date", nullable: false),
                    CutoffDate = table.Column<DateOnly>(type: "date", nullable: false),
                    Timezone = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: false),
                    Status = table.Column<string>(type: "character varying(32)", maxLength: 32, nullable: false),
                    CreatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PayrollCalendars", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "PayrollCodeMappings",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    TenantId = table.Column<Guid>(type: "uuid", nullable: false),
                    LegalEntityId = table.Column<Guid>(type: "uuid", nullable: false),
                    StaffArrPayCodeRef = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: false),
                    PayrollProviderRef = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: true),
                    ProviderEarningCode = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: false),
                    ProviderDeductionCode = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: true),
                    GlAccountRef = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: false),
                    CostCenterRef = table.Column<string>(type: "character varying(128)", maxLength: 128, nullable: true),
                    DepartmentRef = table.Column<string>(type: "character varying(128)", maxLength: 128, nullable: true),
                    TaxableTreatmentSnapshot = table.Column<string>(type: "character varying(1024)", maxLength: 1024, nullable: true),
                    Active = table.Column<bool>(type: "boolean", nullable: false),
                    EffectiveStartDate = table.Column<DateOnly>(type: "date", nullable: false),
                    EffectiveEndDate = table.Column<DateOnly>(type: "date", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PayrollCodeMappings", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "PayrollExportPackets",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    TenantId = table.Column<Guid>(type: "uuid", nullable: false),
                    PayrollBatchId = table.Column<Guid>(type: "uuid", nullable: false),
                    ProviderKey = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: false),
                    ExportFormat = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: false),
                    FileRef = table.Column<string>(type: "character varying(512)", maxLength: 512, nullable: true),
                    PayloadHash = table.Column<string>(type: "character varying(128)", maxLength: 128, nullable: false),
                    ExportedByPersonId = table.Column<Guid>(type: "uuid", nullable: false),
                    ExportedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    ProviderResponseStatus = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: false),
                    ProviderResponseRef = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: true),
                    Errors = table.Column<string>(type: "text", nullable: true),
                    ReplayProtectionKey = table.Column<string>(type: "character varying(128)", maxLength: 128, nullable: false),
                    CreatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PayrollExportPackets", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "PayrollJournalSnapshots",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    TenantId = table.Column<Guid>(type: "uuid", nullable: false),
                    PayrollBatchId = table.Column<Guid>(type: "uuid", nullable: false),
                    LegalEntityId = table.Column<Guid>(type: "uuid", nullable: false),
                    GlAccountRef = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: false),
                    CostCenterRef = table.Column<string>(type: "character varying(128)", maxLength: 128, nullable: true),
                    DepartmentRef = table.Column<string>(type: "character varying(128)", maxLength: 128, nullable: true),
                    ProductKey = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: false),
                    CostObjectType = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: false),
                    CostObjectRef = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: false),
                    DebitAmount = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: false),
                    CreditAmount = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: false),
                    Currency = table.Column<string>(type: "character varying(8)", maxLength: 8, nullable: false),
                    SourcePayrollBatchLineRefs = table.Column<string>(type: "text", nullable: false),
                    Status = table.Column<string>(type: "character varying(32)", maxLength: 32, nullable: false),
                    CreatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PayrollJournalSnapshots", x => x.Id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_BankAccounts_TenantId_AccountDisplayName",
                table: "BankAccounts",
                columns: new[] { "TenantId", "AccountDisplayName" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_LedgArrTenantSettingSections_TenantId_SectionKey",
                table: "LedgArrTenantSettingSections",
                columns: new[] { "TenantId", "SectionKey" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_PayrollBatches_TenantId_LegalEntityId_PayrollCalendarId_Per~",
                table: "PayrollBatches",
                columns: new[] { "TenantId", "LegalEntityId", "PayrollCalendarId", "PeriodStartDate", "PeriodEndDate" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_PayrollBatchLines_TenantId_PayrollBatchId_PersonId_PayCodeR~",
                table: "PayrollBatchLines",
                columns: new[] { "TenantId", "PayrollBatchId", "PersonId", "PayCodeRef", "SourceTimesheetPeriodRef" });

            migrationBuilder.CreateIndex(
                name: "IX_PayrollCalendars_TenantId_LegalEntityId_Name",
                table: "PayrollCalendars",
                columns: new[] { "TenantId", "LegalEntityId", "Name" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_PayrollCodeMappings_TenantId_LegalEntityId_StaffArrPayCodeR~",
                table: "PayrollCodeMappings",
                columns: new[] { "TenantId", "LegalEntityId", "StaffArrPayCodeRef", "ProviderEarningCode" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_PayrollExportPackets_TenantId_PayrollBatchId_PayloadHash",
                table: "PayrollExportPackets",
                columns: new[] { "TenantId", "PayrollBatchId", "PayloadHash" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_PayrollExportPackets_TenantId_ReplayProtectionKey",
                table: "PayrollExportPackets",
                columns: new[] { "TenantId", "ReplayProtectionKey" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_PayrollJournalSnapshots_TenantId_PayrollBatchId",
                table: "PayrollJournalSnapshots",
                columns: new[] { "TenantId", "PayrollBatchId" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "BankAccounts");

            migrationBuilder.DropTable(
                name: "BankReconciliations");

            migrationBuilder.DropTable(
                name: "BankTransactions");

            migrationBuilder.DropTable(
                name: "LedgArrTenantSettingsAudits");

            migrationBuilder.DropTable(
                name: "LedgArrTenantSettingSections");

            migrationBuilder.DropTable(
                name: "PayrollBatches");

            migrationBuilder.DropTable(
                name: "PayrollBatchLines");

            migrationBuilder.DropTable(
                name: "PayrollCalendars");

            migrationBuilder.DropTable(
                name: "PayrollCodeMappings");

            migrationBuilder.DropTable(
                name: "PayrollExportPackets");

            migrationBuilder.DropTable(
                name: "PayrollJournalSnapshots");
        }
    }
}
