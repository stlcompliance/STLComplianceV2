using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace AssurArr.Api.Migrations
{
    /// <inheritdoc />
    public partial class AddCapaActionBlockers : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<string>(
                name: "SupplierImpact",
                table: "assurarr_nonconformances",
                type: "character varying(4000)",
                maxLength: 4000,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "text",
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "SafetyImpact",
                table: "assurarr_nonconformances",
                type: "character varying(4000)",
                maxLength: 4000,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "text",
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "RepeatOfNonconformanceRef",
                table: "assurarr_nonconformances",
                type: "character varying(256)",
                maxLength: 256,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "text",
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "NonconformanceType",
                table: "assurarr_nonconformances",
                type: "character varying(64)",
                maxLength: 64,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "text");

            migrationBuilder.AlterColumn<string>(
                name: "CustomerImpact",
                table: "assurarr_nonconformances",
                type: "character varying(4000)",
                maxLength: 4000,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "text",
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "ComplianceImpact",
                table: "assurarr_nonconformances",
                type: "character varying(4000)",
                maxLength: 4000,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "text",
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "Category",
                table: "assurarr_nonconformances",
                type: "character varying(64)",
                maxLength: 64,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "text");

            migrationBuilder.AddColumn<string[]>(
                name: "BlockerRefs",
                table: "assurarr_nonconformances",
                type: "text[]",
                nullable: false,
                defaultValue: new string[0]);

            migrationBuilder.CreateTable(
                name: "assurarr_capa_action_blockers",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    TenantId = table.Column<Guid>(type: "uuid", nullable: false),
                    Number = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: false),
                    CapaActionId = table.Column<Guid>(type: "uuid", nullable: false),
                    BlockerType = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: false),
                    SourceProduct = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: true),
                    SourceObjectRef = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: true),
                    Title = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: false),
                    Description = table.Column<string>(type: "character varying(4000)", maxLength: 4000, nullable: false),
                    Status = table.Column<string>(type: "character varying(32)", maxLength: 32, nullable: false),
                    CreatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    ResolvedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    ResolvedByPersonId = table.Column<Guid>(type: "uuid", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_assurarr_capa_action_blockers", x => x.Id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_assurarr_capa_action_blockers_TenantId",
                table: "assurarr_capa_action_blockers",
                column: "TenantId");

            migrationBuilder.CreateIndex(
                name: "IX_assurarr_capa_action_blockers_TenantId_CapaActionId",
                table: "assurarr_capa_action_blockers",
                columns: new[] { "TenantId", "CapaActionId" });

            migrationBuilder.CreateIndex(
                name: "IX_assurarr_capa_action_blockers_TenantId_Number",
                table: "assurarr_capa_action_blockers",
                columns: new[] { "TenantId", "Number" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_assurarr_capa_action_blockers_TenantId_Status",
                table: "assurarr_capa_action_blockers",
                columns: new[] { "TenantId", "Status" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "assurarr_capa_action_blockers");

            migrationBuilder.DropColumn(
                name: "BlockerRefs",
                table: "assurarr_nonconformances");

            migrationBuilder.AlterColumn<string>(
                name: "SupplierImpact",
                table: "assurarr_nonconformances",
                type: "text",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "character varying(4000)",
                oldMaxLength: 4000,
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "SafetyImpact",
                table: "assurarr_nonconformances",
                type: "text",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "character varying(4000)",
                oldMaxLength: 4000,
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "RepeatOfNonconformanceRef",
                table: "assurarr_nonconformances",
                type: "text",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "character varying(256)",
                oldMaxLength: 256,
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "NonconformanceType",
                table: "assurarr_nonconformances",
                type: "text",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "character varying(64)",
                oldMaxLength: 64);

            migrationBuilder.AlterColumn<string>(
                name: "CustomerImpact",
                table: "assurarr_nonconformances",
                type: "text",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "character varying(4000)",
                oldMaxLength: 4000,
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "ComplianceImpact",
                table: "assurarr_nonconformances",
                type: "text",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "character varying(4000)",
                oldMaxLength: 4000,
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "Category",
                table: "assurarr_nonconformances",
                type: "text",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "character varying(64)",
                oldMaxLength: 64);
        }
    }
}
