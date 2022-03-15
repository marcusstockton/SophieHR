using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SophieHR.Api.Migrations
{
    public partial class EmployeeAvatar : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_EmployeeAvatar_AspNetUsers_EmployeeId",
                table: "EmployeeAvatar");

            migrationBuilder.DropPrimaryKey(
                name: "PK_EmployeeAvatar",
                table: "EmployeeAvatar");

            migrationBuilder.RenameTable(
                name: "EmployeeAvatar",
                newName: "EmployeeAvatars");

            migrationBuilder.RenameIndex(
                name: "IX_EmployeeAvatar_EmployeeId",
                table: "EmployeeAvatars",
                newName: "IX_EmployeeAvatars_EmployeeId");

            migrationBuilder.AddPrimaryKey(
                name: "PK_EmployeeAvatars",
                table: "EmployeeAvatars",
                column: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_EmployeeAvatars_AspNetUsers_EmployeeId",
                table: "EmployeeAvatars",
                column: "EmployeeId",
                principalTable: "AspNetUsers",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_EmployeeAvatars_AspNetUsers_EmployeeId",
                table: "EmployeeAvatars");

            migrationBuilder.DropPrimaryKey(
                name: "PK_EmployeeAvatars",
                table: "EmployeeAvatars");

            migrationBuilder.RenameTable(
                name: "EmployeeAvatars",
                newName: "EmployeeAvatar");

            migrationBuilder.RenameIndex(
                name: "IX_EmployeeAvatars_EmployeeId",
                table: "EmployeeAvatar",
                newName: "IX_EmployeeAvatar_EmployeeId");

            migrationBuilder.AddPrimaryKey(
                name: "PK_EmployeeAvatar",
                table: "EmployeeAvatar",
                column: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_EmployeeAvatar_AspNetUsers_EmployeeId",
                table: "EmployeeAvatar",
                column: "EmployeeId",
                principalTable: "AspNetUsers",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
