using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace UnitedNationsTelegram.Migrations
{
    public partial class Model : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "State",
                table: "BF_Users");

            migrationBuilder.AddColumn<int>(
                name: "CountryId",
                table: "BF_Users",
                type: "integer",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "Country",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    EmojiFlag = table.Column<string>(type: "text", nullable: false),
                    Name = table.Column<string>(type: "text", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Country", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Poll",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    Text = table.Column<string>(type: "text", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Poll", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Vote",
                columns: table => new
                {
                    CountryId = table.Column<int>(type: "integer", nullable: false),
                    PollId = table.Column<int>(type: "integer", nullable: false),
                    Reaction = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Vote", x => new { x.CountryId, x.PollId });
                    table.ForeignKey(
                        name: "FK_Vote_Country_CountryId",
                        column: x => x.CountryId,
                        principalTable: "Country",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_Vote_Poll_PollId",
                        column: x => x.PollId,
                        principalTable: "Poll",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_BF_Users_CountryId",
                table: "BF_Users",
                column: "CountryId");

            migrationBuilder.CreateIndex(
                name: "IX_Vote_PollId",
                table: "Vote",
                column: "PollId");

            migrationBuilder.AddForeignKey(
                name: "FK_BF_Users_Country_CountryId",
                table: "BF_Users",
                column: "CountryId",
                principalTable: "Country",
                principalColumn: "Id");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_BF_Users_Country_CountryId",
                table: "BF_Users");

            migrationBuilder.DropTable(
                name: "Vote");

            migrationBuilder.DropTable(
                name: "Country");

            migrationBuilder.DropTable(
                name: "Poll");

            migrationBuilder.DropIndex(
                name: "IX_BF_Users_CountryId",
                table: "BF_Users");

            migrationBuilder.DropColumn(
                name: "CountryId",
                table: "BF_Users");

            migrationBuilder.AddColumn<string>(
                name: "State",
                table: "BF_Users",
                type: "text",
                nullable: true);
        }
    }
}
