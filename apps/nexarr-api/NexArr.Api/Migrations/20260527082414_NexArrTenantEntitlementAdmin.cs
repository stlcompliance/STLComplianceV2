using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace NexArr.Api.Migrations
{
    /// <inheritdoc />
    public partial class NexArrTenantEntitlementAdmin : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "service_clients",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    ClientKey = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: false),
                    DisplayName = table.Column<string>(type: "character varying(128)", maxLength: 128, nullable: false),
                    SourceProductKey = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: false),
                    AllowedProductKeys = table.Column<string>(type: "character varying(512)", maxLength: 512, nullable: false),
                    IsActive = table.Column<bool>(type: "boolean", nullable: false),
                    CreatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    ModifiedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_service_clients", x => x.Id);
                    table.ForeignKey(
                        name: "FK_service_clients_product_catalog_SourceProductKey",
                        column: x => x.SourceProductKey,
                        principalTable: "product_catalog",
                        principalColumn: "ProductKey",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "service_tokens",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    ServiceClientId = table.Column<Guid>(type: "uuid", nullable: false),
                    Jti = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: false),
                    TokenHash = table.Column<string>(type: "character varying(128)", maxLength: 128, nullable: false),
                    TenantId = table.Column<Guid>(type: "uuid", nullable: true),
                    AllowedProductKeys = table.Column<string>(type: "character varying(512)", maxLength: 512, nullable: false),
                    ActionScope = table.Column<string>(type: "character varying(128)", maxLength: 128, nullable: true),
                    ExpiresAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    RevokedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    IssuedByUserId = table.Column<Guid>(type: "uuid", nullable: false),
                    CreatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_service_tokens", x => x.Id);
                    table.ForeignKey(
                        name: "FK_service_tokens_service_clients_ServiceClientId",
                        column: x => x.ServiceClientId,
                        principalTable: "service_clients",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_service_clients_ClientKey",
                table: "service_clients",
                column: "ClientKey",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_service_clients_SourceProductKey",
                table: "service_clients",
                column: "SourceProductKey");

            migrationBuilder.CreateIndex(
                name: "IX_service_tokens_ExpiresAt",
                table: "service_tokens",
                column: "ExpiresAt");

            migrationBuilder.CreateIndex(
                name: "IX_service_tokens_Jti",
                table: "service_tokens",
                column: "Jti",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_service_tokens_ServiceClientId",
                table: "service_tokens",
                column: "ServiceClientId");

            migrationBuilder.CreateIndex(
                name: "IX_service_tokens_TenantId",
                table: "service_tokens",
                column: "TenantId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "service_tokens");

            migrationBuilder.DropTable(
                name: "service_clients");
        }
    }
}
