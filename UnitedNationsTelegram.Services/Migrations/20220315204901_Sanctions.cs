using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace UnitedNationsTelegram.Migrations
{
    public partial class Sanctions : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "Type",
                table: "Polls",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.CreateTable(
                name: "Sanctions",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    PollId = table.Column<int>(type: "integer", nullable: false),
                    AgainstId = table.Column<int>(type: "integer", nullable: false),
                    SanctionType = table.Column<string>(type: "text", nullable: false),
                    ActiveUntil = table.Column<DateTime>(type: "timestamp without time zone", nullable: true),
                    IsSupported = table.Column<bool>(type: "boolean", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Sanctions", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Sanctions_Polls_PollId",
                        column: x => x.PollId,
                        principalTable: "Polls",
                        principalColumn: "UserCountryId",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_Sanctions_UserCountries_AgainstId",
                        column: x => x.AgainstId,
                        principalTable: "UserCountries",
                        principalColumn: "UserCountryId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Sanctions_AgainstId",
                table: "Sanctions",
                column: "AgainstId");

            migrationBuilder.CreateIndex(
                name: "IX_Sanctions_PollId",
                table: "Sanctions",
                column: "PollId",
                unique: true);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Sanctions");

            migrationBuilder.DropColumn(
                name: "Type",
                table: "Polls");
        }
    }
}
