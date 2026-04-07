using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace FxIntegrationApi.Migrations
{
    /// <inheritdoc />
    public partial class AddTraderSuggestions : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "TraderSuggestions",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    TraderId = table.Column<int>(type: "int", nullable: false),
                    CustomerId = table.Column<int>(type: "int", nullable: false),
                    ResearchArticleId = table.Column<int>(type: "int", nullable: false),
                    Reasoning = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    RelevanceScore = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    Status = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TraderSuggestions", x => x.Id);
                    table.ForeignKey(
                        name: "FK_TraderSuggestions_Customers_CustomerId",
                        column: x => x.CustomerId,
                        principalTable: "Customers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_TraderSuggestions_ResearchArticles_ResearchArticleId",
                        column: x => x.ResearchArticleId,
                        principalTable: "ResearchArticles",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_TraderSuggestions_Traders_TraderId",
                        column: x => x.TraderId,
                        principalTable: "Traders",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_TraderSuggestions_CustomerId",
                table: "TraderSuggestions",
                column: "CustomerId");

            migrationBuilder.CreateIndex(
                name: "IX_TraderSuggestions_ResearchArticleId",
                table: "TraderSuggestions",
                column: "ResearchArticleId");

            migrationBuilder.CreateIndex(
                name: "IX_TraderSuggestions_TraderId",
                table: "TraderSuggestions",
                column: "TraderId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "TraderSuggestions");
        }
    }
}
