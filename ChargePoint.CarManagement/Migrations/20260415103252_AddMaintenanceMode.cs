using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ChargePoint.CarManagement.Migrations
{
    /// <inheritdoc />
    public partial class AddMaintenanceMode : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.UpdateData(
                table: "Cars",
                keyColumn: "Id",
                keyValue: 1,
                column: "NgayTao",
                value: new DateTime(2026, 4, 15, 17, 32, 52, 55, DateTimeKind.Local).AddTicks(5868));

            migrationBuilder.UpdateData(
                table: "Cars",
                keyColumn: "Id",
                keyValue: 2,
                column: "NgayTao",
                value: new DateTime(2026, 4, 15, 17, 32, 52, 55, DateTimeKind.Local).AddTicks(5872));
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.UpdateData(
                table: "Cars",
                keyColumn: "Id",
                keyValue: 1,
                column: "NgayTao",
                value: new DateTime(2026, 4, 15, 17, 26, 34, 404, DateTimeKind.Local).AddTicks(1879));

            migrationBuilder.UpdateData(
                table: "Cars",
                keyColumn: "Id",
                keyValue: 2,
                column: "NgayTao",
                value: new DateTime(2026, 4, 15, 17, 26, 34, 404, DateTimeKind.Local).AddTicks(1883));
        }
    }
}
