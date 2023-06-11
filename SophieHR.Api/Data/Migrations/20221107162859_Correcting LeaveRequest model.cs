using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SophieHR.Api.Migrations
{
    public partial class CorrectingLeaveRequestmodel : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "SecondHalf",
                table: "LeaveRequests",
                newName: "StartDateSecondHalf");

            migrationBuilder.RenameColumn(
                name: "FirstHalf",
                table: "LeaveRequests",
                newName: "StartDateFirstHalf");

            migrationBuilder.AddColumn<bool>(
                name: "EndDateFirstHalf",
                table: "LeaveRequests",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "EndDateSecondHalf",
                table: "LeaveRequests",
                type: "bit",
                nullable: false,
                defaultValue: false);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "EndDateFirstHalf",
                table: "LeaveRequests");

            migrationBuilder.DropColumn(
                name: "EndDateSecondHalf",
                table: "LeaveRequests");

            migrationBuilder.RenameColumn(
                name: "StartDateSecondHalf",
                table: "LeaveRequests",
                newName: "SecondHalf");

            migrationBuilder.RenameColumn(
                name: "StartDateFirstHalf",
                table: "LeaveRequests",
                newName: "FirstHalf");
        }
    }
}