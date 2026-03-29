using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ChargePoint.CarManagement.Migrations
{
    /// <inheritdoc />
    public partial class AddMaintenanceRecord : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "MaintenanceRecords",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    CarId = table.Column<int>(type: "INTEGER", nullable: false),
                    NgayBaoDuong = table.Column<DateTime>(type: "TEXT", nullable: false),
                    SoKmBaoDuong = table.Column<int>(type: "INTEGER", nullable: false),
                    CapBaoDuong = table.Column<int>(type: "INTEGER", nullable: false),
                    NgayBaoDuongTiepTheo = table.Column<DateTime>(type: "TEXT", nullable: true),
                    SoKmBaoDuongTiepTheo = table.Column<int>(type: "INTEGER", nullable: true),
                    NoiDungBaoDuong = table.Column<string>(type: "TEXT", maxLength: 2000, nullable: true),
                    ChiPhi = table.Column<decimal>(type: "decimal(18,0)", nullable: false),
                    NoiBaoDuong = table.Column<string>(type: "TEXT", maxLength: 200, nullable: true),
                    HinhAnhChungTu = table.Column<string>(type: "TEXT", nullable: true),
                    GhiChu = table.Column<string>(type: "TEXT", maxLength: 1000, nullable: true),
                    NguoiTao = table.Column<string>(type: "TEXT", maxLength: 100, nullable: true),
                    NgayTao = table.Column<DateTime>(type: "TEXT", nullable: false),
                    NgayCapNhat = table.Column<DateTime>(type: "TEXT", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_MaintenanceRecords", x => x.Id);
                    table.ForeignKey(
                        name: "FK_MaintenanceRecords_Cars_CarId",
                        column: x => x.CarId,
                        principalTable: "Cars",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.UpdateData(
                table: "Cars",
                keyColumn: "Id",
                keyValue: 1,
                column: "NgayTao",
                value: new DateTime(2026, 3, 26, 22, 54, 53, 588, DateTimeKind.Local).AddTicks(6468));

            migrationBuilder.UpdateData(
                table: "Cars",
                keyColumn: "Id",
                keyValue: 2,
                column: "NgayTao",
                value: new DateTime(2026, 3, 26, 22, 54, 53, 588, DateTimeKind.Local).AddTicks(6471));

            migrationBuilder.CreateIndex(
                name: "IX_MaintenanceRecords_CarId",
                table: "MaintenanceRecords",
                column: "CarId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "MaintenanceRecords");

            migrationBuilder.UpdateData(
                table: "Cars",
                keyColumn: "Id",
                keyValue: 1,
                column: "NgayTao",
                value: new DateTime(2026, 3, 26, 22, 3, 13, 449, DateTimeKind.Local).AddTicks(1247));

            migrationBuilder.UpdateData(
                table: "Cars",
                keyColumn: "Id",
                keyValue: 2,
                column: "NgayTao",
                value: new DateTime(2026, 3, 26, 22, 3, 13, 449, DateTimeKind.Local).AddTicks(1252));
        }
    }
}
