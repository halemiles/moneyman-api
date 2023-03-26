using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Moneyman.Api.Migrations
{
    public partial class Addedprioritycategoryandpaymenttypefield : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "CategoryType",
                table: "Transactions",
                type: "INTEGER",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "PaymentType",
                table: "Transactions",
                type: "INTEGER",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "PriorityType",
                table: "Transactions",
                type: "INTEGER",
                nullable: false,
                defaultValue: 0);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "CategoryType",
                table: "Transactions");

            migrationBuilder.DropColumn(
                name: "PaymentType",
                table: "Transactions");

            migrationBuilder.DropColumn(
                name: "PriorityType",
                table: "Transactions");
        }
    }
}
