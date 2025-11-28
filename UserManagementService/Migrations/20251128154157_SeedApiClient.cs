using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace UserManagementService.Migrations
{
    /// <inheritdoc />
    public partial class SeedApiClient : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.InsertData(
                table: "ApiClients",
                columns: new[] { "Id", "ApiKey", "Name" },
                values: new object[] { 1, "test-api-key-123", "Test Client" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DeleteData(
                table: "ApiClients",
                keyColumn: "Id",
                keyValue: 1);
        }
    }
}
