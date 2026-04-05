using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ChargePoint.CarManagement.Migrations
{
    /// <inheritdoc />
    public partial class AddBienSoCu : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "BienSoCu",
                table: "Cars",
                type: "TEXT",
                maxLength: 20,
                nullable: true);

            migrationBuilder.UpdateData(
                table: "Cars",
                keyColumn: "Id",
                keyValue: 1,
                columns: new[] { "BienSoCu", "NgayTao" },
                values: new object[] { null, new DateTime(2026, 4, 5, 20, 40, 14, 72, DateTimeKind.Local).AddTicks(5189) });

            migrationBuilder.UpdateData(
                table: "Cars",
                keyColumn: "Id",
                keyValue: 2,
                columns: new[] { "BienSoCu", "NgayTao" },
                values: new object[] { null, new DateTime(2026, 4, 5, 20, 40, 14, 72, DateTimeKind.Local).AddTicks(5192) });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "BienSoCu",
                table: "Cars");

            migrationBuilder.UpdateData(
                table: "Cars",
                keyColumn: "Id",
                keyValue: 1,
                column: "NgayTao",
                value: new DateTime(2026, 4, 5, 19, 12, 33, 274, DateTimeKind.Local).AddTicks(4402));

            migrationBuilder.UpdateData(
                table: "Cars",
                keyColumn: "Id",
                keyValue: 2,
                column: "NgayTao",
                value: new DateTime(2026, 4, 5, 19, 12, 33, 274, DateTimeKind.Local).AddTicks(4405));
        }
    }
}
