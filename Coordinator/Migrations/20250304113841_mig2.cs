using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace Coordinator.Migrations
{
    /// <inheritdoc />
    public partial class mig2 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.InsertData(
                table: "Nodes",
                columns: new[] { "Id", "Name" },
                values: new object[,]
                {
                    { new Guid("21c793b4-a383-44d0-a94a-ccdada4e36ba"), "Stock.API" },
                    { new Guid("26717097-8cf3-4474-a7a0-d77b6b2ce727"), "Payment.API" },
                    { new Guid("4c5b4f07-6fef-4a57-809f-9bfa0ce92590"), "Order.API" }
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DeleteData(
                table: "Nodes",
                keyColumn: "Id",
                keyValue: new Guid("21c793b4-a383-44d0-a94a-ccdada4e36ba"));

            migrationBuilder.DeleteData(
                table: "Nodes",
                keyColumn: "Id",
                keyValue: new Guid("26717097-8cf3-4474-a7a0-d77b6b2ce727"));

            migrationBuilder.DeleteData(
                table: "Nodes",
                keyColumn: "Id",
                keyValue: new Guid("4c5b4f07-6fef-4a57-809f-9bfa0ce92590"));
        }
    }
}
