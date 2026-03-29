using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ChargePoint.CarManagement.Migrations
{
    /// <inheritdoc />
    public partial class AddMauBienSo : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "MauBienSo",
                table: "Cars",
                type: "INTEGER",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.UpdateData(
                table: "Cars",
                keyColumn: "Id",
                keyValue: 1,
                columns: new[] { "MauBienSo", "NgayTao" },
                values: new object[] { 0, new DateTime(2026, 3, 26, 22, 3, 13, 449, DateTimeKind.Local).AddTicks(1247) });

            migrationBuilder.UpdateData(
                table: "Cars",
                keyColumn: "Id",
                keyValue: 2,
                columns: new[] { "MauBienSo", "NgayTao" },
                values: new object[] { 0, new DateTime(2026, 3, 26, 22, 3, 13, 449, DateTimeKind.Local).AddTicks(1252) });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "MauBienSo",
                table: "Cars");

            migrationBuilder.UpdateData(
                table: "Cars",
                keyColumn: "Id",
                keyValue: 1,
                column: "NgayTao",
                value: new DateTime(2026, 3, 26, 21, 9, 18, 374, DateTimeKind.Local).AddTicks(8903));

            migrationBuilder.UpdateData(
                table: "Cars",
                keyColumn: "Id",
                keyValue: 2,
                column: "NgayTao",
                value: new DateTime(2026, 3, 26, 21, 9, 18, 374, DateTimeKind.Local).AddTicks(8907));
        }
    }
}
