using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ProjectBlogNews.Data.Migrations
{
    public partial class AddImageToArticleModel : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "AltText",
                table: "Article",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "ImageFileName",
                table: "Article",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "AltText",
                table: "Article");

            migrationBuilder.DropColumn(
                name: "ImageFileName",
                table: "Article");
        }
    }
}
