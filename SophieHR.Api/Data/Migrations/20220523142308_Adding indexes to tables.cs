using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SophieHR.Api.Migrations
{
    public partial class Addingindexestotables : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateIndex(
                name: "IX_Departments_Id_Name",
                table: "Departments",
                columns: new[] { "Id", "Name" });

            migrationBuilder.CreateIndex(
                name: "IX_Companies_Id_Name",
                table: "Companies",
                columns: new[] { "Id", "Name" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_AspNetUsers_Id_Email_UserName",
                table: "AspNetUsers",
                columns: new[] { "Id", "Email", "UserName" });
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_Departments_Id_Name",
                table: "Departments");

            migrationBuilder.DropIndex(
                name: "IX_Companies_Id_Name",
                table: "Companies");

            migrationBuilder.DropIndex(
                name: "IX_AspNetUsers_Id_Email_UserName",
                table: "AspNetUsers");
        }
    }
}