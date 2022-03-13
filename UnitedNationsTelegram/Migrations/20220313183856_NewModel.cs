using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace UnitedNationsTelegram.Migrations
{
    public partial class NewModel : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_BF_Users_Countries_CountryId",
                table: "BF_Users");

            migrationBuilder.DropForeignKey(
                name: "FK_Votes_Countries_CountryId",
                table: "Votes");

            migrationBuilder.DropIndex(
                name: "IX_BF_Users_CountryId",
                table: "BF_Users");

            migrationBuilder.DropColumn(
                name: "CountryId",
                table: "BF_Users");

            migrationBuilder.RenameColumn(
                name: "CountryId",
                table: "Votes",
                newName: "UserCountryId");

            migrationBuilder.CreateTable(
                name: "UserCountries",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    ChatId = table.Column<long>(type: "bigint", nullable: false),
                    CountryId = table.Column<int>(type: "integer", nullable: false),
                    UserId = table.Column<long>(type: "bigint", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_UserCountries", x => x.Id);
                    table.ForeignKey(
                        name: "FK_UserCountries_BF_Users_UserId",
                        column: x => x.UserId,
                        principalTable: "BF_Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_UserCountries_Countries_CountryId",
                        column: x => x.CountryId,
                        principalTable: "Countries",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_UserCountries_CountryId_ChatId_UserId",
                table: "UserCountries",
                columns: new[] { "CountryId", "ChatId", "UserId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_UserCountries_UserId",
                table: "UserCountries",
                column: "UserId");

            migrationBuilder.AddForeignKey(
                name: "FK_Votes_UserCountries_UserCountryId",
                table: "Votes",
                column: "UserCountryId",
                principalTable: "UserCountries",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Votes_UserCountries_UserCountryId",
                table: "Votes");

            migrationBuilder.DropTable(
                name: "UserCountries");

            migrationBuilder.RenameColumn(
                name: "UserCountryId",
                table: "Votes",
                newName: "CountryId");

            migrationBuilder.AddColumn<int>(
                name: "CountryId",
                table: "BF_Users",
                type: "integer",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_BF_Users_CountryId",
                table: "BF_Users",
                column: "CountryId");

            migrationBuilder.AddForeignKey(
                name: "FK_BF_Users_Countries_CountryId",
                table: "BF_Users",
                column: "CountryId",
                principalTable: "Countries",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_Votes_Countries_CountryId",
                table: "Votes",
                column: "CountryId",
                principalTable: "Countries",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
