using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Moneyman.Api.Migrations
{
    public partial class PlanDates : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "PlanDates",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    TransactionId = table.Column<int>(type: "INTEGER", nullable: true),
                    Date = table.Column<DateTime>(type: "TEXT", nullable: false),
                    Active = table.Column<bool>(type: "INTEGER", nullable: false),
                    OriginalDate = table.Column<DateTime>(type: "TEXT", nullable: false),
                    YearGroup = table.Column<int>(type: "INTEGER", nullable: false),
                    MonthGroup = table.Column<int>(type: "INTEGER", nullable: false),
                    IsAnticipated = table.Column<bool>(type: "INTEGER", nullable: false),
                    OrderId = table.Column<int>(type: "INTEGER", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PlanDates", x => x.Id);
                    table.ForeignKey(
                        name: "FK_PlanDates_Transactions_TransactionId",
                        column: x => x.TransactionId,
                        principalTable: "Transactions",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateIndex(
                name: "IX_PlanDates_TransactionId",
                table: "PlanDates",
                column: "TransactionId");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "PlanDates");
        }
    }
}
