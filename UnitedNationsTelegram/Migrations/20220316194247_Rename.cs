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
                name: "UserCountryId",
                table: "Sanctions",
                newName: "SanctionId");

            migrationBuilder.RenameColumn(
                name: "UserCountryId",
                table: "Polls",
                newName: "PollId");

            migrationBuilder.RenameColumn(
                name: "UserCountryId",
                table: "Countries",
                newName: "CountryId");

            migrationBuilder.RenameColumn(
                name: "UserCountryId",
                table: "BFRoles",
                newName: "Id");

            migrationBuilder.RenameColumn(
                name: "UserCountryId",
                table: "BFRoleClaims",
                newName: "Id");

            migrationBuilder.RenameColumn(
                name: "UserCountryId",
                table: "BF_Users",
                newName: "Id");

            migrationBuilder.RenameColumn(
                name: "UserCountryId",
                table: "BF_UserClaims",
                newName: "Id");

            migrationBuilder.AlterColumn<int>(
                name: "UserCountryId",
                table: "Signature",
                type: "integer",
                nullable: false,
                oldClrType: typeof(int),
                oldType: "integer")
                .OldAnnotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn);

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
