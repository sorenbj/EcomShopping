using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace EcomShopping.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class OptimizeStockReservationIndex : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_StockReservations_ProductId",
                table: "StockReservations");

            migrationBuilder.CreateIndex(
                name: "IX_StockReservations_ProductId_IsReleased_ExpiresAt",
                table: "StockReservations",
                columns: new[] { "ProductId", "IsReleased", "ExpiresAt" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_StockReservations_ProductId_IsReleased_ExpiresAt",
                table: "StockReservations");

            migrationBuilder.CreateIndex(
                name: "IX_StockReservations_ProductId",
                table: "StockReservations",
                column: "ProductId");
        }
    }
}
