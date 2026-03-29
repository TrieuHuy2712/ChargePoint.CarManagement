using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace ChargePoint.CarManagement.Migrations
{
    /// <inheritdoc />
    public partial class AddVideoFields : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Cars",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    Stt = table.Column<int>(type: "INTEGER", nullable: false),
                    TenXe = table.Column<string>(type: "TEXT", maxLength: 100, nullable: false),
                    SoLuong = table.Column<int>(type: "INTEGER", nullable: false),
                    MauXe = table.Column<string>(type: "TEXT", maxLength: 50, nullable: false),
                    SoVIN = table.Column<string>(type: "TEXT", maxLength: 17, nullable: false),
                    BienSo = table.Column<string>(type: "TEXT", maxLength: 20, nullable: true),
                    TenKhachHang = table.Column<string>(type: "TEXT", maxLength: 100, nullable: true),
                    ThongTinChoThue = table.Column<string>(type: "TEXT", maxLength: 500, nullable: true),
                    OdoXe = table.Column<int>(type: "INTEGER", nullable: false),
                    HinhAnhNhanBanGiao = table.Column<string>(type: "TEXT", nullable: true),
                    HinhAnhBanGiaoKH = table.Column<string>(type: "TEXT", nullable: true),
                    VideoNhanBanGiao = table.Column<string>(type: "TEXT", nullable: true),
                    VideoBanGiaoKH = table.Column<string>(type: "TEXT", nullable: true),
                    NgayTao = table.Column<DateTime>(type: "TEXT", nullable: false),
                    NgayCapNhat = table.Column<DateTime>(type: "TEXT", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Cars", x => x.Id);
                });

            migrationBuilder.InsertData(
                table: "Cars",
                columns: new[] { "Id", "BienSo", "HinhAnhBanGiaoKH", "HinhAnhNhanBanGiao", "MauXe", "NgayCapNhat", "NgayTao", "OdoXe", "SoLuong", "SoVIN", "Stt", "TenKhachHang", "TenXe", "ThongTinChoThue", "VideoBanGiaoKH", "VideoNhanBanGiao" },
                values: new object[,]
                {
                    { 1, "30A-12345", null, null, "Đỏ", null, new DateTime(2026, 3, 26, 0, 26, 29, 894, DateTimeKind.Local).AddTicks(2915), 15000, 5, "LVSHCAMB1NE000001", 1, "Nguyễn Văn A", "VinFast VF8", "Xe cho thuê dài hạn", null, null },
                    { 2, "30B-67890", null, null, "Trắng", null, new DateTime(2026, 3, 26, 0, 26, 29, 894, DateTimeKind.Local).AddTicks(2918), 8000, 3, "LVSHCAMB2NE000002", 2, "Trần Thị B", "VinFast VF9", "Xe cho thuê ngắn hạn", null, null }
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Cars");
        }
    }
}
