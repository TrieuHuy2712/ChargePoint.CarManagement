using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ChargePoint.CarManagement.Migrations
{
    /// <inheritdoc />
    public partial class AddBaseAuditFieldsForTrafficViolation : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "NguoiKiemTra",
                table: "TrafficViolationChecks",
                newName: "NguoiTao");

            migrationBuilder.AddColumn<DateTime>(
                name: "NgayCapNhat",
                table: "TrafficViolationChecks",
                type: "TEXT",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "NgayTao",
                table: "TrafficViolationChecks",
                type: "TEXT",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.AddColumn<string>(
                name: "NguoiCapNhat",
                table: "TrafficViolationChecks",
                type: "TEXT",
                maxLength: 100,
                nullable: true);

            migrationBuilder.UpdateData(
                table: "Cars",
                keyColumn: "Id",
                keyValue: 1,
                column: "NgayTao",
                value: new DateTime(2026, 4, 7, 22, 37, 59, 614, DateTimeKind.Local).AddTicks(2280));

            migrationBuilder.UpdateData(
                table: "Cars",
                keyColumn: "Id",
                keyValue: 2,
                column: "NgayTao",
                value: new DateTime(2026, 4, 7, 22, 37, 59, 614, DateTimeKind.Local).AddTicks(2285));
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "NgayCapNhat",
                table: "TrafficViolationChecks");

            migrationBuilder.DropColumn(
                name: "NgayTao",
                table: "TrafficViolationChecks");

            migrationBuilder.DropColumn(
                name: "NguoiCapNhat",
                table: "TrafficViolationChecks");

            migrationBuilder.RenameColumn(
                name: "NguoiTao",
                table: "TrafficViolationChecks",
                newName: "NguoiKiemTra");

            migrationBuilder.UpdateData(
                table: "Cars",
                keyColumn: "Id",
                keyValue: 1,
                column: "NgayTao",
                value: new DateTime(2026, 4, 7, 22, 28, 59, 799, DateTimeKind.Local).AddTicks(8087));

            migrationBuilder.UpdateData(
                table: "Cars",
                keyColumn: "Id",
                keyValue: 2,
                column: "NgayTao",
                value: new DateTime(2026, 4, 7, 22, 28, 59, 799, DateTimeKind.Local).AddTicks(8092));
        }
    }
}
