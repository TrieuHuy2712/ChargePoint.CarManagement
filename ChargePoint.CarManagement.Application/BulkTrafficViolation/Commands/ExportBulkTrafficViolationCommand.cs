using ChargePoint.CarManagement.Domain.Models;
using ChargePoint.CarManagement.Domain.Models.TrafficViolation;
using ChargePoint.CarManagement.Domain.ViewModels.TrafficViolationViewModels;
using Mediator;
using Microsoft.Extensions.Logging;
using OfficeOpenXml;
using OfficeOpenXml.Style;

namespace ChargePoint.CarManagement.Application.BulkTrafficViolation.Commands
{
    public class ExportBulkTrafficViolationCommand : IRequest<Result<ExportedFileResult>>
    {
        public List<TrafficViolationBulkCheckVM> CheckVMs { get; set; }
    }

    public class ExportBulkTrafficViolationCommandHandler(
        ILogger<ExportBulkTrafficViolationCommandHandler> logger) : IRequestHandler<ExportBulkTrafficViolationCommand, Result<ExportedFileResult>>
    {
        private readonly ILogger<ExportBulkTrafficViolationCommandHandler> _logger = logger;
        public async ValueTask<Result<ExportedFileResult>> Handle(ExportBulkTrafficViolationCommand cmd, CancellationToken cancellationToken)
        {

            if (cmd.CheckVMs == null || !cmd.CheckVMs.Any())
            {
                return Result<ExportedFileResult>.Fail("No traffic violation data provided.");
            }

            using (var package = new ExcelPackage())
            {
                // Create Summary worksheet
                var summarySheet = package.Workbook.Worksheets.Add("Tổng hợp");
                CreateSummarySheet(summarySheet, cmd.CheckVMs);

                // Create All Results worksheet
                var allSheet = package.Workbook.Worksheets.Add("Tất cả kết quả");
                CreateAllResultsSheet(allSheet, cmd.CheckVMs);
                // Create Violations worksheet
                var violations = cmd.CheckVMs.Where(r => r.SoLuongViPham > 0 && !r.IsError).ToList();
                if (violations.Any())
                {
                    var violationsSheet = package.Workbook.Worksheets.Add("Có vi phạm");
                    CreateViolationsSheet(violationsSheet, violations);
                }

                // Create Clean worksheet
                var clean = cmd.CheckVMs.Where(r => r.SoLuongViPham == 0 && !r.IsError).ToList();
                if (clean.Any())
                {
                    var cleanSheet = package.Workbook.Worksheets.Add("Không vi phạm");
                    CreateCleanSheet(cleanSheet, clean);
                }

                // Generate file
                var fileName = $"KetQuaTraCuu_{DateTime.Now:yyyyMMdd_HHmmss}.xlsx";
                var fileBytes = package.GetAsByteArray();

                return Result<ExportedFileResult>.Ok(new ExportedFileResult
                {
                    FileContents = fileBytes,
                    ContentType = "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",
                    FileDownloadName = fileName
                });
            }

        }

        #region Private Helper Methods

        private static void CreateSummarySheet(ExcelWorksheet sheet, List<TrafficViolationBulkCheckVM> results)
        {
            // Title
            sheet.Cells["A1"].Value = "BÁO CÁO KẾT QUẢ TRA CỨU PHẠT NGUỘI HÀNG LOẠT";
            sheet.Cells["A1:D1"].Merge = true;
            sheet.Cells["A1"].Style.Font.Size = 16;
            sheet.Cells["A1"].Style.Font.Bold = true;
            sheet.Cells["A1"].Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;

            sheet.Cells["A2"].Value = $"Ngày xuất: {DateTime.Now:dd/MM/yyyy HH:mm:ss}";
            sheet.Cells["A2:D2"].Merge = true;
            sheet.Cells["A2"].Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;

            // Statistics
            var totalChecked = results.Count;
            var totalViolations = results.Count(r => r.SoLuongViPham > 0);
            var totalClean = results.Count(r => r.SoLuongViPham == 0);
            var totalViolationCount = results.Sum(r => r.SoLuongViPham);

            int carsChuaDongPhat = 0;
            int carsDaDongPhat = 0;
            int luotChuaDongPhat = 0;
            int luotDaDongPhat = 0;

            foreach (var r in results.Where(x => x.SoLuongViPham > 0))
            {
                bool hasChuaDong = false;
                bool hasDaDong = false;

                if (r.DanhSachViPham != null && r.DanhSachViPham.Any())
                {
                    foreach (var vp in r.DanhSachViPham)
                    {
                        var status = vp.TrangThai ?? r.TrangThaiCSGT ?? "";
                        if (status.IndexOf("Đã xử phạt", StringComparison.OrdinalIgnoreCase) >= 0)
                        {
                            luotDaDongPhat++;
                            hasDaDong = true;
                        }
                        else
                        {
                            luotChuaDongPhat++;
                            hasChuaDong = true;
                        }
                    }
                }
                else
                {
                    var status = r.TrangThaiCSGT ?? "";
                    if (status.IndexOf("Đã xử phạt", StringComparison.OrdinalIgnoreCase) >= 0)
                    {
                        luotDaDongPhat += r.SoLuongViPham;
                        hasDaDong = true;
                    }
                    else
                    {
                        luotChuaDongPhat += r.SoLuongViPham;
                        hasChuaDong = true;
                    }
                }

                if (hasChuaDong) carsChuaDongPhat++;
                if (hasDaDong) carsDaDongPhat++;
            }

            sheet.Cells["A4"].Value = "THỐNG KÊ";
            sheet.Cells["A4"].Style.Font.Bold = true;
            sheet.Cells["A4"].Style.Font.Size = 14;

            sheet.Cells["A6"].Value = "Tổng số xe đã tra cứu:";
            sheet.Cells["B6"].Value = totalChecked;
            sheet.Cells["B6"].Style.Font.Bold = true;

            sheet.Cells["A7"].Value = "Số xe không vi phạm:";
            sheet.Cells["B7"].Value = totalClean;
            sheet.Cells["B7"].Style.Font.Color.SetColor(System.Drawing.Color.Green);
            sheet.Cells["B7"].Style.Font.Bold = true;

            sheet.Cells["A8"].Value = "Số xe có vi phạm:";
            sheet.Cells["B8"].Value = totalViolations;
            sheet.Cells["B8"].Style.Font.Color.SetColor(System.Drawing.Color.Red);
            sheet.Cells["B8"].Style.Font.Bold = true;

            sheet.Cells["A9"].Value = "Tổng số vi phạm:";
            sheet.Cells["B9"].Value = totalViolationCount;
            sheet.Cells["B9"].Style.Font.Color.SetColor(System.Drawing.Color.DarkRed);
            sheet.Cells["B9"].Style.Font.Bold = true;

            sheet.Cells["A10"].Value = "Tỷ lệ vi phạm:";
            sheet.Cells["B10"].Value = totalChecked > 0 ? (totalViolations * 100.0 / totalChecked).ToString("0.00") + "%" : "0%";
            sheet.Cells["B10"].Style.Font.Bold = true;

            sheet.Cells["A12"].Value = "THÔNG TIN ĐÓNG PHẠT";
            sheet.Cells["A12"].Style.Font.Bold = true;
            sheet.Cells["A12"].Style.Font.Size = 14;

            sheet.Cells["A14"].Value = "Số xe chưa đóng phạt:";
            sheet.Cells["B14"].Value = carsChuaDongPhat;
            sheet.Cells["B14"].Style.Font.Color.SetColor(System.Drawing.Color.OrangeRed);
            sheet.Cells["B14"].Style.Font.Bold = true;

            sheet.Cells["A15"].Value = "Số lượng lỗi chưa đóng phạt:";
            sheet.Cells["B15"].Value = luotChuaDongPhat;
            sheet.Cells["B15"].Style.Font.Color.SetColor(System.Drawing.Color.OrangeRed);
            sheet.Cells["B15"].Style.Font.Bold = true;

            sheet.Cells["A16"].Value = "Số xe đã đóng phạt:";
            sheet.Cells["B16"].Value = carsDaDongPhat;
            sheet.Cells["B16"].Style.Font.Color.SetColor(System.Drawing.Color.Blue);
            sheet.Cells["B16"].Style.Font.Bold = true;

            sheet.Cells["A17"].Value = "Số lượng lỗi đã đóng phạt:";
            sheet.Cells["B17"].Value = luotDaDongPhat;
            sheet.Cells["B17"].Style.Font.Color.SetColor(System.Drawing.Color.Blue);
            sheet.Cells["B17"].Style.Font.Bold = true;

            // Auto-fit columns
            sheet.Cells[sheet.Dimension.Address].AutoFitColumns();
        }

        private static void CreateAllResultsSheet(ExcelWorksheet sheet, List<TrafficViolationBulkCheckVM> results)
        {
            // Headers
            sheet.Cells["A1"].Value = "STT";
            sheet.Cells["B1"].Value = "Biển số";
            sheet.Cells["C1"].Value = "Trạng thái phạt nguội";
            sheet.Cells["D1"].Value = "Ngày giờ vi phạm";
            sheet.Cells["E1"].Value = "Nội dung vi phạm";
            sheet.Cells["F1"].Value = "Địa điểm vi phạm";
            sheet.Cells["G1"].Value = "Chi tiết đầy đủ (Tất cả lỗi)";
            sheet.Cells["H1"].Value = "Ghi chú";

            // Style headers
            using (var range = sheet.Cells["A1:H1"])
            {
                range.Style.Font.Bold = true;
                range.Style.Fill.PatternType = ExcelFillStyle.Solid;
                range.Style.Fill.BackgroundColor.SetColor(System.Drawing.Color.LightBlue);
                range.Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;
                range.Style.Border.BorderAround(ExcelBorderStyle.Thin);
            }

            // Data
            int row = 2;
            int stt = 1;
            foreach (var result in results)
            {
                if (result.SoLuongViPham > 0 && result.DanhSachViPham != null && result.DanhSachViPham.Any())
                {
                    foreach (var vp in result.DanhSachViPham)
                    {
                        var trangThai = vp.TrangThai ?? result.TrangThaiCSGT;

                        sheet.Cells[row, 1].Value = stt++;
                        sheet.Cells[row, 2].Value = result.BienSo;
                        sheet.Cells[row, 3].Value = trangThai;
                        sheet.Cells[row, 4].Value = vp.NgayViPham;
                        sheet.Cells[row, 5].Value = vp.HanhVi;
                        sheet.Cells[row, 6].Value = vp.DiaDiem;

                        sheet.Cells[row, 7].Value = $"Đơn vị phạt: {vp.DonViPhatHien}";
                        sheet.Cells[row, 7].Style.WrapText = true;

                        sheet.Cells[row, 8].Value = result.GhiChu;

                        sheet.Cells[row, 1, row, 8].Style.Fill.PatternType = ExcelFillStyle.Solid;
                        if (trangThai != null && trangThai.IndexOf("Đã xử phạt", StringComparison.OrdinalIgnoreCase) >= 0)
                        {
                            sheet.Cells[row, 1, row, 8].Style.Fill.BackgroundColor.SetColor(System.Drawing.Color.LightBlue);
                        }
                        else
                        {
                            sheet.Cells[row, 1, row, 8].Style.Fill.BackgroundColor.SetColor(System.Drawing.Color.LightPink);
                        }

                        row++;
                    }
                }
                else
                {
                    sheet.Cells[row, 1].Value = stt++;
                    sheet.Cells[row, 2].Value = result.BienSo;
                    sheet.Cells[row, 3].Value = result.TrangThaiCSGT;
                    sheet.Cells[row, 4].Value = result.NgayGioViPham?.ToString("dd/MM/yyyy HH:mm");
                    sheet.Cells[row, 5].Value = result.NoiDungViPham;
                    sheet.Cells[row, 6].Value = result.DiaDiemViPham;

                    sheet.Cells[row, 7].Value = result.FullViPhamData;
                    sheet.Cells[row, 7].Style.WrapText = true; // Cho phép text xuống dòng

                    sheet.Cells[row, 8].Value = result.GhiChu;

                    // Highlight violations
                    if (result.SoLuongViPham > 0)
                    {
                        sheet.Cells[row, 1, row, 8].Style.Fill.PatternType = ExcelFillStyle.Solid;
                        if (result.TrangThaiCSGT != null && result.TrangThaiCSGT.IndexOf("Đã xử phạt", StringComparison.OrdinalIgnoreCase) >= 0)
                        {
                            sheet.Cells[row, 1, row, 8].Style.Fill.BackgroundColor.SetColor(System.Drawing.Color.LightBlue);
                        }
                        else
                        {
                            sheet.Cells[row, 1, row, 8].Style.Fill.BackgroundColor.SetColor(System.Drawing.Color.LightPink);
                        }
                    }

                    row++;
                }
            }

            // Borders
            var dataRange = sheet.Cells[1, 1, row - 1, 8];
            dataRange.Style.Border.Top.Style = ExcelBorderStyle.Thin;
            dataRange.Style.Border.Left.Style = ExcelBorderStyle.Thin;
            dataRange.Style.Border.Right.Style = ExcelBorderStyle.Thin;
            dataRange.Style.Border.Bottom.Style = ExcelBorderStyle.Thin;

            // Auto-fit columns
            sheet.Cells[sheet.Dimension.Address].AutoFitColumns();
            sheet.Column(7).Width = 80; // Giới hạn độ rộng cột Chi tiết
        }

        private static void CreateViolationsSheet(ExcelWorksheet sheet, List<TrafficViolationBulkCheckVM> violations)
        {
            // Headers
            sheet.Cells["A1"].Value = "STT";
            sheet.Cells["B1"].Value = "Biển số";
            sheet.Cells["C1"].Value = "Trạng thái phạt nguội";
            sheet.Cells["D1"].Value = "Ngày giờ vi phạm";
            sheet.Cells["E1"].Value = "Địa điểm vi phạm";
            sheet.Cells["F1"].Value = "Chi tiết đầy đủ (Tất cả lỗi)";

            // Style headers
            using (var range = sheet.Cells["A1:F1"])
            {
                range.Style.Font.Bold = true;
                range.Style.Fill.PatternType = ExcelFillStyle.Solid;
                range.Style.Fill.BackgroundColor.SetColor(System.Drawing.Color.Red);
                range.Style.Font.Color.SetColor(System.Drawing.Color.White);
                range.Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;
            }

            // Data
            int row = 2;
            int stt = 1;
            foreach (var result in violations)
            {
                if (result.DanhSachViPham != null && result.DanhSachViPham.Any())
                {
                    foreach (var vp in result.DanhSachViPham)
                    {
                        sheet.Cells[row, 1].Value = stt++;
                        sheet.Cells[row, 2].Value = result.BienSo;
                        sheet.Cells[row, 3].Value = vp.TrangThai ?? result.TrangThaiCSGT;
                        sheet.Cells[row, 4].Value = vp.NgayViPham;
                        sheet.Cells[row, 5].Value = vp.DiaDiem;

                        sheet.Cells[row, 6].Value = $"Lỗi: {vp.HanhVi} - Đơn vị: {vp.DonViPhatHien}";
                        sheet.Cells[row, 6].Style.WrapText = true;

                        row++;
                    }
                }
                else
                {
                    sheet.Cells[row, 1].Value = stt++;
                    sheet.Cells[row, 2].Value = result.BienSo;
                    sheet.Cells[row, 3].Value = result.TrangThaiCSGT;
                    sheet.Cells[row, 4].Value = result.NgayGioViPham?.ToString("dd/MM/yyyy HH:mm");
                    sheet.Cells[row, 5].Value = result.DiaDiemViPham;

                    sheet.Cells[row, 6].Value = result.FullViPhamData;
                    sheet.Cells[row, 6].Style.WrapText = true;

                    row++;
                }
            }

            // Borders and auto-fit
            var dataRange = sheet.Cells[1, 1, row - 1, 6];
            dataRange.Style.Border.Top.Style = ExcelBorderStyle.Thin;
            dataRange.Style.Border.Left.Style = ExcelBorderStyle.Thin;
            dataRange.Style.Border.Right.Style = ExcelBorderStyle.Thin;
            dataRange.Style.Border.Bottom.Style = ExcelBorderStyle.Thin;
            sheet.Cells[sheet.Dimension.Address].AutoFitColumns();
            sheet.Column(6).Width = 80;
        }

        private static void CreateCleanSheet(ExcelWorksheet sheet, List<TrafficViolationBulkCheckVM> clean)
        {
            // Headers
            sheet.Cells["A1"].Value = "STT";
            sheet.Cells["B1"].Value = "Biển số";
            sheet.Cells["C1"].Value = "Trạng thái";

            // Style headers
            using (var range = sheet.Cells["A1:C1"])
            {
                range.Style.Font.Bold = true;
                range.Style.Fill.PatternType = ExcelFillStyle.Solid;
                range.Style.Fill.BackgroundColor.SetColor(System.Drawing.Color.LightGreen);
                range.Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;
            }

            // Data
            int row = 2;
            foreach (var result in clean)
            {
                sheet.Cells[row, 1].Value = row - 1;
                sheet.Cells[row, 2].Value = result.BienSo;
                sheet.Cells[row, 3].Value = "✓ Không vi phạm";
                sheet.Cells[row, 3].Style.Font.Color.SetColor(System.Drawing.Color.Green);
                row++;
            }

            // Borders and auto-fit
            var dataRange = sheet.Cells[1, 1, row - 1, 3];
            dataRange.Style.Border.Top.Style = ExcelBorderStyle.Thin;
            dataRange.Style.Border.Left.Style = ExcelBorderStyle.Thin;
            dataRange.Style.Border.Right.Style = ExcelBorderStyle.Thin;
            dataRange.Style.Border.Bottom.Style = ExcelBorderStyle.Thin;
            sheet.Cells[sheet.Dimension.Address].AutoFitColumns();
        }

        #endregion
    }
}
