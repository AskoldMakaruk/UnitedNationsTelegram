using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace UnitedNationsTelegram.Migrations
{
    public partial class NewPollModel : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "ChatId",
                table: "Polls");

            migrationBuilder.AddColumn<DateTime>(
                name: "Created",
                table: "Polls",
                type: "timestamp without time zone",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.AddColumn<int>(
                name: "OpenedById",
                table: "Polls",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.CreateIndex(
                name: "IX_Polls_OpenedById",
                table: "Polls",
                column: "OpenedById");

            migrationBuilder.AddForeignKey(
                name: "FK_Polls_UserCountries_OpenedById",
                table: "Polls",
                column: "OpenedById",
                principalTable: "UserCountries",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Polls_UserCountries_OpenedById",
                table: "Polls");

            migrationBuilder.DropIndex(
                name: "IX_Polls_OpenedById",
                table: "Polls");

            migrationBuilder.DropColumn(
                name: "Created",
                table: "Polls");

            migrationBuilder.DropColumn(
                name: "OpenedById",
                table: "Polls");

            migrationBuilder.AddColumn<long>(
                name: "ChatId",
                table: "Polls",
                type: "bigint",
                nullable: false,
                defaultValue: 0L);
        }
    }
}
