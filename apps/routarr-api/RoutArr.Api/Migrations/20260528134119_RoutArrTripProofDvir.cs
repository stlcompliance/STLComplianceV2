using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace RoutArr.Api.Migrations
{
    /// <inheritdoc />
    public partial class RoutArrTripProofDvir : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "routarr_trip_dvir_inspections",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    TenantId = table.Column<Guid>(type: "uuid", nullable: false),
                    TripId = table.Column<Guid>(type: "uuid", nullable: false),
                    Phase = table.Column<string>(type: "character varying(32)", maxLength: 32, nullable: false),
                    VehicleRefKey = table.Column<string>(type: "character varying(128)", maxLength: 128, nullable: false),
                    Result = table.Column<string>(type: "character varying(32)", maxLength: 32, nullable: false),
                    OdometerReading = table.Column<long>(type: "bigint", nullable: true),
                    DefectNotes = table.Column<string>(type: "character varying(1024)", maxLength: 1024, nullable: false),
                    SubmittedByPersonId = table.Column<string>(type: "character varying(128)", maxLength: 128, nullable: false),
                    SubmittedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    CreatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_routarr_trip_dvir_inspections", x => x.Id);
                    table.ForeignKey(
                        name: "FK_routarr_trip_dvir_inspections_routarr_trips_TripId",
                        column: x => x.TripId,
                        principalTable: "routarr_trips",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "routarr_trip_proof_records",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    TenantId = table.Column<Guid>(type: "uuid", nullable: false),
                    TripId = table.Column<Guid>(type: "uuid", nullable: false),
                    ProofType = table.Column<string>(type: "character varying(32)", maxLength: 32, nullable: false),
                    CapturedByPersonId = table.Column<string>(type: "character varying(128)", maxLength: 128, nullable: false),
                    VehicleRefKey = table.Column<string>(type: "character varying(128)", maxLength: 128, nullable: true),
                    ReferenceKey = table.Column<string>(type: "character varying(128)", maxLength: 128, nullable: false),
                    Notes = table.Column<string>(type: "character varying(1024)", maxLength: 1024, nullable: false),
                    CapturedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    CreatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_routarr_trip_proof_records", x => x.Id);
                    table.ForeignKey(
                        name: "FK_routarr_trip_proof_records_routarr_trips_TripId",
                        column: x => x.TripId,
                        principalTable: "routarr_trips",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_routarr_trip_dvir_inspections_TenantId",
                table: "routarr_trip_dvir_inspections",
                column: "TenantId");

            migrationBuilder.CreateIndex(
                name: "IX_routarr_trip_dvir_inspections_TenantId_TripId",
                table: "routarr_trip_dvir_inspections",
                columns: new[] { "TenantId", "TripId" });

            migrationBuilder.CreateIndex(
                name: "IX_routarr_trip_dvir_inspections_TenantId_TripId_Phase",
                table: "routarr_trip_dvir_inspections",
                columns: new[] { "TenantId", "TripId", "Phase" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_routarr_trip_dvir_inspections_TripId",
                table: "routarr_trip_dvir_inspections",
                column: "TripId");

            migrationBuilder.CreateIndex(
                name: "IX_routarr_trip_proof_records_TenantId",
                table: "routarr_trip_proof_records",
                column: "TenantId");

            migrationBuilder.CreateIndex(
                name: "IX_routarr_trip_proof_records_TenantId_TripId",
                table: "routarr_trip_proof_records",
                columns: new[] { "TenantId", "TripId" });

            migrationBuilder.CreateIndex(
                name: "IX_routarr_trip_proof_records_TenantId_TripId_CapturedAt",
                table: "routarr_trip_proof_records",
                columns: new[] { "TenantId", "TripId", "CapturedAt" });

            migrationBuilder.CreateIndex(
                name: "IX_routarr_trip_proof_records_TripId",
                table: "routarr_trip_proof_records",
                column: "TripId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "routarr_trip_dvir_inspections");

            migrationBuilder.DropTable(
                name: "routarr_trip_proof_records");
        }
    }
}
