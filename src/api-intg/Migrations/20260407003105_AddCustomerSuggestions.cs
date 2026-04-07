using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace FxIntegrationApi.Migrations
{
    /// <inheritdoc />
    public partial class AddCustomerSuggestions : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "CustomerSuggestions",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    CustomerName = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    Phone = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    Email = table.Column<string>(type: "nvarchar(300)", maxLength: 300, nullable: false),
                    Company = table.Column<string>(type: "nvarchar(300)", maxLength: 300, nullable: false),
                    CurrencyPair = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    Direction = table.Column<string>(type: "nvarchar(10)", maxLength: 10, nullable: false),
                    Analysis = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Confidence = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    SuggestedBy = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    ReceivedAt = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CustomerSuggestions", x => x.Id);
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "CustomerSuggestions");
        }
    }
}
