using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace RoutArr.Api.Migrations
{
    /// <inheritdoc />
    public partial class RoutArrTripCaptureAttachments : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "RequireDeliveryProofPhotoBeforeComplete",
                table: "routarr_tenant_trip_execution_settings",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "RequireDeliverySignatureBeforeComplete",
                table: "routarr_tenant_trip_execution_settings",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "RequirePickupProofPhotoBeforeStart",
                table: "routarr_tenant_trip_execution_settings",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "RequirePostTripDvirPhotoBeforeComplete",
                table: "routarr_tenant_trip_execution_settings",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "RequirePreTripDvirPhotoBeforeStart",
                table: "routarr_tenant_trip_execution_settings",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.CreateTable(
                name: "routarr_trip_capture_attachments",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    TenantId = table.Column<Guid>(type: "uuid", nullable: false),
                    TripId = table.Column<Guid>(type: "uuid", nullable: false),
                    SubjectType = table.Column<string>(type: "character varying(16)", maxLength: 16, nullable: false),
                    SubjectId = table.Column<Guid>(type: "uuid", nullable: false),
                    AttachmentKind = table.Column<string>(type: "character varying(32)", maxLength: 32, nullable: false),
                    FileName = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: false),
                    ContentType = table.Column<string>(type: "character varying(128)", maxLength: 128, nullable: false),
                    SizeBytes = table.Column<long>(type: "bigint", nullable: false),
                    StorageKey = table.Column<string>(type: "character varying(512)", maxLength: 512, nullable: false),
                    Notes = table.Column<string>(type: "character varying(1024)", maxLength: 1024, nullable: true),
                    CapturedByPersonId = table.Column<string>(type: "character varying(128)", maxLength: 128, nullable: false),
                    CreatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_routarr_trip_capture_attachments", x => x.Id);
                    table.ForeignKey(
                        name: "FK_routarr_trip_capture_attachments_routarr_trips_TripId",
                        column: x => x.TripId,
                        principalTable: "routarr_trips",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_routarr_trip_capture_attachments_TenantId",
                table: "routarr_trip_capture_attachments",
                column: "TenantId");

            migrationBuilder.CreateIndex(
                name: "IX_routarr_trip_capture_attachments_TenantId_TripId",
                table: "routarr_trip_capture_attachments",
                columns: new[] { "TenantId", "TripId" });

            migrationBuilder.CreateIndex(
                name: "IX_routarr_trip_capture_attachments_TenantId_TripId_Attachment~",
                table: "routarr_trip_capture_attachments",
                columns: new[] { "TenantId", "TripId", "AttachmentKind", "CreatedAt" });

            migrationBuilder.CreateIndex(
                name: "IX_routarr_trip_capture_attachments_TenantId_TripId_SubjectTyp~",
                table: "routarr_trip_capture_attachments",
                columns: new[] { "TenantId", "TripId", "SubjectType", "SubjectId" });

            migrationBuilder.CreateIndex(
                name: "IX_routarr_trip_capture_attachments_TripId",
                table: "routarr_trip_capture_attachments",
                column: "TripId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "routarr_trip_capture_attachments");

            migrationBuilder.DropColumn(
                name: "RequireDeliveryProofPhotoBeforeComplete",
                table: "routarr_tenant_trip_execution_settings");

            migrationBuilder.DropColumn(
                name: "RequireDeliverySignatureBeforeComplete",
                table: "routarr_tenant_trip_execution_settings");

            migrationBuilder.DropColumn(
                name: "RequirePickupProofPhotoBeforeStart",
                table: "routarr_tenant_trip_execution_settings");

            migrationBuilder.DropColumn(
                name: "RequirePostTripDvirPhotoBeforeComplete",
                table: "routarr_tenant_trip_execution_settings");

            migrationBuilder.DropColumn(
                name: "RequirePreTripDvirPhotoBeforeStart",
                table: "routarr_tenant_trip_execution_settings");
        }
    }
}
