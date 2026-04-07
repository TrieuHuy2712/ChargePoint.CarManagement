using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ChargePoint.CarManagement.Migrations
{
    /// <inheritdoc />
    public partial class AddNgayThueVaNgayHetHan : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<DateTime>(
                name: "NgayHetHan",
                table: "Cars",
                type: "TEXT",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "NgayThue",
                table: "Cars",
                type: "TEXT",
                nullable: true);

            migrationBuilder.UpdateData(
                table: "Cars",
                keyColumn: "Id",
                keyValue: 1,
                columns: new[] { "NgayHetHan", "NgayTao", "NgayThue" },
                values: new object[] { null, new DateTime(2026, 4, 7, 21, 57, 18, 358, DateTimeKind.Local).AddTicks(5794), null });

            migrationBuilder.UpdateData(
                table: "Cars",
                keyColumn: "Id",
                keyValue: 2,
                columns: new[] { "NgayHetHan", "NgayTao", "NgayThue" },
                values: new object[] { null, new DateTime(2026, 4, 7, 21, 57, 18, 358, DateTimeKind.Local).AddTicks(5801), null });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "NgayHetHan",
                table: "Cars");

            migrationBuilder.DropColumn(
                name: "NgayThue",
                table: "Cars");

            migrationBuilder.UpdateData(
                table: "Cars",
                keyColumn: "Id",
                keyValue: 1,
                column: "NgayTao",
                value: new DateTime(2026, 4, 5, 20, 40, 14, 72, DateTimeKind.Local).AddTicks(5189));

            migrationBuilder.UpdateData(
                table: "Cars",
                keyColumn: "Id",
                keyValue: 2,
                column: "NgayTao",
                value: new DateTime(2026, 4, 5, 20, 40, 14, 72, DateTimeKind.Local).AddTicks(5192));
        }
    }
}
