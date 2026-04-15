using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ChargePoint.CarManagement.Migrations
{
    /// <inheritdoc />
    public partial class AddMaintenanceModeSetting : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
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

            migrationBuilder.InsertData(
                table: "SystemSettings",
                columns: new[] { "Key", "Description", "Type", "Value" },
                values: new object[] { "MaintenanceMode", "Bảo trì hệ thống (Chỉ root account mới được truy cập)", "boolean", "false" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DeleteData(
                table: "SystemSettings",
                keyColumn: "Key",
                keyValue: "MaintenanceMode");

            migrationBuilder.UpdateData(
                table: "Cars",
                keyColumn: "Id",
                keyValue: 1,
                column: "NgayTao",
                value: new DateTime(2026, 4, 15, 16, 43, 22, 794, DateTimeKind.Local).AddTicks(7206));

            migrationBuilder.UpdateData(
                table: "Cars",
                keyColumn: "Id",
                keyValue: 2,
                column: "NgayTao",
                value: new DateTime(2026, 4, 15, 16, 43, 22, 794, DateTimeKind.Local).AddTicks(7210));
        }
    }
}
