using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace FxIntegrationApi.Migrations
{
    /// <inheritdoc />
    public partial class AddTradingObjectiveToCustomerPreferences : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "TradingObjective",
                table: "CustomerPreferences",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "TradingObjective",
                table: "CustomerPreferences");
        }
    }
}
