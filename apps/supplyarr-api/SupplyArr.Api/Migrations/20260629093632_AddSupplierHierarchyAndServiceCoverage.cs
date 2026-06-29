using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SupplyArr.Api.Migrations
{
    /// <inheritdoc />
    public partial class AddSupplierHierarchyAndServiceCoverage : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "AddressLine1",
                table: "supplyarr_external_parties",
                type: "character varying(256)",
                maxLength: 256,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "AddressLine2",
                table: "supplyarr_external_parties",
                type: "character varying(256)",
                maxLength: 256,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "CountryCode",
                table: "supplyarr_external_parties",
                type: "character varying(2)",
                maxLength: 2,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "Locality",
                table: "supplyarr_external_parties",
                type: "character varying(128)",
                maxLength: 128,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<Guid>(
                name: "ParentExternalPartyId",
                table: "supplyarr_external_parties",
                type: "uuid",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "PostalCode",
                table: "supplyarr_external_parties",
                type: "character varying(32)",
                maxLength: 32,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "RegionCode",
                table: "supplyarr_external_parties",
                type: "character varying(64)",
                maxLength: 64,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "ServiceTypesJson",
                table: "supplyarr_external_parties",
                type: "character varying(2048)",
                maxLength: 2048,
                nullable: false,
                defaultValue: "[]");

            migrationBuilder.AddColumn<string>(
                name: "UnitKind",
                table: "supplyarr_external_parties",
                type: "character varying(32)",
                maxLength: 32,
                nullable: false,
                defaultValue: "identity");

            migrationBuilder.CreateIndex(
                name: "IX_supplyarr_external_parties_ParentExternalPartyId",
                table: "supplyarr_external_parties",
                column: "ParentExternalPartyId");

            migrationBuilder.CreateIndex(
                name: "IX_supplyarr_external_parties_TenantId_ParentExternalPartyId",
                table: "supplyarr_external_parties",
                columns: new[] { "TenantId", "ParentExternalPartyId" });

            migrationBuilder.AddForeignKey(
                name: "FK_supplyarr_external_parties_supplyarr_external_parties_Paren~",
                table: "supplyarr_external_parties",
                column: "ParentExternalPartyId",
                principalTable: "supplyarr_external_parties",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_supplyarr_external_parties_supplyarr_external_parties_Paren~",
                table: "supplyarr_external_parties");

            migrationBuilder.DropIndex(
                name: "IX_supplyarr_external_parties_ParentExternalPartyId",
                table: "supplyarr_external_parties");

            migrationBuilder.DropIndex(
                name: "IX_supplyarr_external_parties_TenantId_ParentExternalPartyId",
                table: "supplyarr_external_parties");

            migrationBuilder.DropColumn(
                name: "AddressLine1",
                table: "supplyarr_external_parties");

            migrationBuilder.DropColumn(
                name: "AddressLine2",
                table: "supplyarr_external_parties");

            migrationBuilder.DropColumn(
                name: "CountryCode",
                table: "supplyarr_external_parties");

            migrationBuilder.DropColumn(
                name: "Locality",
                table: "supplyarr_external_parties");

            migrationBuilder.DropColumn(
                name: "ParentExternalPartyId",
                table: "supplyarr_external_parties");

            migrationBuilder.DropColumn(
                name: "PostalCode",
                table: "supplyarr_external_parties");

            migrationBuilder.DropColumn(
                name: "RegionCode",
                table: "supplyarr_external_parties");

            migrationBuilder.DropColumn(
                name: "ServiceTypesJson",
                table: "supplyarr_external_parties");

            migrationBuilder.DropColumn(
                name: "UnitKind",
                table: "supplyarr_external_parties");
        }
    }
}
