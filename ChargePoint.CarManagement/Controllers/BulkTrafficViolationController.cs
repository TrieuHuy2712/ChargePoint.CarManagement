using ChargePoint.CarManagement.Data;
using ChargePoint.CarManagement.Models;
using ChargePoint.CarManagement.Models.Enums;
using ChargePoint.CarManagement.Models.ViewModels;
using ChargePoint.CarManagement.Models.ViewModels.TrafficViolationViewModels;
using ChargePoint.CarManagement.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using OfficeOpenXml;
using OfficeOpenXml.Style;

namespace ChargePoint.CarManagement.Controllers
{
    [Authorize]
    public class BulkTrafficViolationController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly ITrafficViolationService _violationService;

        public BulkTrafficViolationController(
            ApplicationDbContext context,
            ITrafficViolationService violationService)
        {
            _context = context;
            _violationService = violationService;
        }

        // GET: BulkTrafficViolation/Index
        public async Task<IActionResult> Index(string q, int page = 1, int pageSize = 50)
        {
            if (page < 1) page = 1;
            if (pageSize < 1) pageSize = 50;

            var carQuery = _context.Cars.AsQueryable();

            if (!string.IsNullOrWhiteSpace(q))
            {
                var key = q.Trim();
                var keyLower = key.ToLower();
                var keyNormalized = keyLower.Replace("-", "").Replace(".", "");

                carQuery = carQuery.Where(c =>
                    (c.BienSo != null && (c.BienSo.ToLower().Contains(keyLower) ||
                                          c.BienSo.Replace("-", "").Replace(".", "").ToLower().Contains(keyNormalized))) ||
                    (c.TenXe != null && c.TenXe.ToLower().Contains(keyLower)) ||
                    (c.TenKhachHang != null && c.TenKhachHang.ToLower().Contains(keyLower)) ||
                    (c.SoVIN != null && c.SoVIN.ToLower().Contains(keyLower)) ||
                    (c.MauXe != null && c.MauXe.ToLower().Contains(keyLower))
                );
            }

            var totalCount = await carQuery.CountAsync();
            var totalPages = (int)Math.Ceiling(totalCount / (double)pageSize);
            if (page > totalPages && totalPages > 0) page = totalPages;

            var cars = await carQuery
                .OrderBy(c => c.Stt)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            var viewModel = new PagedResult<Car>
            {
                Items = cars,
                PageNumber = page,
                PageSize = pageSize,
                TotalCount = totalCount,
                SearchQuery = q ?? string.Empty
            };

            return View(viewModel);
        }

        // POST: BulkTrafficViolation/CheckOnline
        [HttpPost]
        public async Task<IActionResult> CheckOnline([FromBody] int[] carIds)
        {
            if (carIds == null || carIds.Length == 0)
            {
                return Json(new { success = false, message = "Không có xe nào được chọn" });
            }

            var results = new List<object>();
            var cars = await _context.Cars.Where(c => carIds.Contains(c.Id)).ToListAsync();

            foreach (var car in cars)
            {
                if (string.IsNullOrEmpty(car.BienSo))
                {
                    results.Add(new
                    {
                        carId = car.Id,
                        bienSo = car.BienSo,
                        success = false,
                        message = "Không có biển số"
                    });
                    continue;
                }

                try
                {
                    var result = await _violationService.CheckViolationAsync(car.BienSo);
                    results.Add(new
                    {
                        carId = car.Id,
                        bienSo = car.BienSo,
                        tenXe = car.TenXe,
                        success = result.Success,
                        coViPham = result.CoViPham,
                        soLuongViPham = result.SoLuongViPham,
                        danhSachViPham = result.DanhSachViPham,
                        message = result.Message
                    });
                }
                catch (Exception ex)
                {
                    results.Add(new
                    {
                        carId = car.Id,
                        bienSo = car.BienSo,
                        success = false,
                        message = $"Lỗi: {ex.Message}"
                    });
                }

                // Delay để tránh quá tải API
                await Task.Delay(500);
            }

            return Json(new { success = true, results });
        }

