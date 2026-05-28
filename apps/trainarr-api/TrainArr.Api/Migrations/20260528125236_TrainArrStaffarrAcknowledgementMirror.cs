using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace TrainArr.Api.Migrations
{
    /// <inheritdoc />
    public partial class TrainArrStaffarrAcknowledgementMirror : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<DateTimeOffset>(
                name: "StaffarrAcknowledgementAt",
                table: "trainarr_training_assignments",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "StaffarrAcknowledgementRequestId",
                table: "trainarr_training_assignments",
                type: "uuid",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "StaffarrAcknowledgementStatus",
                table: "trainarr_training_assignments",
                type: "character varying(32)",
                maxLength: 32,
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "StaffarrAcknowledgementAt",
                table: "trainarr_training_assignments");

            migrationBuilder.DropColumn(
                name: "StaffarrAcknowledgementRequestId",
                table: "trainarr_training_assignments");

            migrationBuilder.DropColumn(
                name: "StaffarrAcknowledgementStatus",
                table: "trainarr_training_assignments");
        }
    }
}
