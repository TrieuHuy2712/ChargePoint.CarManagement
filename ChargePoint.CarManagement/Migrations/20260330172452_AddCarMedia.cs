using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ChargePoint.CarManagement.Migrations
{
    /// <inheritdoc />
    public partial class AddCarMedia : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "HinhAnhBanGiaoKH",
                table: "Cars");

            migrationBuilder.DropColumn(
                name: "HinhAnhNhanBanGiao",
                table: "Cars");

            migrationBuilder.DropColumn(
                name: "VideoBanGiaoKH",
                table: "Cars");

            migrationBuilder.DropColumn(
                name: "VideoNhanBanGiao",
                table: "Cars");

            migrationBuilder.AddColumn<string>(
                name: "PrimaryImageUrl",
                table: "Cars",
                type: "TEXT",
                maxLength: 1000,
                nullable: true);

            migrationBuilder.CreateTable(
                name: "CarMedias",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    CarId = table.Column<int>(type: "INTEGER", nullable: false),
                    Type = table.Column<int>(type: "INTEGER", nullable: false),
                    Url = table.Column<string>(type: "TEXT", maxLength: 1000, nullable: false),
                    FileName = table.Column<string>(type: "TEXT", maxLength: 260, nullable: true),
                    IsPrimary = table.Column<bool>(type: "INTEGER", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "TEXT", nullable: false)
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
                });

            migrationBuilder.UpdateData(
                table: "Cars",
                keyColumn: "Id",
                keyValue: 1,
                columns: new[] { "NgayTao", "PrimaryImageUrl" },
                values: new object[] { new DateTime(2026, 3, 31, 0, 24, 52, 652, DateTimeKind.Local).AddTicks(7758), null });

            migrationBuilder.UpdateData(
                table: "Cars",
                keyColumn: "Id",
                keyValue: 2,
                columns: new[] { "NgayTao", "PrimaryImageUrl" },
                values: new object[] { new DateTime(2026, 3, 31, 0, 24, 52, 652, DateTimeKind.Local).AddTicks(7761), null });

            migrationBuilder.CreateIndex(
                name: "IX_CarMedias_CarId",
                table: "CarMedias",
                column: "CarId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "CarMedias");

            migrationBuilder.DropColumn(
                name: "PrimaryImageUrl",
                table: "Cars");

            migrationBuilder.AddColumn<string>(
                name: "HinhAnhBanGiaoKH",
                table: "Cars",
                type: "TEXT",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "HinhAnhNhanBanGiao",
                table: "Cars",
                type: "TEXT",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "VideoBanGiaoKH",
                table: "Cars",
                type: "TEXT",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "VideoNhanBanGiao",
                table: "Cars",
                type: "TEXT",
                nullable: true);

            migrationBuilder.UpdateData(
                table: "Cars",
                keyColumn: "Id",
                keyValue: 1,
                columns: new[] { "HinhAnhBanGiaoKH", "HinhAnhNhanBanGiao", "NgayTao", "VideoBanGiaoKH", "VideoNhanBanGiao" },
                values: new object[] { null, null, new DateTime(2026, 3, 26, 23, 14, 51, 212, DateTimeKind.Local).AddTicks(3114), null, null });

            migrationBuilder.UpdateData(
                table: "Cars",
                keyColumn: "Id",
                keyValue: 2,
                columns: new[] { "HinhAnhBanGiaoKH", "HinhAnhNhanBanGiao", "NgayTao", "VideoBanGiaoKH", "VideoNhanBanGiao" },
                values: new object[] { null, null, new DateTime(2026, 3, 26, 23, 14, 51, 212, DateTimeKind.Local).AddTicks(3117), null, null });
        }
    }
}
