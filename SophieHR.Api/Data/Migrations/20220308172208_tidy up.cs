using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SophieHR.Api.Migrations
{
    public partial class tidyup : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_AspNetUsers_EmployeeAvatar_AvatarId",
                table: "AspNetUsers");

            migrationBuilder.DropIndex(
                name: "IX_AspNetUsers_AvatarId",
                table: "AspNetUsers");

            migrationBuilder.DropColumn(
                name: "AvatarId",
                table: "AspNetUsers");

            migrationBuilder.CreateIndex(
                name: "IX_EmployeeAvatar_EmployeeId",
                table: "EmployeeAvatar",
                column: "EmployeeId",
                unique: true);

            migrationBuilder.AddForeignKey(
                name: "FK_EmployeeAvatar_AspNetUsers_EmployeeId",
                table: "EmployeeAvatar",
                column: "EmployeeId",
                principalTable: "AspNetUsers",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_EmployeeAvatar_AspNetUsers_EmployeeId",
                table: "EmployeeAvatar");

            migrationBuilder.DropIndex(
                name: "IX_EmployeeAvatar_EmployeeId",
                table: "EmployeeAvatar");

            migrationBuilder.AddColumn<Guid>(
                name: "AvatarId",
                table: "AspNetUsers",
                type: "uniqueidentifier",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_AspNetUsers_AvatarId",
                table: "AspNetUsers",
                column: "AvatarId");

            migrationBuilder.AddForeignKey(
                name: "FK_AspNetUsers_EmployeeAvatar_AvatarId",
                table: "AspNetUsers",
                column: "AvatarId",
                principalTable: "EmployeeAvatar",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}