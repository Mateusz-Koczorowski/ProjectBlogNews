﻿using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ProjectBlogNews.Data.Migrations
{
    public partial class SubscriptionModelCreate : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<decimal>(
                name: "Price",
                table: "Subscription",
                type: "decimal(18,2)",
                nullable: false,
                defaultValue: 0m);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Price",
                table: "Subscription");
        }
    }
}
