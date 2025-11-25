using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SophieHR.Api.Migrations
{
    /// <inheritdoc />
    public partial class Addingindexes : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_AspNetUsers_CompanyId",
                table: "AspNetUsers");

            migrationBuilder.CreateIndex(
                name: "IX_LeaveRequests_Id_EmployeeId",
                table: "LeaveRequests",
                columns: new[] { "Id", "EmployeeId" });

            migrationBuilder.CreateIndex(
                name: "IX_AspNetUsers_CompanyId_DepartmentId_ManagerId",
                table: "AspNetUsers",
                columns: new[] { "CompanyId", "DepartmentId", "ManagerId" });

            migrationBuilder.CreateIndex(
                name: "IX_AspNetUsers_Id_WorkEmailAddress",
                table: "AspNetUsers",
                columns: new[] { "Id", "WorkEmailAddress" },
                unique: true,
                filter: "[WorkEmailAddress] IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "IX_Address_Id_Postcode",
                table: "Address",
                columns: new[] { "Id", "Postcode" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_LeaveRequests_Id_EmployeeId",
                table: "LeaveRequests");

            migrationBuilder.DropIndex(
                name: "IX_AspNetUsers_CompanyId_DepartmentId_ManagerId",
                table: "AspNetUsers");

            migrationBuilder.DropIndex(
                name: "IX_AspNetUsers_Id_WorkEmailAddress",
                table: "AspNetUsers");

            migrationBuilder.DropIndex(
                name: "IX_Address_Id_Postcode",
                table: "Address");

            migrationBuilder.CreateIndex(
                name: "IX_AspNetUsers_CompanyId",
                table: "AspNetUsers",
                column: "CompanyId");
        }
    }
}
