using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ChargePoint.CarManagement.Migrations
{
    /// <inheritdoc />
    public partial class AddViolationStatusTracking : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<DateTime>(
                name: "NgayCapNhatTrangThai",
                table: "TrafficViolationChecks",
                type: "TEXT",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "NguoiXuLy",
                table: "TrafficViolationChecks",
                type: "TEXT",
                maxLength: 100,
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "TrangThaiXuLy",
                table: "TrafficViolationChecks",
                type: "INTEGER",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.UpdateData(
                table: "Cars",
                keyColumn: "Id",
                keyValue: 1,
                column: "NgayTao",
                value: new DateTime(2026, 4, 5, 14, 41, 45, 719, DateTimeKind.Local).AddTicks(1008));

            migrationBuilder.UpdateData(
                table: "Cars",
                keyColumn: "Id",
                keyValue: 2,
                column: "NgayTao",
                value: new DateTime(2026, 4, 5, 14, 41, 45, 719, DateTimeKind.Local).AddTicks(1012));
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "NgayCapNhatTrangThai",
                table: "TrafficViolationChecks");

            migrationBuilder.DropColumn(
                name: "NguoiXuLy",
                table: "TrafficViolationChecks");

            migrationBuilder.DropColumn(
                name: "TrangThaiXuLy",
                table: "TrafficViolationChecks");

            migrationBuilder.UpdateData(
                table: "Cars",
                keyColumn: "Id",
                keyValue: 1,
                column: "NgayTao",
                value: new DateTime(2026, 4, 4, 22, 59, 8, 71, DateTimeKind.Local).AddTicks(1869));

            migrationBuilder.UpdateData(
                table: "Cars",
                keyColumn: "Id",
                keyValue: 2,
                column: "NgayTao",
                value: new DateTime(2026, 4, 4, 22, 59, 8, 71, DateTimeKind.Local).AddTicks(1873));
        }
    }
}
