using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Moneyman.Api.Migrations
{
    public partial class Anticipated : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "Anticipated",
                table: "Transactions",
                type: "INTEGER",
                nullable: false,
                defaultValue: false);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Anticipated",
                table: "Transactions");
        }
    }
}
