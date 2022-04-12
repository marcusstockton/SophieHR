using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SophieHR.Api.Migrations
{
    public partial class AddedPassportandnationalinsurancepropertiestoemployee : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "NationalInsuranceNumber",
                table: "AspNetUsers",
                type: "nvarchar(9)",
                maxLength: 9,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "PassportNumber",
                table: "AspNetUsers",
                type: "nvarchar(9)",
                maxLength: 9,
                nullable: true);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "NationalInsuranceNumber",
                table: "AspNetUsers");

            migrationBuilder.DropColumn(
                name: "PassportNumber",
                table: "AspNetUsers");
        }
    }
}