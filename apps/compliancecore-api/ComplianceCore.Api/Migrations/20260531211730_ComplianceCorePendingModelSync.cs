using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ComplianceCore.Api.Migrations
{
    /// <inheritdoc />
    public partial class ComplianceCorePendingModelSync : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "compliancecore_product_gate_responses",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    TenantId = table.Column<Guid>(type: "uuid", nullable: false),
                    WorkflowGateCheckResultId = table.Column<Guid>(type: "uuid", nullable: false),
                    SourceProduct = table.Column<string>(type: "character varying(32)", maxLength: 32, nullable: false),
                    ResponseOutcome = table.Column<string>(type: "character varying(32)", maxLength: 32, nullable: false),
                    ResponseCode = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: true),
                    ResponseMessage = table.Column<string>(type: "character varying(1024)", maxLength: 1024, nullable: true),
                    ResponsePayloadJson = table.Column<string>(type: "jsonb", nullable: false),
                    RespondedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_compliancecore_product_gate_responses", x => x.Id);
                    table.ForeignKey(
                        name: "FK_compliancecore_product_gate_responses_compliancecore_workfl~",
                        column: x => x.WorkflowGateCheckResultId,
                        principalTable: "compliancecore_workflow_gate_check_results",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_compliancecore_product_gate_responses_TenantId",
                table: "compliancecore_product_gate_responses",
                column: "TenantId");

            migrationBuilder.CreateIndex(
                name: "IX_compliancecore_product_gate_responses_TenantId_WorkflowGate~",
                table: "compliancecore_product_gate_responses",
                columns: new[] { "TenantId", "WorkflowGateCheckResultId", "RespondedAt" });

            migrationBuilder.CreateIndex(
                name: "IX_compliancecore_product_gate_responses_WorkflowGateCheckResu~",
                table: "compliancecore_product_gate_responses",
                column: "WorkflowGateCheckResultId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "compliancecore_product_gate_responses");
        }
    }
}
