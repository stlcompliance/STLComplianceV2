using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace StaffArr.Api.Migrations
{
    /// <inheritdoc />
    public partial class StaffArrOrganizationStructureCutover : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_staffarr_people_staffarr_org_units_HomeBaseLocationId",
                table: "staffarr_people");

            migrationBuilder.AddColumn<string>(
                name: "ArchiveReason",
                table: "staffarr_org_units",
                type: "character varying(512)",
                maxLength: 512,
                nullable: true);

            migrationBuilder.AddColumn<DateTimeOffset>(
                name: "ArchivedAt",
                table: "staffarr_org_units",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "ArchivedByUserId",
                table: "staffarr_org_units",
                type: "uuid",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Code",
                table: "staffarr_org_units",
                type: "character varying(64)",
                maxLength: 64,
                nullable: true);

            migrationBuilder.CreateTable(
                name: "staffarr_internal_locations",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    TenantId = table.Column<Guid>(type: "uuid", nullable: false),
                    LocationNumber = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: false),
                    Name = table.Column<string>(type: "character varying(128)", maxLength: 128, nullable: false),
                    Description = table.Column<string>(type: "character varying(512)", maxLength: 512, nullable: true),
                    LocationType = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: false),
                    ParentLocationId = table.Column<Guid>(type: "uuid", nullable: true),
                    SiteOrgUnitId = table.Column<Guid>(type: "uuid", nullable: true),
                    Status = table.Column<string>(type: "character varying(32)", maxLength: 32, nullable: false),
                    AllowedProductUsage = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: false),
                    ArchivedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    ArchivedByUserId = table.Column<Guid>(type: "uuid", nullable: true),
                    ArchiveReason = table.Column<string>(type: "character varying(512)", maxLength: 512, nullable: true),
                    CreatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_staffarr_internal_locations", x => x.Id);
                    table.ForeignKey(
                        name: "FK_staffarr_internal_locations_staffarr_internal_locations_Par~",
                        column: x => x.ParentLocationId,
                        principalTable: "staffarr_internal_locations",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_staffarr_internal_locations_staffarr_org_units_SiteOrgUnitId",
                        column: x => x.SiteOrgUnitId,
                        principalTable: "staffarr_org_units",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_staffarr_internal_locations_staffarr_people_ArchivedByUserId",
                        column: x => x.ArchivedByUserId,
                        principalTable: "staffarr_people",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateIndex(
                name: "IX_staffarr_org_units_ArchivedByUserId",
                table: "staffarr_org_units",
                column: "ArchivedByUserId");

            migrationBuilder.CreateIndex(
                name: "IX_staffarr_org_units_TenantId_Code",
                table: "staffarr_org_units",
                columns: new[] { "TenantId", "Code" },
                unique: true,
                filter: "\"Code\" IS NOT NULL AND \"ParentOrgUnitId\" IS NULL");

            migrationBuilder.CreateIndex(
                name: "IX_staffarr_org_units_TenantId_ParentOrgUnitId_Code",
                table: "staffarr_org_units",
                columns: new[] { "TenantId", "ParentOrgUnitId", "Code" },
                unique: true,
                filter: "\"Code\" IS NOT NULL AND \"ParentOrgUnitId\" IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "IX_staffarr_internal_locations_ArchivedByUserId",
                table: "staffarr_internal_locations",
                column: "ArchivedByUserId");

            migrationBuilder.CreateIndex(
                name: "IX_staffarr_internal_locations_ParentLocationId",
                table: "staffarr_internal_locations",
                column: "ParentLocationId");

            migrationBuilder.CreateIndex(
                name: "IX_staffarr_internal_locations_SiteOrgUnitId",
                table: "staffarr_internal_locations",
                column: "SiteOrgUnitId");

            migrationBuilder.CreateIndex(
                name: "IX_staffarr_internal_locations_Status",
                table: "staffarr_internal_locations",
                column: "Status");

            migrationBuilder.CreateIndex(
                name: "IX_staffarr_internal_locations_TenantId",
                table: "staffarr_internal_locations",
                column: "TenantId");

            migrationBuilder.CreateIndex(
                name: "IX_staffarr_internal_locations_TenantId_LocationNumber",
                table: "staffarr_internal_locations",
                columns: new[] { "TenantId", "LocationNumber" },
                unique: true,
                filter: "\"ParentLocationId\" IS NULL");

            migrationBuilder.CreateIndex(
                name: "IX_staffarr_internal_locations_TenantId_ParentLocationId_Locat~",
                table: "staffarr_internal_locations",
                columns: new[] { "TenantId", "ParentLocationId", "LocationNumber" },
                unique: true,
                filter: "\"ParentLocationId\" IS NOT NULL");

            migrationBuilder.Sql("""
UPDATE staffarr_org_units
SET "Code" = CASE
    WHEN "UnitType" = 'site' THEN 'SITE-' || replace("Id"::text, '-', '')
    ELSE 'OU-' || replace("Id"::text, '-', '')
END
WHERE "Code" IS NULL;
""");

            migrationBuilder.Sql("""
WITH RECURSIVE site_tree AS (
    SELECT
        ou."Id",
        ou."TenantId",
        ou."ParentOrgUnitId",
        ou."UnitType",
        ou."Name",
        ou."Description",
        ou."Status",
        ou."ArchivedAt",
        ou."ArchivedByUserId",
        ou."ArchiveReason",
        ou."CreatedAt",
        ou."UpdatedAt",
        ou."Id" AS "SiteOrgUnitId"
    FROM staffarr_org_units ou
    WHERE ou."UnitType" = 'site'
    UNION ALL
    SELECT
        child."Id",
        child."TenantId",
        child."ParentOrgUnitId",
        child."UnitType",
        child."Name",
        child."Description",
        child."Status",
        child."ArchivedAt",
        child."ArchivedByUserId",
        child."ArchiveReason",
        child."CreatedAt",
        child."UpdatedAt",
        parent."SiteOrgUnitId"
    FROM staffarr_org_units child
    JOIN site_tree parent
        ON child."TenantId" = parent."TenantId"
       AND child."ParentOrgUnitId" = parent."Id"
),
referenced_home_base_units AS (
    SELECT DISTINCT p."TenantId", p."HomeBaseLocationId" AS "Id"
    FROM staffarr_people p
    WHERE p."HomeBaseLocationId" IS NOT NULL
),
extra_units AS (
    SELECT
        ou."Id",
        ou."TenantId",
        ou."ParentOrgUnitId",
        ou."UnitType",
        ou."Name",
        ou."Description",
        ou."Status",
        ou."ArchivedAt",
        ou."ArchivedByUserId",
        ou."ArchiveReason",
        ou."CreatedAt",
        ou."UpdatedAt",
        NULL::uuid AS "SiteOrgUnitId"
    FROM staffarr_org_units ou
    JOIN referenced_home_base_units referenced
        ON referenced."TenantId" = ou."TenantId"
       AND referenced."Id" = ou."Id"
    LEFT JOIN site_tree site_seed
        ON site_seed."TenantId" = ou."TenantId"
       AND site_seed."Id" = ou."Id"
    WHERE site_seed."Id" IS NULL
),
seeded_units AS (
    SELECT * FROM site_tree
    UNION ALL
    SELECT * FROM extra_units
),
seeded_ids AS (
    SELECT DISTINCT "TenantId", "Id"
    FROM seeded_units
)
INSERT INTO staffarr_internal_locations (
    "Id",
    "TenantId",
    "LocationNumber",
    "Name",
    "Description",
    "LocationType",
    "ParentLocationId",
    "SiteOrgUnitId",
    "Status",
    "AllowedProductUsage",
    "ArchivedAt",
    "ArchivedByUserId",
    "ArchiveReason",
    "CreatedAt",
    "UpdatedAt"
)
SELECT
    unit."Id",
    unit."TenantId",
    'LOC-' || replace(unit."Id"::text, '-', ''),
    unit."Name",
    unit."Description",
    CASE
        WHEN unit."UnitType" IN (
            'site',
            'department',
            'team',
            'position',
            'company',
            'division',
            'region',
            'business_unit',
            'cost_center',
            'other'
        ) THEN unit."UnitType"
        ELSE 'other'
    END,
    CASE
        WHEN parent_seed."Id" IS NULL THEN NULL
        ELSE unit."ParentOrgUnitId"
    END,
    unit."SiteOrgUnitId",
    CASE
        WHEN unit."Status" = 'archived' THEN 'archived'
        ELSE unit."Status"
    END,
    'all',
    unit."ArchivedAt",
    unit."ArchivedByUserId",
    unit."ArchiveReason",
    unit."CreatedAt",
    unit."UpdatedAt"
FROM seeded_units unit
LEFT JOIN seeded_ids parent_seed
    ON parent_seed."TenantId" = unit."TenantId"
   AND parent_seed."Id" = unit."ParentOrgUnitId"
ON CONFLICT ("Id") DO NOTHING;
""");

            migrationBuilder.AddForeignKey(
                name: "FK_staffarr_org_units_staffarr_people_ArchivedByUserId",
                table: "staffarr_org_units",
                column: "ArchivedByUserId",
                principalTable: "staffarr_people",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_staffarr_people_staffarr_internal_locations_HomeBaseLocatio~",
                table: "staffarr_people",
                column: "HomeBaseLocationId",
                principalTable: "staffarr_internal_locations",
                principalColumn: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_staffarr_org_units_staffarr_people_ArchivedByUserId",
                table: "staffarr_org_units");

            migrationBuilder.DropForeignKey(
                name: "FK_staffarr_people_staffarr_internal_locations_HomeBaseLocatio~",
                table: "staffarr_people");

            migrationBuilder.DropTable(
                name: "staffarr_internal_locations");

            migrationBuilder.DropIndex(
                name: "IX_staffarr_org_units_ArchivedByUserId",
                table: "staffarr_org_units");

            migrationBuilder.DropIndex(
                name: "IX_staffarr_org_units_TenantId_Code",
                table: "staffarr_org_units");

            migrationBuilder.DropIndex(
                name: "IX_staffarr_org_units_TenantId_ParentOrgUnitId_Code",
                table: "staffarr_org_units");

            migrationBuilder.DropColumn(
                name: "ArchiveReason",
                table: "staffarr_org_units");

            migrationBuilder.DropColumn(
                name: "ArchivedAt",
                table: "staffarr_org_units");

            migrationBuilder.DropColumn(
                name: "ArchivedByUserId",
                table: "staffarr_org_units");

            migrationBuilder.DropColumn(
                name: "Code",
                table: "staffarr_org_units");

            migrationBuilder.AddForeignKey(
                name: "FK_staffarr_people_staffarr_org_units_HomeBaseLocationId",
                table: "staffarr_people",
                column: "HomeBaseLocationId",
                principalTable: "staffarr_org_units",
                principalColumn: "Id");
        }
    }
}
