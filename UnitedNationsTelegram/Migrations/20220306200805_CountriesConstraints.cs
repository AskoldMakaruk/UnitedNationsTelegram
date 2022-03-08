using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace UnitedNationsTelegram.Migrations
{
    public partial class CountriesConstraints : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateIndex(
                name: "IX_Countries_EmojiFlag",
                table: "Countries",
                column: "EmojiFlag",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Countries_Name",
                table: "Countries",
                column: "Name",
                unique: true);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_Countries_EmojiFlag",
                table: "Countries");

            migrationBuilder.DropIndex(
                name: "IX_Countries_Name",
                table: "Countries");
        }
    }
}
