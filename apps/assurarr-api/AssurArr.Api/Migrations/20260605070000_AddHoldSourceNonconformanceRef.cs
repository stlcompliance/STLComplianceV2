using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace AssurArr.Api.Migrations;

public partial class AddHoldSourceNonconformanceRef : Migration
{
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.AddColumn<DateTimeOffset>(
            name: "DiscoveredAt",
            table: "assurarr_nonconformances",
            type: "timestamp with time zone",
            nullable: true);

        migrationBuilder.AddColumn<Guid>(
            name: "DiscoveredByPersonId",
            table: "assurarr_nonconformances",
            type: "uuid",
            nullable: true);

        migrationBuilder.AddColumn<string[]>(
            name: "AuditTrail",
            table: "assurarr_nonconformances",
            type: "text[]",
            nullable: false,
            defaultValue: new string[0]);

        migrationBuilder.AddColumn<string[]>(
            name: "CapaRefs",
            table: "assurarr_nonconformances",
            type: "text[]",
            nullable: false,
            defaultValue: new string[0]);

        migrationBuilder.AddColumn<string[]>(
            name: "ComplianceRefs",
            table: "assurarr_nonconformances",
            type: "text[]",
            nullable: false,
            defaultValue: new string[0]);

        migrationBuilder.AddColumn<string[]>(
            name: "ContainmentRefs",
            table: "assurarr_nonconformances",
            type: "text[]",
            nullable: false,
            defaultValue: new string[0]);

        migrationBuilder.AddColumn<string[]>(
            name: "HoldRefs",
            table: "assurarr_nonconformances",
            type: "text[]",
            nullable: false,
            defaultValue: new string[0]);

        migrationBuilder.AddColumn<string[]>(
            name: "AffectedItemRefs",
            table: "assurarr_nonconformances",
            type: "text[]",
            nullable: false,
            defaultValue: new string[0]);

        migrationBuilder.AddColumn<string[]>(
            name: "AffectedAssetRefs",
            table: "assurarr_nonconformances",
            type: "text[]",
            nullable: false,
            defaultValue: new string[0]);

        migrationBuilder.AddColumn<string[]>(
            name: "AffectedOrderRefs",
            table: "assurarr_nonconformances",
            type: "text[]",
            nullable: false,
            defaultValue: new string[0]);

        migrationBuilder.AddColumn<string[]>(
            name: "AffectedSupplierRefs",
            table: "assurarr_nonconformances",
            type: "text[]",
            nullable: false,
            defaultValue: new string[0]);

        migrationBuilder.AddColumn<string[]>(
            name: "AffectedCustomerRefs",
            table: "assurarr_nonconformances",
            type: "text[]",
            nullable: false,
            defaultValue: new string[0]);

        migrationBuilder.AddColumn<string[]>(
            name: "AffectedShipmentRefs",
            table: "assurarr_nonconformances",
            type: "text[]",
            nullable: false,
            defaultValue: new string[0]);

        migrationBuilder.AddColumn<string>(
            name: "FinancialImpactSnapshot",
            table: "assurarr_nonconformances",
            type: "character varying(4000)",
            maxLength: 4000,
            nullable: true);

        migrationBuilder.AddColumn<string[]>(
            name: "DispositionRefs",
            table: "assurarr_nonconformances",
            type: "text[]",
            nullable: false,
            defaultValue: new string[0]);

        migrationBuilder.AddColumn<Guid>(
            name: "StaffArrLocationId",
            table: "assurarr_nonconformances",
            type: "uuid",
            nullable: true);

        migrationBuilder.AddColumn<Guid>(
            name: "StaffArrSiteId",
            table: "assurarr_nonconformances",
            type: "uuid",
            nullable: true);

        migrationBuilder.AddColumn<Guid>(
            name: "StaffArrLocationId",
            table: "assurarr_quality_holds",
            type: "uuid",
            nullable: true);

        migrationBuilder.AddColumn<Guid>(
            name: "StaffArrSiteId",
            table: "assurarr_quality_holds",
            type: "uuid",
            nullable: true);

        migrationBuilder.AddColumn<string[]>(
            name: "AuditTrail",
            table: "assurarr_quality_holds",
            type: "text[]",
            nullable: false,
            defaultValue: new string[0]);

        migrationBuilder.AddColumn<string>(
            name: "SourceNonconformanceRef",
            table: "assurarr_quality_holds",
            type: "character varying(256)",
            maxLength: 256,
            nullable: true);
    }

    protected override void Down(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.DropColumn(
            name: "AuditTrail",
            table: "assurarr_quality_holds");

        migrationBuilder.DropColumn(
            name: "StaffArrSiteId",
            table: "assurarr_quality_holds");

        migrationBuilder.DropColumn(
            name: "StaffArrLocationId",
            table: "assurarr_quality_holds");

        migrationBuilder.DropColumn(
            name: "SourceNonconformanceRef",
            table: "assurarr_quality_holds");

        migrationBuilder.DropColumn(
            name: "StaffArrSiteId",
            table: "assurarr_nonconformances");

        migrationBuilder.DropColumn(
            name: "StaffArrLocationId",
            table: "assurarr_nonconformances");

        migrationBuilder.DropColumn(
            name: "DispositionRefs",
            table: "assurarr_nonconformances");

        migrationBuilder.DropColumn(
            name: "FinancialImpactSnapshot",
            table: "assurarr_nonconformances");

        migrationBuilder.DropColumn(
            name: "ContainmentRefs",
            table: "assurarr_nonconformances");

        migrationBuilder.DropColumn(
            name: "HoldRefs",
            table: "assurarr_nonconformances");

        migrationBuilder.DropColumn(
            name: "AffectedShipmentRefs",
            table: "assurarr_nonconformances");

        migrationBuilder.DropColumn(
            name: "AffectedCustomerRefs",
            table: "assurarr_nonconformances");

        migrationBuilder.DropColumn(
            name: "AffectedSupplierRefs",
            table: "assurarr_nonconformances");

        migrationBuilder.DropColumn(
            name: "AffectedOrderRefs",
            table: "assurarr_nonconformances");

        migrationBuilder.DropColumn(
            name: "AffectedAssetRefs",
            table: "assurarr_nonconformances");

        migrationBuilder.DropColumn(
            name: "AffectedItemRefs",
            table: "assurarr_nonconformances");

        migrationBuilder.DropColumn(
            name: "ComplianceRefs",
            table: "assurarr_nonconformances");

        migrationBuilder.DropColumn(
            name: "CapaRefs",
            table: "assurarr_nonconformances");

        migrationBuilder.DropColumn(
            name: "AuditTrail",
            table: "assurarr_nonconformances");

        migrationBuilder.DropColumn(
            name: "DiscoveredByPersonId",
            table: "assurarr_nonconformances");

        migrationBuilder.DropColumn(
            name: "DiscoveredAt",
            table: "assurarr_nonconformances");
    }
}
