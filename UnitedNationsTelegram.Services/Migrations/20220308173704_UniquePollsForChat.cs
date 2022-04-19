using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace UnitedNationsTelegram.Migrations
{
    public partial class UniquePollsForChat : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<long>(
                name: "ChatId",
                table: "Polls",
                type: "bigint",
                nullable: false,
                defaultValue: 0L);

            migrationBuilder.AddColumn<bool>(
                name: "IsActive",
                table: "Polls",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<int>(
                name: "MessageId",
                table: "Polls",
                type: "integer",
                nullable: false,
                defaultValue: 0);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "ChatId",
                table: "Polls");

            migrationBuilder.DropColumn(
                name: "IsActive",
                table: "Polls");

            migrationBuilder.DropColumn(
                name: "MessageId",
                table: "Polls");
        }
    }
}
