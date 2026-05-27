using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace MaintainArr.Api.Migrations
{
    /// <inheritdoc />
    public partial class MaintainArrAssetRegistryFoundations : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "maintainarr_asset_classes",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    TenantId = table.Column<Guid>(type: "uuid", nullable: false),
                    ClassKey = table.Column<string>(type: "character varying(128)", maxLength: 128, nullable: false),
                    Name = table.Column<string>(type: "character varying(128)", maxLength: 128, nullable: false),
                    Description = table.Column<string>(type: "character varying(512)", maxLength: 512, nullable: false),
                    Status = table.Column<string>(type: "character varying(32)", maxLength: 32, nullable: false),
                    CreatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_maintainarr_asset_classes", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "maintainarr_audit_events",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    TenantId = table.Column<Guid>(type: "uuid", nullable: false),
                    ActorUserId = table.Column<Guid>(type: "uuid", nullable: true),
                    Action = table.Column<string>(type: "character varying(128)", maxLength: 128, nullable: false),
                    TargetType = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: false),
                    TargetId = table.Column<string>(type: "character varying(128)", maxLength: 128, nullable: true),
                    Result = table.Column<string>(type: "character varying(32)", maxLength: 32, nullable: false),
                    ReasonCode = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: true),
                    CorrelationId = table.Column<Guid>(type: "uuid", nullable: false),
                    OccurredAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_maintainarr_audit_events", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "maintainarr_asset_types",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    TenantId = table.Column<Guid>(type: "uuid", nullable: false),
                    AssetClassId = table.Column<Guid>(type: "uuid", nullable: false),
                    TypeKey = table.Column<string>(type: "character varying(128)", maxLength: 128, nullable: false),
                    Name = table.Column<string>(type: "character varying(128)", maxLength: 128, nullable: false),
                    Description = table.Column<string>(type: "character varying(512)", maxLength: 512, nullable: false),
                    Status = table.Column<string>(type: "character varying(32)", maxLength: 32, nullable: false),
                    CreatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_maintainarr_asset_types", x => x.Id);
                    table.ForeignKey(
                        name: "FK_maintainarr_asset_types_maintainarr_asset_classes_AssetClas~",
                        column: x => x.AssetClassId,
                        principalTable: "maintainarr_asset_classes",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "maintainarr_assets",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    TenantId = table.Column<Guid>(type: "uuid", nullable: false),
                    AssetTypeId = table.Column<Guid>(type: "uuid", nullable: false),
                    AssetTag = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: false),
                    Name = table.Column<string>(type: "character varying(128)", maxLength: 128, nullable: false),
                    Description = table.Column<string>(type: "character varying(512)", maxLength: 512, nullable: false),
                    LifecycleStatus = table.Column<string>(type: "character varying(32)", maxLength: 32, nullable: false),
                    SiteRef = table.Column<string>(type: "character varying(128)", maxLength: 128, nullable: true),
                    CreatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_maintainarr_assets", x => x.Id);
                    table.ForeignKey(
                        name: "FK_maintainarr_assets_maintainarr_asset_types_AssetTypeId",
                        column: x => x.AssetTypeId,
                        principalTable: "maintainarr_asset_types",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_maintainarr_asset_classes_TenantId",
                table: "maintainarr_asset_classes",
                column: "TenantId");

            migrationBuilder.CreateIndex(
                name: "IX_maintainarr_asset_classes_TenantId_ClassKey",
                table: "maintainarr_asset_classes",
                columns: new[] { "TenantId", "ClassKey" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_maintainarr_asset_types_AssetClassId",
                table: "maintainarr_asset_types",
                column: "AssetClassId");

            migrationBuilder.CreateIndex(
                name: "IX_maintainarr_asset_types_TenantId",
                table: "maintainarr_asset_types",
                column: "TenantId");

            migrationBuilder.CreateIndex(
                name: "IX_maintainarr_asset_types_TenantId_TypeKey",
                table: "maintainarr_asset_types",
                columns: new[] { "TenantId", "TypeKey" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_maintainarr_assets_AssetTypeId",
                table: "maintainarr_assets",
                column: "AssetTypeId");

            migrationBuilder.CreateIndex(
                name: "IX_maintainarr_assets_TenantId",
                table: "maintainarr_assets",
                column: "TenantId");

            migrationBuilder.CreateIndex(
                name: "IX_maintainarr_assets_TenantId_AssetTag",
                table: "maintainarr_assets",
                columns: new[] { "TenantId", "AssetTag" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_maintainarr_assets_TenantId_AssetTypeId",
                table: "maintainarr_assets",
                columns: new[] { "TenantId", "AssetTypeId" });

            migrationBuilder.CreateIndex(
                name: "IX_maintainarr_audit_events_TenantId",
                table: "maintainarr_audit_events",
                column: "TenantId");

            migrationBuilder.CreateIndex(
                name: "IX_maintainarr_audit_events_TenantId_OccurredAt",
                table: "maintainarr_audit_events",
                columns: new[] { "TenantId", "OccurredAt" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "maintainarr_assets");

            migrationBuilder.DropTable(
                name: "maintainarr_audit_events");

            migrationBuilder.DropTable(
                name: "maintainarr_asset_types");

            migrationBuilder.DropTable(
                name: "maintainarr_asset_classes");
        }
    }
}
