using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace NexArr.Api.Migrations
{
    public partial class NexArrProductManifestMetadata : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "ApiBaseUrl",
                table: "product_catalog",
                type: "character varying(512)",
                maxLength: 512,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "CanonicalCallbackPath",
                table: "product_catalog",
                type: "character varying(128)",
                maxLength: 128,
                nullable: false,
                defaultValue: "/auth/nexarr/callback");

            migrationBuilder.AddColumn<string>(
                name: "DocumentationUrl",
                table: "product_catalog",
                type: "character varying(512)",
                maxLength: 512,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "EntitlementDependencyRules",
                table: "product_catalog",
                type: "character varying(2048)",
                maxLength: 2048,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "EnvironmentKey",
                table: "product_catalog",
                type: "character varying(64)",
                maxLength: 64,
                nullable: false,
                defaultValue: "local");

            migrationBuilder.AddColumn<string>(
                name: "HealthUrl",
                table: "product_catalog",
                type: "character varying(512)",
                maxLength: 512,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "MarketingUrl",
                table: "product_catalog",
                type: "character varying(512)",
                maxLength: 512,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "ProductCategory",
                table: "product_catalog",
                type: "character varying(64)",
                maxLength: 64,
                nullable: false,
                defaultValue: "operations");

            migrationBuilder.AddColumn<string>(
                name: "ProductDependencyMetadata",
                table: "product_catalog",
                type: "character varying(2048)",
                maxLength: 2048,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "ProductOwner",
                table: "product_catalog",
                type: "character varying(128)",
                maxLength: 128,
                nullable: false,
                defaultValue: "STL Compliance");

            migrationBuilder.AddColumn<string>(
                name: "ProductStatus",
                table: "product_catalog",
                type: "character varying(32)",
                maxLength: 32,
                nullable: false,
                defaultValue: "available");

            migrationBuilder.AddColumn<string>(
                name: "ServiceAudience",
                table: "product_catalog",
                type: "character varying(128)",
                maxLength: 128,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "SupportUrl",
                table: "product_catalog",
                type: "character varying(512)",
                maxLength: 512,
                nullable: false,
                defaultValue: "");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "ApiBaseUrl",
                table: "product_catalog");

            migrationBuilder.DropColumn(
                name: "CanonicalCallbackPath",
                table: "product_catalog");

            migrationBuilder.DropColumn(
                name: "DocumentationUrl",
                table: "product_catalog");

            migrationBuilder.DropColumn(
                name: "EntitlementDependencyRules",
                table: "product_catalog");

            migrationBuilder.DropColumn(
                name: "EnvironmentKey",
                table: "product_catalog");

            migrationBuilder.DropColumn(
                name: "HealthUrl",
                table: "product_catalog");

            migrationBuilder.DropColumn(
                name: "MarketingUrl",
                table: "product_catalog");

            migrationBuilder.DropColumn(
                name: "ProductCategory",
                table: "product_catalog");

            migrationBuilder.DropColumn(
                name: "ProductDependencyMetadata",
                table: "product_catalog");

            migrationBuilder.DropColumn(
                name: "ProductOwner",
                table: "product_catalog");

            migrationBuilder.DropColumn(
                name: "ProductStatus",
                table: "product_catalog");

            migrationBuilder.DropColumn(
                name: "ServiceAudience",
                table: "product_catalog");

            migrationBuilder.DropColumn(
                name: "SupportUrl",
                table: "product_catalog");
        }
    }
}
