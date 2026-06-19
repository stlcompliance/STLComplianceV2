using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace NexArr.Api.Migrations
{
    /// <inheritdoc />
    public partial class AddUserCredentialRequiresPasswordChange : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "RequiresPasswordChange",
                table: "user_credentials",
                type: "boolean",
                nullable: false,
                defaultValue: false);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "RequiresPasswordChange",
                table: "user_credentials");
        }
    }
}
