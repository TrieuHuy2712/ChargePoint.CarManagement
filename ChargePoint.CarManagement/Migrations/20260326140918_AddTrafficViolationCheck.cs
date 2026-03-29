using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ChargePoint.CarManagement.Migrations
{
    /// <inheritdoc />
    public partial class AddTrafficViolationCheck : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "TrafficViolationChecks",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    CarId = table.Column<int>(type: "INTEGER", nullable: false),
                    NgayKiemTra = table.Column<DateTime>(type: "TEXT", nullable: false),
                    CoViPham = table.Column<bool>(type: "INTEGER", nullable: false),
                    SoLuongViPham = table.Column<int>(type: "INTEGER", nullable: false),
                    TongTienPhat = table.Column<decimal>(type: "decimal(18,0)", nullable: false),
                    GhiChu = table.Column<string>(type: "TEXT", maxLength: 1000, nullable: true),
                    NguoiKiemTra = table.Column<string>(type: "TEXT", maxLength: 100, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TrafficViolationChecks", x => x.Id);
                    table.ForeignKey(
                        name: "FK_TrafficViolationChecks_Cars_CarId",
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
                value: new DateTime(2026, 3, 26, 21, 9, 18, 374, DateTimeKind.Local).AddTicks(8903));

            migrationBuilder.UpdateData(
                table: "Cars",
                keyColumn: "Id",
                keyValue: 2,
                column: "NgayTao",
                value: new DateTime(2026, 3, 26, 21, 9, 18, 374, DateTimeKind.Local).AddTicks(8907));

            migrationBuilder.CreateIndex(
                name: "IX_TrafficViolationChecks_CarId",
                table: "TrafficViolationChecks",
                column: "CarId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "TrafficViolationChecks");

            migrationBuilder.UpdateData(
                table: "Cars",
                keyColumn: "Id",
                keyValue: 1,
                column: "NgayTao",
                value: new DateTime(2026, 3, 26, 0, 29, 0, 397, DateTimeKind.Local).AddTicks(723));

            migrationBuilder.UpdateData(
                table: "Cars",
                keyColumn: "Id",
                keyValue: 2,
                column: "NgayTao",
                value: new DateTime(2026, 3, 26, 0, 29, 0, 397, DateTimeKind.Local).AddTicks(727));
        }
    }
}
