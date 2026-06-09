using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace NexArr.Api.Migrations
{
    /// <inheritdoc />
    public partial class NexArrMasterReferenceCsvImport : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<Guid>(
                name: "TargetDatasetId",
                table: "staging_records",
                type: "uuid",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_staging_records_TargetDatasetId",
                table: "staging_records",
                column: "TargetDatasetId");

            migrationBuilder.AddForeignKey(
                name: "FK_staging_records_reference_datasets_TargetDatasetId",
                table: "staging_records",
                column: "TargetDatasetId",
                principalTable: "reference_datasets",
                principalColumn: "Id",
                onDelete: ReferentialAction.SetNull);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_staging_records_reference_datasets_TargetDatasetId",
                table: "staging_records");

            migrationBuilder.DropIndex(
                name: "IX_staging_records_TargetDatasetId",
                table: "staging_records");

            migrationBuilder.DropColumn(
                name: "TargetDatasetId",
                table: "staging_records");
        }
    }
}
