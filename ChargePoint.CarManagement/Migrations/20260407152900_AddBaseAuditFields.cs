using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ChargePoint.CarManagement.Migrations
{
    /// <inheritdoc />
    public partial class AddBaseAuditFields : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "NguoiCapNhat",
                table: "TireRecords",
                type: "TEXT",
                maxLength: 100,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "NguoiCapNhat",
                table: "MaintenanceRecords",
                type: "TEXT",
                maxLength: 100,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "NguoiCapNhat",
                table: "Cars",
                type: "TEXT",
                maxLength: 100,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "NguoiTao",
                table: "Cars",
                type: "TEXT",
                maxLength: 100,
                nullable: true);

            migrationBuilder.UpdateData(
                table: "Cars",
                keyColumn: "Id",
                keyValue: 1,
                columns: new[] { "NgayTao", "NguoiCapNhat", "NguoiTao" },
                values: new object[] { new DateTime(2026, 4, 7, 22, 28, 59, 799, DateTimeKind.Local).AddTicks(8087), null, null });

            migrationBuilder.UpdateData(
                table: "Cars",
                keyColumn: "Id",
                keyValue: 2,
                columns: new[] { "NgayTao", "NguoiCapNhat", "NguoiTao" },
                values: new object[] { new DateTime(2026, 4, 7, 22, 28, 59, 799, DateTimeKind.Local).AddTicks(8092), null, null });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "NguoiCapNhat",
                table: "TireRecords");

            migrationBuilder.DropColumn(
                name: "NguoiCapNhat",
                table: "MaintenanceRecords");

            migrationBuilder.DropColumn(
                name: "NguoiCapNhat",
                table: "Cars");

            migrationBuilder.DropColumn(
                name: "NguoiTao",
                table: "Cars");

            migrationBuilder.UpdateData(
                table: "Cars",
                keyColumn: "Id",
                keyValue: 1,
                column: "NgayTao",
                value: new DateTime(2026, 4, 7, 21, 57, 18, 358, DateTimeKind.Local).AddTicks(5794));

            migrationBuilder.UpdateData(
                table: "Cars",
                keyColumn: "Id",
                keyValue: 2,
                column: "NgayTao",
                value: new DateTime(2026, 4, 7, 21, 57, 18, 358, DateTimeKind.Local).AddTicks(5801));
        }
    }
}
