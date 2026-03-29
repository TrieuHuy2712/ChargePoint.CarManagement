using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ChargePoint.CarManagement.Migrations
{
    /// <inheritdoc />
    public partial class AddTireRecord : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "TireRecords",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    CarId = table.Column<int>(type: "INTEGER", nullable: false),
                    ViTriLop = table.Column<int>(type: "INTEGER", nullable: false),
                    LoaiThaoTac = table.Column<int>(type: "INTEGER", nullable: false),
                    NgayThucHien = table.Column<DateTime>(type: "TEXT", nullable: false),
                    OdoThayLop = table.Column<int>(type: "INTEGER", nullable: false),
                    HangLop = table.Column<string>(type: "TEXT", maxLength: 100, nullable: true),
                    ModelLop = table.Column<string>(type: "TEXT", maxLength: 100, nullable: true),
                    KichThuocLop = table.Column<string>(type: "TEXT", maxLength: 50, nullable: true),
                    OdoThayTiepTheo = table.Column<int>(type: "INTEGER", nullable: true),
                    ChiPhi = table.Column<decimal>(type: "decimal(18,0)", nullable: false),
                    NoiThucHien = table.Column<string>(type: "TEXT", maxLength: 200, nullable: true),
                    HinhAnhChungTu = table.Column<string>(type: "TEXT", nullable: true),
                    GhiChu = table.Column<string>(type: "TEXT", maxLength: 1000, nullable: true),
                    NguoiTao = table.Column<string>(type: "TEXT", maxLength: 100, nullable: true),
                    NgayTao = table.Column<DateTime>(type: "TEXT", nullable: false),
                    NgayCapNhat = table.Column<DateTime>(type: "TEXT", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TireRecords", x => x.Id);
                    table.ForeignKey(
                        name: "FK_TireRecords_Cars_CarId",
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
                value: new DateTime(2026, 3, 26, 23, 14, 51, 212, DateTimeKind.Local).AddTicks(3114));

            migrationBuilder.UpdateData(
                table: "Cars",
                keyColumn: "Id",
                keyValue: 2,
                column: "NgayTao",
                value: new DateTime(2026, 3, 26, 23, 14, 51, 212, DateTimeKind.Local).AddTicks(3117));

            migrationBuilder.CreateIndex(
                name: "IX_TireRecords_CarId",
                table: "TireRecords",
                column: "CarId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "TireRecords");

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
        }
    }
}
