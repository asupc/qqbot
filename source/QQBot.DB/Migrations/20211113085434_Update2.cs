using Microsoft.EntityFrameworkCore.Migrations;

namespace QQBot.DB.Migrations
{
    public partial class Update2 : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {

            migrationBuilder.AddColumn<int>(
                name: "ConcCount",
                table: "t_qqbot_task",
                type: "int",
                nullable: false,
                defaultValue: 0);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "ConcCount",
                table: "t_qqbot_task");
        }
    }
}