        // POST: BulkTrafficViolation/Save
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Save([FromBody] List<TrafficViolationBulkCheckVM> results)
        {
            if (results == null || results.Count == 0)
            {
                return Json(new { success = false, message = "Không có dữ liệu để lưu" });
            }

            var savedCount = 0;
            var errors = new List<string>();

            foreach (var item in results)
            {
                try
                {
                    var coViPham = item.SoLuongViPham > 0;

                    var newRecord = new TrafficViolationCheck
                    {
                        CarId = item.CarId,
                        NgayKiemTra = DateTime.Now,
                        NguoiKiemTra = User.Identity?.Name,
                        CoViPham = coViPham,
                        SoLuongViPham = item.SoLuongViPham,
                        NgayGioViPham = item.NgayGioViPham,
                        NoiDungViPham = item.NoiDungViPham,
                        DiaDiemViPham = item.DiaDiemViPham,
                        TrangThaiXuLy = ViolationStatus.DaBao,
                        NgayCapNhatTrangThai = DateTime.Now,
                        GhiChu = item.GhiChu
                    };

                    _context.TrafficViolationChecks.Add(newRecord);
                    savedCount++;
                }
                catch (Exception ex)
                {
                    errors.Add($"Biển số {item.BienSo}: {ex.Message}");
                }
            }

            if (savedCount > 0)
            {
                await _context.SaveChangesAsync();
            }

            return Json(new
            {
                success = true,
                savedCount,
                totalCount = results.Count,
                errors,
                message = $"Đã lưu {savedCount}/{results.Count} kết quả kiểm tra"
            });
        }

