using System;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace ChargePoint.CarManagement.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class InitialMySqlMigration : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterDatabase()
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "Cars",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    Stt = table.Column<int>(type: "int", nullable: false),
                    TenXe = table.Column<string>(type: "varchar(100)", maxLength: 100, nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    SoLuong = table.Column<int>(type: "int", nullable: false),
                    MauXe = table.Column<string>(type: "varchar(50)", maxLength: 50, nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    SoVIN = table.Column<string>(type: "varchar(17)", maxLength: 17, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    BienSo = table.Column<string>(type: "varchar(20)", maxLength: 20, nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    MauBienSo = table.Column<int>(type: "int", nullable: false),
                    BienSoCu = table.Column<string>(type: "varchar(20)", maxLength: 20, nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    TenKhachHang = table.Column<string>(type: "varchar(100)", maxLength: 100, nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    ThongTinChoThue = table.Column<string>(type: "varchar(500)", maxLength: 500, nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    NgayThue = table.Column<DateTime>(type: "datetime(6)", nullable: true),
                    NgayHetHan = table.Column<DateTime>(type: "datetime(6)", nullable: true),
                    OdoXe = table.Column<int>(type: "int", nullable: false),
                    PrimaryImageUrl = table.Column<string>(type: "varchar(1000)", maxLength: 1000, nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    NguoiTao = table.Column<string>(type: "varchar(100)", maxLength: 100, nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    NguoiCapNhat = table.Column<string>(type: "varchar(100)", maxLength: 100, nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    NgayTao = table.Column<DateTime>(type: "datetime(6)", nullable: false),
                    NgayCapNhat = table.Column<DateTime>(type: "datetime(6)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Cars", x => x.Id);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "SystemSettings",
                columns: table => new
                {
                    Key = table.Column<string>(type: "varchar(50)", maxLength: 50, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    Value = table.Column<string>(type: "longtext", nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    Description = table.Column<string>(type: "varchar(200)", maxLength: 200, nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    Type = table.Column<string>(type: "longtext", nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SystemSettings", x => x.Key);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "CarMedias",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    CarId = table.Column<int>(type: "int", nullable: false),
                    Type = table.Column<int>(type: "int", nullable: false),
                    Url = table.Column<string>(type: "varchar(1000)", maxLength: 1000, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    FileName = table.Column<string>(type: "varchar(260)", maxLength: 260, nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    IsPrimary = table.Column<bool>(type: "tinyint(1)", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime(6)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CarMedias", x => x.Id);
                    table.ForeignKey(
                        name: "FK_CarMedias_Cars_CarId",
                        column: x => x.CarId,
                        principalTable: "Cars",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "MaintenanceRecords",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    CarId = table.Column<int>(type: "int", nullable: false),
                    NgayBaoDuong = table.Column<DateTime>(type: "datetime(6)", nullable: false),
                    SoKmBaoDuong = table.Column<int>(type: "int", nullable: false),
                    CapBaoDuong = table.Column<int>(type: "int", nullable: true),
                    SoKmBaoDuongTiepTheo = table.Column<int>(type: "int", nullable: true),
                    NoiDungBaoDuong = table.Column<string>(type: "varchar(2000)", maxLength: 2000, nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    ChiPhi = table.Column<decimal>(type: "decimal(18,0)", nullable: false),
                    NoiBaoDuong = table.Column<string>(type: "varchar(200)", maxLength: 200, nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    HinhAnhChungTu = table.Column<string>(type: "longtext", nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    GhiChu = table.Column<string>(type: "varchar(1000)", maxLength: 1000, nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    LoaiHoSo = table.Column<int>(type: "int", nullable: false),
                    NguoiTao = table.Column<string>(type: "varchar(100)", maxLength: 100, nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    NguoiCapNhat = table.Column<string>(type: "varchar(100)", maxLength: 100, nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    NgayTao = table.Column<DateTime>(type: "datetime(6)", nullable: false),
                    NgayCapNhat = table.Column<DateTime>(type: "datetime(6)", nullable: true)
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
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "TireRecords",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    CarId = table.Column<int>(type: "int", nullable: false),
                    ViTriLop = table.Column<int>(type: "int", nullable: false),
                    LoaiThaoTac = table.Column<int>(type: "int", nullable: false),
                    NgayThucHien = table.Column<DateTime>(type: "datetime(6)", nullable: false),
                    OdoThayLop = table.Column<int>(type: "int", nullable: false),
                    HangLop = table.Column<string>(type: "varchar(100)", maxLength: 100, nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    ModelLop = table.Column<string>(type: "varchar(100)", maxLength: 100, nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    KichThuocLop = table.Column<string>(type: "varchar(50)", maxLength: 50, nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    OdoThayTiepTheo = table.Column<int>(type: "int", nullable: true),
                    ChiPhi = table.Column<decimal>(type: "decimal(18,0)", nullable: false),
                    NoiThucHien = table.Column<string>(type: "varchar(200)", maxLength: 200, nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    HinhAnhChungTu = table.Column<string>(type: "longtext", nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    HinhAnhDOT = table.Column<string>(type: "longtext", nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    GhiChu = table.Column<string>(type: "varchar(1000)", maxLength: 1000, nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    NguoiTao = table.Column<string>(type: "varchar(100)", maxLength: 100, nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    NguoiCapNhat = table.Column<string>(type: "varchar(100)", maxLength: 100, nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    NgayTao = table.Column<DateTime>(type: "datetime(6)", nullable: false),
                    NgayCapNhat = table.Column<DateTime>(type: "datetime(6)", nullable: true)
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
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "TrafficViolationChecks",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    CarId = table.Column<int>(type: "int", nullable: false),
                    NgayKiemTra = table.Column<DateTime>(type: "datetime(6)", nullable: false),
                    CoViPham = table.Column<bool>(type: "tinyint(1)", nullable: false),
                    SoLuongViPham = table.Column<int>(type: "int", nullable: false),
                    NgayGioViPham = table.Column<DateTime>(type: "datetime(6)", nullable: true),
                    NoiDungViPham = table.Column<string>(type: "varchar(500)", maxLength: 500, nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    DiaDiemViPham = table.Column<string>(type: "varchar(500)", maxLength: 500, nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    TrangThaiXuLy = table.Column<int>(type: "int", nullable: false),
                    NgayCapNhatTrangThai = table.Column<DateTime>(type: "datetime(6)", nullable: true),
                    NguoiXuLy = table.Column<string>(type: "varchar(100)", maxLength: 100, nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    GhiChu = table.Column<string>(type: "varchar(1000)", maxLength: 1000, nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    NguoiTao = table.Column<string>(type: "varchar(100)", maxLength: 100, nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    NguoiCapNhat = table.Column<string>(type: "varchar(100)", maxLength: 100, nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    NgayTao = table.Column<DateTime>(type: "datetime(6)", nullable: false),
                    NgayCapNhat = table.Column<DateTime>(type: "datetime(6)", nullable: true)
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
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.InsertData(
                table: "Cars",
                columns: new[] { "Id", "BienSo", "BienSoCu", "MauBienSo", "MauXe", "NgayCapNhat", "NgayHetHan", "NgayTao", "NgayThue", "NguoiCapNhat", "NguoiTao", "OdoXe", "PrimaryImageUrl", "SoLuong", "SoVIN", "Stt", "TenKhachHang", "TenXe", "ThongTinChoThue" },
                values: new object[,]
                {
                    { 1, "30A-12345", null, 0, "Đỏ", null, null, new DateTime(2026, 5, 2, 12, 50, 39, 906, DateTimeKind.Local).AddTicks(5376), null, null, null, 15000, null, 5, "LVSHCAMB1NE000001", 1, "Nguyễn Văn A", "VinFast VF8", "Xe cho thuê dài hạn" },
                    { 2, "30B-67890", null, 0, "Trắng", null, null, new DateTime(2026, 5, 2, 12, 50, 39, 906, DateTimeKind.Local).AddTicks(5380), null, null, null, 8000, null, 3, "LVSHCAMB2NE000002", 2, "Trần Thị B", "VinFast VF9", "Xe cho thuê ngắn hạn" }
                });

            migrationBuilder.InsertData(
                table: "SystemSettings",
                columns: new[] { "Key", "Description", "Type", "Value" },
                values: new object[,]
                {
                    { "AutoUpdateOdo_Maintenance", "Tự động cập nhật ODO của xe khi thêm hồ sơ bảo dưỡng", "boolean", "false" },
                    { "AutoUpdateOdo_Tire", "Tự động cập nhật ODO của xe khi thêm hồ sơ lốp", "boolean", "false" },
                    { "MaintenanceMode", "Bảo trì hệ thống (Chỉ root account mới được truy cập)", "boolean", "false" }
                });

            migrationBuilder.CreateIndex(
                name: "IX_CarMedias_CarId",
                table: "CarMedias",
                column: "CarId");

            migrationBuilder.CreateIndex(
                name: "IX_MaintenanceRecords_CarId",
                table: "MaintenanceRecords",
                column: "CarId");

            migrationBuilder.CreateIndex(
                name: "IX_TireRecords_CarId",
                table: "TireRecords",
                column: "CarId");

            migrationBuilder.CreateIndex(
                name: "IX_TrafficViolationChecks_CarId",
                table: "TrafficViolationChecks",
                column: "CarId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "CarMedias");

            migrationBuilder.DropTable(
                name: "MaintenanceRecords");

            migrationBuilder.DropTable(
                name: "SystemSettings");

            migrationBuilder.DropTable(
                name: "TireRecords");

            migrationBuilder.DropTable(
                name: "TrafficViolationChecks");

            migrationBuilder.DropTable(
                name: "Cars");
        }
    }
}
