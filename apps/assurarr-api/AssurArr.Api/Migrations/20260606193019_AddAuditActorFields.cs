using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace AssurArr.Api.Migrations
{
    /// <inheritdoc />
    public partial class AddAuditActorFields : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql(
                """
                DO $$
                BEGIN
                    IF to_regclass('public.assurarr_quality_audit_checklists') IS NULL THEN
                        CREATE TABLE public.assurarr_quality_audit_checklists
                        (
                            "Id" uuid NOT NULL,
                            "TenantId" uuid NOT NULL,
                            "Number" character varying(64) NOT NULL,
                            "AuditId" uuid NOT NULL,
                            "Title" character varying(256) NOT NULL,
                            "Description" character varying(4000) NOT NULL,
                            "Status" character varying(32) NOT NULL,
                            "ItemRefs" text[] NOT NULL,
                            "CreatedAt" timestamp with time zone NOT NULL,
                            "UpdatedAt" timestamp with time zone NOT NULL,
                            "ClosedAt" timestamp with time zone NULL,
                            "ClosedByPersonId" uuid NULL,
                            "ClosureSummary" character varying(4000) NULL,
                            CONSTRAINT "PK_assurarr_quality_audit_checklists" PRIMARY KEY ("Id")
                        );
                    END IF;

                    IF to_regclass('public.assurarr_quality_audit_checklist_items') IS NULL THEN
                        CREATE TABLE public.assurarr_quality_audit_checklist_items
                        (
                            "Id" uuid NOT NULL,
                            "TenantId" uuid NOT NULL,
                            "Number" character varying(64) NOT NULL,
                            "ChecklistId" uuid NOT NULL,
                            "Sequence" integer NOT NULL,
                            "Prompt" character varying(4000) NOT NULL,
                            "HelpText" character varying(4000) NULL,
                            "RequirementRef" character varying(256) NULL,
                            "ResponseType" character varying(64) NOT NULL,
                            "Required" boolean NOT NULL,
                            "ResponseValue" character varying(4000) NULL,
                            "Result" character varying(64) NULL,
                            "FindingCreated" boolean NOT NULL,
                            "FindingRef" character varying(256) NULL,
                            "EvidenceRecordRefs" text[] NOT NULL,
                            "AnsweredAt" timestamp with time zone NULL,
                            "AnsweredByPersonId" uuid NULL,
                            "CreatedAt" timestamp with time zone NOT NULL,
                            "UpdatedAt" timestamp with time zone NOT NULL,
                            CONSTRAINT "PK_assurarr_quality_audit_checklist_items" PRIMARY KEY ("Id")
                        );
                    END IF;
                END
                $$;
                """);

            migrationBuilder.Sql(
                """
                CREATE INDEX IF NOT EXISTS "IX_assurarr_quality_audit_checklists_TenantId"
                ON public.assurarr_quality_audit_checklists ("TenantId");
                """);

            migrationBuilder.Sql(
                """
                CREATE INDEX IF NOT EXISTS "IX_assurarr_quality_audit_checklists_TenantId_AuditId"
                ON public.assurarr_quality_audit_checklists ("TenantId", "AuditId");
                """);

            migrationBuilder.Sql(
                """
                CREATE UNIQUE INDEX IF NOT EXISTS "IX_assurarr_quality_audit_checklists_TenantId_Number"
                ON public.assurarr_quality_audit_checklists ("TenantId", "Number");
                """);

            migrationBuilder.Sql(
                """
                CREATE INDEX IF NOT EXISTS "IX_assurarr_quality_audit_checklists_TenantId_Status"
                ON public.assurarr_quality_audit_checklists ("TenantId", "Status");
                """);

            migrationBuilder.Sql(
                """
                CREATE INDEX IF NOT EXISTS "IX_assurarr_quality_audit_checklist_items_TenantId"
                ON public.assurarr_quality_audit_checklist_items ("TenantId");
                """);

            migrationBuilder.Sql(
                """
                CREATE INDEX IF NOT EXISTS "IX_assurarr_quality_audit_checklist_items_TenantId_ChecklistId"
                ON public.assurarr_quality_audit_checklist_items ("TenantId", "ChecklistId");
                """);

            migrationBuilder.Sql(
                """
                CREATE UNIQUE INDEX IF NOT EXISTS "IX_assurarr_quality_audit_checklist_items_TenantId_Number"
                ON public.assurarr_quality_audit_checklist_items ("TenantId", "Number");
                """);

            migrationBuilder.Sql(
                """
                CREATE INDEX IF NOT EXISTS "IX_assurarr_quality_audit_checklist_items_TenantId_Sequence"
                ON public.assurarr_quality_audit_checklist_items ("TenantId", "Sequence");
                """);

            migrationBuilder.AddColumn<Guid>(
                name: "CreatedByPersonId",
                table: "assurarr_verification_plans",
                type: "uuid",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"));

            migrationBuilder.AddColumn<Guid>(
                name: "UpdatedByPersonId",
                table: "assurarr_verification_plans",
                type: "uuid",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"));

            migrationBuilder.AddColumn<Guid>(
                name: "CreatedByPersonId",
                table: "assurarr_supplier_quality_issues",
                type: "uuid",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"));

            migrationBuilder.AddColumn<Guid>(
                name: "UpdatedByPersonId",
                table: "assurarr_supplier_quality_issues",
                type: "uuid",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"));

            migrationBuilder.AddColumn<Guid>(
                name: "CreatedByPersonId",
                table: "assurarr_supplier_corrective_action_requests",
                type: "uuid",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"));

            migrationBuilder.AddColumn<Guid>(
                name: "UpdatedByPersonId",
                table: "assurarr_supplier_corrective_action_requests",
                type: "uuid",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"));

            migrationBuilder.AddColumn<Guid>(
                name: "CreatedByPersonId",
                table: "assurarr_root_cause_analyses",
                type: "uuid",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"));

            migrationBuilder.AddColumn<Guid>(
                name: "UpdatedByPersonId",
                table: "assurarr_root_cause_analyses",
                type: "uuid",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"));

            migrationBuilder.AddColumn<Guid>(
                name: "CreatedByPersonId",
                table: "assurarr_quality_status_snapshots",
                type: "uuid",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"));

            migrationBuilder.AddColumn<Guid>(
                name: "UpdatedByPersonId",
                table: "assurarr_quality_status_snapshots",
                type: "uuid",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"));

            migrationBuilder.AddColumn<Guid>(
                name: "CreatedByPersonId",
                table: "assurarr_quality_scorecards",
                type: "uuid",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"));

            migrationBuilder.AddColumn<Guid>(
                name: "UpdatedByPersonId",
                table: "assurarr_quality_scorecards",
                type: "uuid",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"));

            migrationBuilder.AddColumn<Guid>(
                name: "CreatedByPersonId",
                table: "assurarr_quality_risk_profiles",
                type: "uuid",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"));

            migrationBuilder.AddColumn<Guid>(
                name: "UpdatedByPersonId",
                table: "assurarr_quality_risk_profiles",
                type: "uuid",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"));

            migrationBuilder.AddColumn<Guid>(
                name: "CreatedByPersonId",
                table: "assurarr_quality_reviews",
                type: "uuid",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"));

            migrationBuilder.AddColumn<Guid>(
                name: "UpdatedByPersonId",
                table: "assurarr_quality_reviews",
                type: "uuid",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"));

            migrationBuilder.AddColumn<Guid>(
                name: "CreatedByPersonId",
                table: "assurarr_quality_releases",
                type: "uuid",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"));

            migrationBuilder.AddColumn<Guid>(
                name: "UpdatedByPersonId",
                table: "assurarr_quality_releases",
                type: "uuid",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"));

            migrationBuilder.AddColumn<Guid>(
                name: "CreatedByPersonId",
                table: "assurarr_quality_metrics",
                type: "uuid",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"));

            migrationBuilder.AddColumn<Guid>(
                name: "UpdatedByPersonId",
                table: "assurarr_quality_metrics",
                type: "uuid",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"));

            migrationBuilder.AddColumn<Guid>(
                name: "CreatedByPersonId",
                table: "assurarr_quality_holds",
                type: "uuid",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"));

            migrationBuilder.AddColumn<Guid>(
                name: "UpdatedByPersonId",
                table: "assurarr_quality_holds",
                type: "uuid",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"));

            migrationBuilder.AddColumn<Guid>(
                name: "CreatedByPersonId",
                table: "assurarr_quality_audits",
                type: "uuid",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"));

            migrationBuilder.AddColumn<Guid>(
                name: "UpdatedByPersonId",
                table: "assurarr_quality_audits",
                type: "uuid",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"));

            migrationBuilder.AddColumn<Guid>(
                name: "CreatedByPersonId",
                table: "assurarr_quality_audit_checklists",
                type: "uuid",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"));

            migrationBuilder.AddColumn<Guid>(
                name: "UpdatedByPersonId",
                table: "assurarr_quality_audit_checklists",
                type: "uuid",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"));

            migrationBuilder.AddColumn<Guid>(
                name: "CreatedByPersonId",
                table: "assurarr_quality_audit_checklist_items",
                type: "uuid",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"));

            migrationBuilder.AddColumn<Guid>(
                name: "UpdatedByPersonId",
                table: "assurarr_quality_audit_checklist_items",
                type: "uuid",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"));

            migrationBuilder.AddColumn<Guid>(
                name: "CreatedByPersonId",
                table: "assurarr_nonconformances",
                type: "uuid",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"));

            migrationBuilder.AddColumn<Guid>(
                name: "UpdatedByPersonId",
                table: "assurarr_nonconformances",
                type: "uuid",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"));

            migrationBuilder.AddColumn<Guid>(
                name: "CreatedByPersonId",
                table: "assurarr_effectiveness_verifications",
                type: "uuid",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"));

            migrationBuilder.AddColumn<Guid>(
                name: "UpdatedByPersonId",
                table: "assurarr_effectiveness_verifications",
                type: "uuid",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"));

            migrationBuilder.AddColumn<Guid>(
                name: "CreatedByPersonId",
                table: "assurarr_dispositions",
                type: "uuid",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"));

            migrationBuilder.AddColumn<Guid>(
                name: "UpdatedByPersonId",
                table: "assurarr_dispositions",
                type: "uuid",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"));

            migrationBuilder.AddColumn<Guid>(
                name: "CreatedByPersonId",
                table: "assurarr_customer_complaint_quality_cases",
                type: "uuid",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"));

            migrationBuilder.AddColumn<Guid>(
                name: "UpdatedByPersonId",
                table: "assurarr_customer_complaint_quality_cases",
                type: "uuid",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"));

            migrationBuilder.AddColumn<Guid>(
                name: "CreatedByPersonId",
                table: "assurarr_containment_actions",
                type: "uuid",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"));

            migrationBuilder.AddColumn<Guid>(
                name: "UpdatedByPersonId",
                table: "assurarr_containment_actions",
                type: "uuid",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"));

            migrationBuilder.AddColumn<Guid>(
                name: "CreatedByPersonId",
                table: "assurarr_capas",
                type: "uuid",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"));

            migrationBuilder.AddColumn<Guid>(
                name: "UpdatedByPersonId",
                table: "assurarr_capas",
                type: "uuid",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"));

            migrationBuilder.AddColumn<Guid>(
                name: "CreatedByPersonId",
                table: "assurarr_capa_actions",
                type: "uuid",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"));

            migrationBuilder.AddColumn<Guid>(
                name: "UpdatedByPersonId",
                table: "assurarr_capa_actions",
                type: "uuid",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"));

            migrationBuilder.AddColumn<Guid>(
                name: "CreatedByPersonId",
                table: "assurarr_capa_action_blockers",
                type: "uuid",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"));

            migrationBuilder.AddColumn<Guid>(
                name: "UpdatedByPersonId",
                table: "assurarr_capa_action_blockers",
                type: "uuid",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"));

            migrationBuilder.AddColumn<Guid>(
                name: "CreatedByPersonId",
                table: "assurarr_audit_findings",
                type: "uuid",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"));

            migrationBuilder.AddColumn<Guid>(
                name: "UpdatedByPersonId",
                table: "assurarr_audit_findings",
                type: "uuid",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"));
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "CreatedByPersonId",
                table: "assurarr_verification_plans");

            migrationBuilder.DropColumn(
                name: "UpdatedByPersonId",
                table: "assurarr_verification_plans");

            migrationBuilder.DropColumn(
                name: "CreatedByPersonId",
                table: "assurarr_supplier_quality_issues");

            migrationBuilder.DropColumn(
                name: "UpdatedByPersonId",
                table: "assurarr_supplier_quality_issues");

            migrationBuilder.DropColumn(
                name: "CreatedByPersonId",
                table: "assurarr_supplier_corrective_action_requests");

            migrationBuilder.DropColumn(
                name: "UpdatedByPersonId",
                table: "assurarr_supplier_corrective_action_requests");

            migrationBuilder.DropColumn(
                name: "CreatedByPersonId",
                table: "assurarr_root_cause_analyses");

            migrationBuilder.DropColumn(
                name: "UpdatedByPersonId",
                table: "assurarr_root_cause_analyses");

            migrationBuilder.DropColumn(
                name: "CreatedByPersonId",
                table: "assurarr_quality_status_snapshots");

            migrationBuilder.DropColumn(
                name: "UpdatedByPersonId",
                table: "assurarr_quality_status_snapshots");

            migrationBuilder.DropColumn(
                name: "CreatedByPersonId",
                table: "assurarr_quality_scorecards");

            migrationBuilder.DropColumn(
                name: "UpdatedByPersonId",
                table: "assurarr_quality_scorecards");

            migrationBuilder.DropColumn(
                name: "CreatedByPersonId",
                table: "assurarr_quality_risk_profiles");

            migrationBuilder.DropColumn(
                name: "UpdatedByPersonId",
                table: "assurarr_quality_risk_profiles");

            migrationBuilder.DropColumn(
                name: "CreatedByPersonId",
                table: "assurarr_quality_reviews");

            migrationBuilder.DropColumn(
                name: "UpdatedByPersonId",
                table: "assurarr_quality_reviews");

            migrationBuilder.DropColumn(
                name: "CreatedByPersonId",
                table: "assurarr_quality_releases");

            migrationBuilder.DropColumn(
                name: "UpdatedByPersonId",
                table: "assurarr_quality_releases");

            migrationBuilder.DropColumn(
                name: "CreatedByPersonId",
                table: "assurarr_quality_metrics");

            migrationBuilder.DropColumn(
                name: "UpdatedByPersonId",
                table: "assurarr_quality_metrics");

            migrationBuilder.DropColumn(
                name: "CreatedByPersonId",
                table: "assurarr_quality_holds");

            migrationBuilder.DropColumn(
                name: "UpdatedByPersonId",
                table: "assurarr_quality_holds");

            migrationBuilder.DropColumn(
                name: "CreatedByPersonId",
                table: "assurarr_quality_audits");

            migrationBuilder.DropColumn(
                name: "UpdatedByPersonId",
                table: "assurarr_quality_audits");

            migrationBuilder.DropColumn(
                name: "CreatedByPersonId",
                table: "assurarr_quality_audit_checklists");

            migrationBuilder.DropColumn(
                name: "UpdatedByPersonId",
                table: "assurarr_quality_audit_checklists");

            migrationBuilder.DropColumn(
                name: "CreatedByPersonId",
                table: "assurarr_quality_audit_checklist_items");

            migrationBuilder.DropColumn(
                name: "UpdatedByPersonId",
                table: "assurarr_quality_audit_checklist_items");

            migrationBuilder.DropColumn(
                name: "CreatedByPersonId",
                table: "assurarr_nonconformances");

            migrationBuilder.DropColumn(
                name: "UpdatedByPersonId",
                table: "assurarr_nonconformances");

            migrationBuilder.DropColumn(
                name: "CreatedByPersonId",
                table: "assurarr_effectiveness_verifications");

            migrationBuilder.DropColumn(
                name: "UpdatedByPersonId",
                table: "assurarr_effectiveness_verifications");

            migrationBuilder.DropColumn(
                name: "CreatedByPersonId",
                table: "assurarr_dispositions");

            migrationBuilder.DropColumn(
                name: "UpdatedByPersonId",
                table: "assurarr_dispositions");

            migrationBuilder.DropColumn(
                name: "CreatedByPersonId",
                table: "assurarr_customer_complaint_quality_cases");

            migrationBuilder.DropColumn(
                name: "UpdatedByPersonId",
                table: "assurarr_customer_complaint_quality_cases");

            migrationBuilder.DropColumn(
                name: "CreatedByPersonId",
                table: "assurarr_containment_actions");

            migrationBuilder.DropColumn(
                name: "UpdatedByPersonId",
                table: "assurarr_containment_actions");

            migrationBuilder.DropColumn(
                name: "CreatedByPersonId",
                table: "assurarr_capas");

            migrationBuilder.DropColumn(
                name: "UpdatedByPersonId",
                table: "assurarr_capas");

            migrationBuilder.DropColumn(
                name: "CreatedByPersonId",
                table: "assurarr_capa_actions");

            migrationBuilder.DropColumn(
                name: "UpdatedByPersonId",
                table: "assurarr_capa_actions");

            migrationBuilder.DropColumn(
                name: "CreatedByPersonId",
                table: "assurarr_capa_action_blockers");

            migrationBuilder.DropColumn(
                name: "UpdatedByPersonId",
                table: "assurarr_capa_action_blockers");

            migrationBuilder.DropColumn(
                name: "CreatedByPersonId",
                table: "assurarr_audit_findings");

            migrationBuilder.DropColumn(
                name: "UpdatedByPersonId",
                table: "assurarr_audit_findings");
        }
    }
}
