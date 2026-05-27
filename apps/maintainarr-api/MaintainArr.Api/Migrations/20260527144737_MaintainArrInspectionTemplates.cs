using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace MaintainArr.Api.Migrations
{
    /// <inheritdoc />
    public partial class MaintainArrInspectionTemplates : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "maintainarr_inspection_templates",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    TenantId = table.Column<Guid>(type: "uuid", nullable: false),
                    TemplateKey = table.Column<string>(type: "character varying(128)", maxLength: 128, nullable: false),
                    Name = table.Column<string>(type: "character varying(128)", maxLength: 128, nullable: false),
                    Description = table.Column<string>(type: "character varying(512)", maxLength: 512, nullable: false),
                    Version = table.Column<int>(type: "integer", nullable: false),
                    Status = table.Column<string>(type: "character varying(32)", maxLength: 32, nullable: false),
                    CreatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_maintainarr_inspection_templates", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "maintainarr_inspection_template_asset_types",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    TenantId = table.Column<Guid>(type: "uuid", nullable: false),
                    InspectionTemplateId = table.Column<Guid>(type: "uuid", nullable: false),
                    AssetTypeId = table.Column<Guid>(type: "uuid", nullable: false),
                    CreatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_maintainarr_inspection_template_asset_types", x => x.Id);
                    table.ForeignKey(
                        name: "FK_maintainarr_inspection_template_asset_types_maintainarr_ass~",
                        column: x => x.AssetTypeId,
                        principalTable: "maintainarr_asset_types",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_maintainarr_inspection_template_asset_types_maintainarr_ins~",
                        column: x => x.InspectionTemplateId,
                        principalTable: "maintainarr_inspection_templates",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "maintainarr_inspection_template_categories",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    TenantId = table.Column<Guid>(type: "uuid", nullable: false),
                    InspectionTemplateId = table.Column<Guid>(type: "uuid", nullable: false),
                    CategoryKey = table.Column<string>(type: "character varying(128)", maxLength: 128, nullable: false),
                    Name = table.Column<string>(type: "character varying(128)", maxLength: 128, nullable: false),
                    SortOrder = table.Column<int>(type: "integer", nullable: false),
                    CreatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_maintainarr_inspection_template_categories", x => x.Id);
                    table.ForeignKey(
                        name: "FK_maintainarr_inspection_template_categories_maintainarr_insp~",
                        column: x => x.InspectionTemplateId,
                        principalTable: "maintainarr_inspection_templates",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "maintainarr_inspection_checklist_items",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    TenantId = table.Column<Guid>(type: "uuid", nullable: false),
                    InspectionTemplateId = table.Column<Guid>(type: "uuid", nullable: false),
                    CategoryId = table.Column<Guid>(type: "uuid", nullable: true),
                    ItemKey = table.Column<string>(type: "character varying(128)", maxLength: 128, nullable: false),
                    Prompt = table.Column<string>(type: "character varying(512)", maxLength: 512, nullable: false),
                    ItemType = table.Column<string>(type: "character varying(32)", maxLength: 32, nullable: false),
                    IsRequired = table.Column<bool>(type: "boolean", nullable: false),
                    SortOrder = table.Column<int>(type: "integer", nullable: false),
                    CreatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_maintainarr_inspection_checklist_items", x => x.Id);
                    table.ForeignKey(
                        name: "FK_maintainarr_inspection_checklist_items_maintainarr_inspecti~",
                        column: x => x.CategoryId,
                        principalTable: "maintainarr_inspection_template_categories",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                    table.ForeignKey(
                        name: "FK_maintainarr_inspection_checklist_items_maintainarr_inspect~1",
                        column: x => x.InspectionTemplateId,
                        principalTable: "maintainarr_inspection_templates",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_maintainarr_inspection_checklist_items_CategoryId",
                table: "maintainarr_inspection_checklist_items",
                column: "CategoryId");

            migrationBuilder.CreateIndex(
                name: "IX_maintainarr_inspection_checklist_items_InspectionTemplateId",
                table: "maintainarr_inspection_checklist_items",
                column: "InspectionTemplateId");

            migrationBuilder.CreateIndex(
                name: "IX_maintainarr_inspection_checklist_items_TenantId",
                table: "maintainarr_inspection_checklist_items",
                column: "TenantId");

            migrationBuilder.CreateIndex(
                name: "IX_maintainarr_inspection_checklist_items_TenantId_Inspection~1",
                table: "maintainarr_inspection_checklist_items",
                columns: new[] { "TenantId", "InspectionTemplateId", "SortOrder" });

            migrationBuilder.CreateIndex(
                name: "IX_maintainarr_inspection_checklist_items_TenantId_InspectionT~",
                table: "maintainarr_inspection_checklist_items",
                columns: new[] { "TenantId", "InspectionTemplateId", "ItemKey" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_maintainarr_inspection_template_asset_types_AssetTypeId",
                table: "maintainarr_inspection_template_asset_types",
                column: "AssetTypeId");

            migrationBuilder.CreateIndex(
                name: "IX_maintainarr_inspection_template_asset_types_InspectionTempl~",
                table: "maintainarr_inspection_template_asset_types",
                column: "InspectionTemplateId");

            migrationBuilder.CreateIndex(
                name: "IX_maintainarr_inspection_template_asset_types_TenantId",
                table: "maintainarr_inspection_template_asset_types",
                column: "TenantId");

            migrationBuilder.CreateIndex(
                name: "IX_maintainarr_inspection_template_asset_types_TenantId_Inspec~",
                table: "maintainarr_inspection_template_asset_types",
                columns: new[] { "TenantId", "InspectionTemplateId", "AssetTypeId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_maintainarr_inspection_template_categories_InspectionTempla~",
                table: "maintainarr_inspection_template_categories",
                column: "InspectionTemplateId");

            migrationBuilder.CreateIndex(
                name: "IX_maintainarr_inspection_template_categories_TenantId",
                table: "maintainarr_inspection_template_categories",
                column: "TenantId");

            migrationBuilder.CreateIndex(
                name: "IX_maintainarr_inspection_template_categories_TenantId_Inspect~",
                table: "maintainarr_inspection_template_categories",
                columns: new[] { "TenantId", "InspectionTemplateId", "CategoryKey" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_maintainarr_inspection_templates_TenantId",
                table: "maintainarr_inspection_templates",
                column: "TenantId");

            migrationBuilder.CreateIndex(
                name: "IX_maintainarr_inspection_templates_TenantId_Status",
                table: "maintainarr_inspection_templates",
                columns: new[] { "TenantId", "Status" });

            migrationBuilder.CreateIndex(
                name: "IX_maintainarr_inspection_templates_TenantId_TemplateKey",
                table: "maintainarr_inspection_templates",
                columns: new[] { "TenantId", "TemplateKey" },
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "maintainarr_inspection_checklist_items");

            migrationBuilder.DropTable(
                name: "maintainarr_inspection_template_asset_types");

            migrationBuilder.DropTable(
                name: "maintainarr_inspection_template_categories");

            migrationBuilder.DropTable(
                name: "maintainarr_inspection_templates");
        }
    }
}
