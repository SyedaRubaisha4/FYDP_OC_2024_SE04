using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace JobPost_Service.Migrations
{
    /// <inheritdoc />
    public partial class Type : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "Type",
                table: "ServicePosts",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "Type",
                table: "JobPosts",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Type",
                table: "ServicePosts");

            migrationBuilder.DropColumn(
                name: "Type",
                table: "JobPosts");
        }
    }
}
