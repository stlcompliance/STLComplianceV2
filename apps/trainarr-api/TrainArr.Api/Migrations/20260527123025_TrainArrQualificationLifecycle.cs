using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace TrainArr.Api.Migrations
{
    /// <inheritdoc />
    public partial class TrainArrQualificationLifecycle : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<Guid>(
                name: "LifecyclePublicationId",
                table: "trainarr_qualification_issues",
                type: "uuid",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "LifecycleReason",
                table: "trainarr_qualification_issues",
                type: "character varying(1024)",
                maxLength: 1024,
                nullable: true);

            migrationBuilder.AddColumn<DateTimeOffset>(
                name: "StatusChangedAt",
                table: "trainarr_qualification_issues",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_trainarr_qualification_issues_TenantId_LifecyclePublication~",
                table: "trainarr_qualification_issues",
                columns: new[] { "TenantId", "LifecyclePublicationId" },
                unique: true,
                filter: "\"LifecyclePublicationId\" IS NOT NULL");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_trainarr_qualification_issues_TenantId_LifecyclePublication~",
                table: "trainarr_qualification_issues");

            migrationBuilder.DropColumn(
                name: "LifecyclePublicationId",
                table: "trainarr_qualification_issues");

            migrationBuilder.DropColumn(
                name: "LifecycleReason",
                table: "trainarr_qualification_issues");

            migrationBuilder.DropColumn(
                name: "StatusChangedAt",
                table: "trainarr_qualification_issues");
        }
    }
}
