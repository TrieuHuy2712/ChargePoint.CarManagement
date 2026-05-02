using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ChargePoint.CarManagement.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddSearchIndexes : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.UpdateData(
                table: "Cars",
                keyColumn: "Id",
                keyValue: 1,
                column: "NgayTao",
                value: new DateTime(2026, 5, 2, 13, 21, 8, 295, DateTimeKind.Local).AddTicks(5019));

            migrationBuilder.UpdateData(
                table: "Cars",
                keyColumn: "Id",
                keyValue: 2,
                column: "NgayTao",
                value: new DateTime(2026, 5, 2, 13, 21, 8, 295, DateTimeKind.Local).AddTicks(5024));

            migrationBuilder.CreateIndex(
                name: "idx_cars_bienso",
                table: "Cars",
                column: "BienSo");

            migrationBuilder.CreateIndex(
                name: "idx_cars_mauxe",
                table: "Cars",
                column: "MauXe");

            migrationBuilder.CreateIndex(
                name: "idx_cars_sovin",
                table: "Cars",
                column: "SoVIN");

            migrationBuilder.CreateIndex(
                name: "idx_cars_stt",
                table: "Cars",
                column: "Stt");

            migrationBuilder.CreateIndex(
                name: "idx_cars_tenkhachhang",
                table: "Cars",
                column: "TenKhachHang");

            migrationBuilder.CreateIndex(
                name: "idx_cars_tenxe",
                table: "Cars",
                column: "TenXe");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "idx_cars_bienso",
                table: "Cars");

            migrationBuilder.DropIndex(
                name: "idx_cars_mauxe",
                table: "Cars");

            migrationBuilder.DropIndex(
                name: "idx_cars_sovin",
                table: "Cars");

            migrationBuilder.DropIndex(
                name: "idx_cars_stt",
                table: "Cars");

            migrationBuilder.DropIndex(
                name: "idx_cars_tenkhachhang",
                table: "Cars");

            migrationBuilder.DropIndex(
                name: "idx_cars_tenxe",
                table: "Cars");

            migrationBuilder.UpdateData(
                table: "Cars",
                keyColumn: "Id",
                keyValue: 1,
                column: "NgayTao",
                value: new DateTime(2026, 5, 2, 12, 50, 39, 906, DateTimeKind.Local).AddTicks(5376));

            migrationBuilder.UpdateData(
                table: "Cars",
                keyColumn: "Id",
                keyValue: 2,
                column: "NgayTao",
                value: new DateTime(2026, 5, 2, 12, 50, 39, 906, DateTimeKind.Local).AddTicks(5380));
        }
    }
}
