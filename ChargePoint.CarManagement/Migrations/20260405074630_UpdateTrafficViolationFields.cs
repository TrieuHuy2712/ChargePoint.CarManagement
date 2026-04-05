using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ChargePoint.CarManagement.Migrations
{
    /// <inheritdoc />
    public partial class UpdateTrafficViolationFields : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "TongTienPhat",
                table: "TrafficViolationChecks");

            migrationBuilder.AddColumn<string>(
                name: "DiaDiemViPham",
                table: "TrafficViolationChecks",
                type: "TEXT",
                maxLength: 500,
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "NgayGioViPham",
                table: "TrafficViolationChecks",
                type: "TEXT",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "NoiDungViPham",
                table: "TrafficViolationChecks",
                type: "TEXT",
                maxLength: 500,
                nullable: true);

            migrationBuilder.UpdateData(
                table: "Cars",
                keyColumn: "Id",
                keyValue: 1,
                column: "NgayTao",
                value: new DateTime(2026, 4, 5, 14, 46, 30, 412, DateTimeKind.Local).AddTicks(6670));

            migrationBuilder.UpdateData(
                table: "Cars",
                keyColumn: "Id",
                keyValue: 2,
                column: "NgayTao",
                value: new DateTime(2026, 4, 5, 14, 46, 30, 412, DateTimeKind.Local).AddTicks(6674));
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "DiaDiemViPham",
                table: "TrafficViolationChecks");

            migrationBuilder.DropColumn(
                name: "NgayGioViPham",
                table: "TrafficViolationChecks");

            migrationBuilder.DropColumn(
                name: "NoiDungViPham",
                table: "TrafficViolationChecks");

            migrationBuilder.AddColumn<decimal>(
                name: "TongTienPhat",
                table: "TrafficViolationChecks",
                type: "decimal(18,0)",
                nullable: false,
                defaultValue: 0m);

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
    }
}
