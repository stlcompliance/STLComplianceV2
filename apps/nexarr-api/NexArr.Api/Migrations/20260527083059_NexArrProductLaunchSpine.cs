using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace NexArr.Api.Migrations
{
    /// <inheritdoc />
    public partial class NexArrProductLaunchSpine : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "handoff_codes",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    CodeHash = table.Column<string>(type: "character varying(128)", maxLength: 128, nullable: false),
                    UserId = table.Column<Guid>(type: "uuid", nullable: false),
                    TenantId = table.Column<Guid>(type: "uuid", nullable: false),
                    SessionId = table.Column<Guid>(type: "uuid", nullable: false),
                    TargetProductKey = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: false),
                    CallbackUrl = table.Column<string>(type: "character varying(2048)", maxLength: 2048, nullable: true),
                    ExpiresAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    RedeemedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    CreatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_handoff_codes", x => x.Id);
                    table.ForeignKey(
                        name: "FK_handoff_codes_platform_users_UserId",
                        column: x => x.UserId,
                        principalTable: "platform_users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_handoff_codes_tenants_TenantId",
                        column: x => x.TenantId,
                        principalTable: "tenants",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "product_callback_allowlist",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    ProductKey = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: false),
                    TenantId = table.Column<Guid>(type: "uuid", nullable: true),
                    UrlPattern = table.Column<string>(type: "character varying(512)", maxLength: 512, nullable: false),
                    PatternType = table.Column<string>(type: "character varying(32)", maxLength: 32, nullable: false),
                    IsActive = table.Column<bool>(type: "boolean", nullable: false),
                    CreatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    ModifiedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_product_callback_allowlist", x => x.Id);
                    table.ForeignKey(
                        name: "FK_product_callback_allowlist_product_catalog_ProductKey",
                        column: x => x.ProductKey,
                        principalTable: "product_catalog",
                        principalColumn: "ProductKey",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_product_callback_allowlist_tenants_TenantId",
                        column: x => x.TenantId,
                        principalTable: "tenants",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "product_launch_profiles",
                columns: table => new
                {
                    ProductKey = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: false),
                    BaseUrl = table.Column<string>(type: "character varying(512)", maxLength: 512, nullable: false),
                    LaunchPath = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: false),
                    IsActive = table.Column<bool>(type: "boolean", nullable: false),
                    ModifiedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_product_launch_profiles", x => x.ProductKey);
                    table.ForeignKey(
                        name: "FK_product_launch_profiles_product_catalog_ProductKey",
                        column: x => x.ProductKey,
                        principalTable: "product_catalog",
                        principalColumn: "ProductKey",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_handoff_codes_CodeHash",
                table: "handoff_codes",
                column: "CodeHash",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_handoff_codes_ExpiresAt",
                table: "handoff_codes",
                column: "ExpiresAt");

            migrationBuilder.CreateIndex(
                name: "IX_handoff_codes_TenantId_TargetProductKey",
                table: "handoff_codes",
                columns: new[] { "TenantId", "TargetProductKey" });

            migrationBuilder.CreateIndex(
                name: "IX_handoff_codes_UserId",
                table: "handoff_codes",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_product_callback_allowlist_ProductKey_TenantId",
                table: "product_callback_allowlist",
                columns: new[] { "ProductKey", "TenantId" });

            migrationBuilder.CreateIndex(
                name: "IX_product_callback_allowlist_TenantId",
                table: "product_callback_allowlist",
                column: "TenantId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "handoff_codes");

            migrationBuilder.DropTable(
                name: "product_callback_allowlist");

            migrationBuilder.DropTable(
                name: "product_launch_profiles");
        }
    }
}