        // POST: BulkTrafficViolation/ExportResults
        [HttpPost]
        public IActionResult ExportResults([FromBody] List<TrafficViolationBulkCheckVM> results)
        {
            if (results == null || results.Count == 0)
            {
                return BadRequest("Không có dữ liệu để xuất");
            }

            using (var package = new ExcelPackage())
            {
                // Create Summary worksheet
                var summarySheet = package.Workbook.Worksheets.Add("Tổng hợp");
                CreateSummarySheet(summarySheet, results);

                // Create All Results worksheet
                var allSheet = package.Workbook.Worksheets.Add("Tất cả kết quả");
                CreateAllResultsSheet(allSheet, results);

                // Create Violations worksheet
                var violations = results.Where(r => r.SoLuongViPham > 0 && string.IsNullOrEmpty(r.GhiChu?.Contains("Lỗi") == true ? r.GhiChu : null)).ToList();
                if (violations.Any())
                {
                    var violationsSheet = package.Workbook.Worksheets.Add("Có vi phạm");
                    CreateViolationsSheet(violationsSheet, violations);
                }

                // Create Clean worksheet
                var clean = results.Where(r => r.SoLuongViPham == 0 && string.IsNullOrEmpty(r.GhiChu?.Contains("Lỗi") == true ? r.GhiChu : null)).ToList();
                if (clean.Any())
                {
                    var cleanSheet = package.Workbook.Worksheets.Add("Không vi phạm");
                    CreateCleanSheet(cleanSheet, clean);
                }

                // Generate file
                var fileName = $"KetQuaTraCuu_{DateTime.Now:yyyyMMdd_HHmmss}.xlsx";
                var fileBytes = package.GetAsByteArray();

                return File(fileBytes, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", fileName);
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

            // Auto-fit columns
            sheet.Cells[sheet.Dimension.Address].AutoFitColumns();
        }

        private static void CreateAllResultsSheet(ExcelWorksheet sheet, List<TrafficViolationBulkCheckVM> results)
        {
            // Headers
            sheet.Cells["A1"].Value = "STT";
            sheet.Cells["B1"].Value = "Biển số";
            sheet.Cells["C1"].Value = "Số lượng vi phạm";
            sheet.Cells["D1"].Value = "Ngày giờ vi phạm";
            sheet.Cells["E1"].Value = "Nội dung vi phạm";
            sheet.Cells["F1"].Value = "Địa điểm vi phạm";
            sheet.Cells["G1"].Value = "Ghi chú";

            // Style headers
            using (var range = sheet.Cells["A1:G1"])
            {
                range.Style.Font.Bold = true;
                range.Style.Fill.PatternType = ExcelFillStyle.Solid;
                range.Style.Fill.BackgroundColor.SetColor(System.Drawing.Color.LightBlue);
                range.Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;
                range.Style.Border.BorderAround(ExcelBorderStyle.Thin);
            }

            // Data
            int row = 2;
            foreach (var result in results)
            {
                sheet.Cells[row, 1].Value = row - 1;
                sheet.Cells[row, 2].Value = result.BienSo;
                sheet.Cells[row, 3].Value = result.SoLuongViPham;
                sheet.Cells[row, 4].Value = result.NgayGioViPham?.ToString("dd/MM/yyyy HH:mm");
                sheet.Cells[row, 5].Value = result.NoiDungViPham;
                sheet.Cells[row, 6].Value = result.DiaDiemViPham;
                sheet.Cells[row, 7].Value = result.GhiChu;

                // Highlight violations
                if (result.SoLuongViPham > 0)
                {
                    sheet.Cells[row, 1, row, 7].Style.Fill.PatternType = ExcelFillStyle.Solid;
                    sheet.Cells[row, 1, row, 7].Style.Fill.BackgroundColor.SetColor(System.Drawing.Color.LightPink);
                }

                row++;
            }

            // Borders
            var dataRange = sheet.Cells[1, 1, row - 1, 7];
            dataRange.Style.Border.Top.Style = ExcelBorderStyle.Thin;
            dataRange.Style.Border.Left.Style = ExcelBorderStyle.Thin;
            dataRange.Style.Border.Right.Style = ExcelBorderStyle.Thin;
            dataRange.Style.Border.Bottom.Style = ExcelBorderStyle.Thin;

            // Auto-fit columns
            sheet.Cells[sheet.Dimension.Address].AutoFitColumns();
        }

        private static void CreateViolationsSheet(ExcelWorksheet sheet, List<TrafficViolationBulkCheckVM> violations)
        {
            // Headers
            sheet.Cells["A1"].Value = "STT";
            sheet.Cells["B1"].Value = "Biển số";
            sheet.Cells["C1"].Value = "Số vi phạm";
            sheet.Cells["D1"].Value = "Ngày giờ vi phạm";
            sheet.Cells["E1"].Value = "Nội dung vi phạm";
            sheet.Cells["F1"].Value = "Địa điểm vi phạm";

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
            foreach (var result in violations)
            {
                sheet.Cells[row, 1].Value = row - 1;
                sheet.Cells[row, 2].Value = result.BienSo;
                sheet.Cells[row, 3].Value = result.SoLuongViPham;
                sheet.Cells[row, 4].Value = result.NgayGioViPham?.ToString("dd/MM/yyyy HH:mm");
                sheet.Cells[row, 5].Value = result.NoiDungViPham;
                sheet.Cells[row, 6].Value = result.DiaDiemViPham;

                // Highlight high violation count
                if (result.SoLuongViPham > 2)
                {
                    sheet.Cells[row, 3].Style.Font.Color.SetColor(System.Drawing.Color.DarkRed);
                    sheet.Cells[row, 3].Style.Font.Bold = true;
                }

                row++;
            }

            // Borders and auto-fit
            var dataRange = sheet.Cells[1, 1, row - 1, 6];
            dataRange.Style.Border.Top.Style = ExcelBorderStyle.Thin;
            dataRange.Style.Border.Left.Style = ExcelBorderStyle.Thin;
            dataRange.Style.Border.Right.Style = ExcelBorderStyle.Thin;
            dataRange.Style.Border.Bottom.Style = ExcelBorderStyle.Thin;
            sheet.Cells[sheet.Dimension.Address].AutoFitColumns();
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
