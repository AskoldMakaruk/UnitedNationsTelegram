using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace UnitedNationsTelegram.Migrations
{
    public partial class Rename : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropPrimaryKey(
                name: "PK_Signature",
                table: "Signature");

            migrationBuilder.RenameColumn(
                name: "Id",
                table: "UserCountries",
                newName: "UserCountryId");
            
            migrationBuilder.RenameColumn(
                name: "Id",
                table: "Sanctions",
                newName: "SanctionId");

            migrationBuilder.RenameColumn(
                name: "Id",
                table: "Polls",
                newName: "PollId");

            migrationBuilder.RenameColumn(
                name: "Id",
                table: "Countries",
                newName: "CountryId");

            migrationBuilder.AddColumn<int>(
                name: "SignatureId",
                table: "Signature",
                type: "integer",
                nullable: false,
                defaultValue: 0)
                .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn);

            migrationBuilder.AddPrimaryKey(
                name: "PK_Signature",
                table: "Signature",
                column: "SignatureId");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropPrimaryKey(
                name: "PK_Signature",
                table: "Signature");

            migrationBuilder.DropColumn(
                name: "SignatureId",
                table: "Signature");

            migrationBuilder.RenameColumn(
                name: "SanctionId",
                table: "Sanctions",
                newName: "UserCountryId");

            migrationBuilder.RenameColumn(
                name: "PollId",
                table: "Polls",
                newName: "UserCountryId");

            migrationBuilder.RenameColumn(
                name: "CountryId",
                table: "Countries",
                newName: "UserCountryId");

            migrationBuilder.RenameColumn(
                name: "Id",
                table: "BFRoles",
                newName: "UserCountryId");

            migrationBuilder.RenameColumn(
                name: "Id",
                table: "BFRoleClaims",
                newName: "UserCountryId");

            migrationBuilder.RenameColumn(
                name: "Id",
                table: "BF_Users",
                newName: "UserCountryId");

            migrationBuilder.RenameColumn(
                name: "Id",
                table: "BF_UserClaims",
                newName: "UserCountryId");

            migrationBuilder.AlterColumn<int>(
                name: "UserCountryId",
                table: "Signature",
                type: "integer",
                nullable: false,
                oldClrType: typeof(int),
                oldType: "integer")
                .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn);

            migrationBuilder.AddPrimaryKey(
                name: "PK_Signature",
                table: "Signature",
                column: "UserCountryId");
        }
    }
}
