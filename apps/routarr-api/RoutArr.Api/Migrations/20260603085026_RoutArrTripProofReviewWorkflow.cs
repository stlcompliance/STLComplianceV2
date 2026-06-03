using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace RoutArr.Api.Migrations
{
    /// <inheritdoc />
    public partial class RoutArrTripProofReviewWorkflow : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "ReviewNotes",
                table: "routarr_trip_proof_records",
                type: "character varying(1024)",
                maxLength: 1024,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "ReviewStatus",
                table: "routarr_trip_proof_records",
                type: "character varying(32)",
                maxLength: 32,
                nullable: false,
                defaultValue: "pending_review");

            migrationBuilder.AddColumn<DateTimeOffset>(
                name: "ReviewedAt",
                table: "routarr_trip_proof_records",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "ReviewedByPersonId",
                table: "routarr_trip_proof_records",
                type: "character varying(128)",
                maxLength: 128,
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_routarr_trip_proof_records_TenantId_TripId_ReviewStatus",
                table: "routarr_trip_proof_records",
                columns: new[] { "TenantId", "TripId", "ReviewStatus" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_routarr_trip_proof_records_TenantId_TripId_ReviewStatus",
                table: "routarr_trip_proof_records");

            migrationBuilder.DropColumn(
                name: "ReviewNotes",
                table: "routarr_trip_proof_records");

            migrationBuilder.DropColumn(
                name: "ReviewStatus",
                table: "routarr_trip_proof_records");

            migrationBuilder.DropColumn(
                name: "ReviewedAt",
                table: "routarr_trip_proof_records");

            migrationBuilder.DropColumn(
                name: "ReviewedByPersonId",
                table: "routarr_trip_proof_records");
        }
    }
}
