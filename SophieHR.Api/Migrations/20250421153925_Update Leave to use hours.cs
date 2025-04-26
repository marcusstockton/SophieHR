using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SophieHR.Api.Migrations
{
    /// <inheritdoc />
    public partial class UpdateLeavetousehours : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "EndDateFirstHalf",
                table: "LeaveRequests");

            migrationBuilder.DropColumn(
                name: "EndDateSecondHalf",
                table: "LeaveRequests");

            migrationBuilder.DropColumn(
                name: "StartDateFirstHalf",
                table: "LeaveRequests");

            migrationBuilder.DropColumn(
                name: "StartDateSecondHalf",
                table: "LeaveRequests");

            migrationBuilder.AddColumn<int>(
                name: "Hours",
                table: "LeaveRequests",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "NormalHoursPerDay",
                table: "LeaveRequests",
                type: "int",
                nullable: false,
                defaultValue: 0);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Hours",
                table: "LeaveRequests");

            migrationBuilder.DropColumn(
                name: "NormalHoursPerDay",
                table: "LeaveRequests");

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

            migrationBuilder.AddColumn<bool>(
                name: "StartDateFirstHalf",
                table: "LeaveRequests",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "StartDateSecondHalf",
                table: "LeaveRequests",
                type: "bit",
                nullable: false,
                defaultValue: false);
        }
    }
}
