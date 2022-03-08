using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace UnitedNationsTelegram.Migrations
{
    public partial class Countries : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_BF_Users_Country_CountryId",
                table: "BF_Users");

            migrationBuilder.DropForeignKey(
                name: "FK_Vote_Country_CountryId",
                table: "Vote");

            migrationBuilder.DropForeignKey(
                name: "FK_Vote_Poll_PollId",
                table: "Vote");

            migrationBuilder.DropPrimaryKey(
                name: "PK_Vote",
                table: "Vote");

            migrationBuilder.DropPrimaryKey(
                name: "PK_Poll",
                table: "Poll");

            migrationBuilder.DropPrimaryKey(
                name: "PK_Country",
                table: "Country");

            migrationBuilder.RenameTable(
                name: "Vote",
                newName: "Votes");

            migrationBuilder.RenameTable(
                name: "Poll",
                newName: "Polls");

            migrationBuilder.RenameTable(
                name: "Country",
                newName: "Countries");

            migrationBuilder.RenameIndex(
                name: "IX_Vote_PollId",
                table: "Votes",
                newName: "IX_Votes_PollId");

            migrationBuilder.AddPrimaryKey(
                name: "PK_Votes",
                table: "Votes",
                columns: new[] { "CountryId", "PollId" });

            migrationBuilder.AddPrimaryKey(
                name: "PK_Polls",
                table: "Polls",
                column: "Id");

            migrationBuilder.AddPrimaryKey(
                name: "PK_Countries",
                table: "Countries",
                column: "Id");

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

            migrationBuilder.AddForeignKey(
                name: "FK_Votes_Polls_PollId",
                table: "Votes",
                column: "PollId",
                principalTable: "Polls",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_BF_Users_Countries_CountryId",
                table: "BF_Users");

            migrationBuilder.DropForeignKey(
                name: "FK_Votes_Countries_CountryId",
                table: "Votes");

            migrationBuilder.DropForeignKey(
                name: "FK_Votes_Polls_PollId",
                table: "Votes");

            migrationBuilder.DropPrimaryKey(
                name: "PK_Votes",
                table: "Votes");

            migrationBuilder.DropPrimaryKey(
                name: "PK_Polls",
                table: "Polls");

            migrationBuilder.DropPrimaryKey(
                name: "PK_Countries",
                table: "Countries");

            migrationBuilder.RenameTable(
                name: "Votes",
                newName: "Vote");

            migrationBuilder.RenameTable(
                name: "Polls",
                newName: "Poll");

            migrationBuilder.RenameTable(
                name: "Countries",
                newName: "Country");

            migrationBuilder.RenameIndex(
                name: "IX_Votes_PollId",
                table: "Vote",
                newName: "IX_Vote_PollId");

            migrationBuilder.AddPrimaryKey(
                name: "PK_Vote",
                table: "Vote",
                columns: new[] { "CountryId", "PollId" });

            migrationBuilder.AddPrimaryKey(
                name: "PK_Poll",
                table: "Poll",
                column: "Id");

            migrationBuilder.AddPrimaryKey(
                name: "PK_Country",
                table: "Country",
                column: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_BF_Users_Country_CountryId",
                table: "BF_Users",
                column: "CountryId",
                principalTable: "Country",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_Vote_Country_CountryId",
                table: "Vote",
                column: "CountryId",
                principalTable: "Country",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_Vote_Poll_PollId",
                table: "Vote",
                column: "PollId",
                principalTable: "Poll",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
