using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SophieHR.Api.Migrations
{
    /// <inheritdoc />
    public partial class Addingcommentstoleaverequest : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "Comments",
                table: "LeaveRequests",
                type: "nvarchar(max)",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Comments",
                table: "LeaveRequests");
        }
    }
}
