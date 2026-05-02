using ChargePoint.CarManagement.Application.Interfaces;
using ChargePoint.CarManagement.Application.Interfaces.Common;
using ChargePoint.CarManagement.Domain.Entities;
using ChargePoint.CarManagement.Domain.Models;
using Mediator;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using OfficeOpenXml;

namespace ChargePoint.CarManagement.Application.Car.Commands
{
    public class BulkImportCarsCommand : IRequest<Result<BulkImportModel>>
    {
        public IFormFile File { get; set; }
    }

    public class BulkImportCarsCommandHandler(
        IUnitOfWork unitOfWork,
        ILogger<BulkImportCarsCommandHandler> logger) : IRequestHandler<BulkImportCarsCommand, Result<BulkImportModel>>
    {
        private readonly IUnitOfWork _unitOfWork = unitOfWork;
        private readonly ILogger<BulkImportCarsCommandHandler> _logger = logger;
        public async ValueTask<Result<BulkImportModel>> Handle(BulkImportCarsCommand command, CancellationToken cancellationToken)
        {
            try
            {
                ExcelPackage.License.SetNonCommercialPersonal("Your Name or Organization's Name");
                using var stream = new MemoryStream();
                await command.File.CopyToAsync(stream);
                using var package = new ExcelPackage(stream);

                var worksheet = package.Workbook.Worksheets.FirstOrDefault();
                if (worksheet == null)
                {
                    return Result<BulkImportModel>.Fail("No worksheet found in the Excel file.");
                }

                int rowCount = worksheet.Dimension?.Rows ?? 0;
                if (rowCount < 2)
                {
                    return Result<BulkImportModel>.Fail("The Excel file must contain at least one data row.");
                }
                int addedCount = 0;
                int updatedCount = 0;
                var skippedRows = new List<string>();
                var duplicateInFile = new Dictionary<string, List<int>>();

                var carQueryable =  await _unitOfWork.Cars.FindAsync(c => !string.IsNullOrEmpty(c.SoVIN), cancellationToken: cancellationToken);
                var existingVinSet = carQueryable.Select(c => c.SoVIN.ToLower()).ToList();
                var existingVinLookup = new HashSet<string>(existingVinSet);

                var rowData = new List<(int Row, string? RawVin, string? RawTenXe, string? RawBienSo, string? RawBienSoCu, string? RawMauXe, string? RawLoaiBien, string? RawKhachHang, string? RawOdo, string? RawNgayThue, string? RawNgayHetHan)>();
                var vinRowsMap = new Dictionary<string, List<int>>();

                for (int row = 2; row <= rowCount; row++)
                {
                    var rawVin = worksheet.Cells[row, 2].Value?.ToString()?.Trim();
                    var rawTenXe = worksheet.Cells[row, 3].Value?.ToString()?.Trim();
                    var rawBienSo = worksheet.Cells[row, 4].Value?.ToString()?.Trim();
                    var rawBienSoCu = worksheet.Cells[row, 5].Value?.ToString()?.Trim();
                    var rawMauXe = worksheet.Cells[row, 6].Value?.ToString()?.Trim();
                    var rawLoaiBien = worksheet.Cells[row, 7].Value?.ToString()?.Trim();
                    var rawKhachHang = worksheet.Cells[row, 8].Value?.ToString()?.Trim();
                    var rawOdo = worksheet.Cells[row, 9].Value?.ToString()?.Trim();
                    var rawNgayThue = worksheet.Cells[row, 10].Value?.ToString()?.Trim();
                    var rawNgayHetHan = worksheet.Cells[row, 11].Value?.ToString()?.Trim();

                    rowData.Add((row, rawVin, rawTenXe, rawBienSo, rawBienSoCu, rawMauXe, rawLoaiBien, rawKhachHang, rawOdo, rawNgayThue, rawNgayHetHan));

                    if (!string.IsNullOrWhiteSpace(rawVin) && rawVin.Length == 17)
                    {
                        var vinLower = rawVin.ToLower();
                        if (!vinRowsMap.ContainsKey(vinLower))
                        {
                            vinRowsMap[vinLower] = new List<int>();
                        }
                        vinRowsMap[vinLower].Add(row);
                    }
                }

                foreach (var item in vinRowsMap.Where(x => x.Value.Count > 1))
                {
                    duplicateInFile[item.Key] = item.Value;
                }

                var newCars = new List<Domain.Entities.Car>();
                int maxStt = await _unitOfWork.Cars.FindAsync(c => true, cancellationToken: cancellationToken)
                    .ContinueWith(t => t.Result.Count != 0 ? t.Result.Max(c => c.Stt) : 0, cancellationToken);

                foreach (var item in rowData)
                {
                    if (string.IsNullOrWhiteSpace(item.RawVin) || item.RawVin.Length != 17)
                    {
                        skippedRows.Add($"Dòng {item.Row} (Sai định dạng 17 ký tự VIN)");
                        continue;
                    }

                    var vinLower = item.RawVin.ToLower();

                    if (duplicateInFile.ContainsKey(vinLower))
                    {
                        continue;
                    }

                    if (existingVinLookup.Contains(vinLower))
                    {
                        // VIN already exists in system: skip saving silently (do not show in duplicate list/UI).
                        continue;
                    }

                    var loaiBienLower = item.RawLoaiBien?.ToLower() ?? "";
                    bool isTrang = loaiBienLower.Contains("trang");
                    bool isVang = loaiBienLower.Contains("vang");

                    if (!string.IsNullOrWhiteSpace(loaiBienLower) && !isTrang && !isVang)
                    {
                        skippedRows.Add($"Dòng {item.Row} (Loại biển chỉ nhận chứa từ 'Trắng' hoặc 'Vàng')");
                        continue;
                    }

                    MauBienSo mauBien = isVang ? MauBienSo.Vang : MauBienSo.Trang;

                    int odo = 0;
                    if (!string.IsNullOrWhiteSpace(item.RawOdo))
                    {
                        int.TryParse(item.RawOdo, out odo);
                    }

                    DateTime? ngayThue = null;
                    if (!string.IsNullOrWhiteSpace(item.RawNgayThue) && DateTime.TryParseExact(item.RawNgayThue, "dd/MM/yyyy", System.Globalization.CultureInfo.InvariantCulture, System.Globalization.DateTimeStyles.None, out var nt))
                    {
                        ngayThue = nt;
                    }

                    DateTime? ngayHetHan = null;
                    if (!string.IsNullOrWhiteSpace(item.RawNgayHetHan) && DateTime.TryParseExact(item.RawNgayHetHan, "dd/MM/yyyy", System.Globalization.CultureInfo.InvariantCulture, System.Globalization.DateTimeStyles.None, out var nh))
                    {
                        ngayHetHan = nh;
                    }

                    maxStt++;
                    var car = new Domain.Entities.Car
                    {
                        Stt = maxStt,
                        SoVIN = item.RawVin,
                        TenXe = item.RawTenXe,
                        BienSo = string.IsNullOrWhiteSpace(item.RawBienSo) ? null : item.RawBienSo,
                        BienSoCu = string.IsNullOrWhiteSpace(item.RawBienSoCu) ? null : item.RawBienSoCu,
                        MauXe = item.RawMauXe,
                        MauBienSo = mauBien,
                        TenKhachHang = string.IsNullOrWhiteSpace(item.RawKhachHang) ? null : item.RawKhachHang,
                        OdoXe = odo,
                        NgayThue = ngayThue,
                        NgayHetHan = ngayHetHan,
                        SoLuong = 1,
                    };

                    newCars.Add(car);
                    addedCount++;
                }

                if (newCars.Count > 0)
                {
                    await _unitOfWork.Cars.AddRangeAsync(newCars, cancellationToken);
                    await _unitOfWork.SaveChangesAsync(cancellationToken);
                }

                var duplicateVinRows = duplicateInFile
                    .Select(x => new { vin = x.Key.ToUpper(), rows = x.Value.OrderBy(r => r).ToList(), source = "File" })
                    .OrderBy(x => x.vin)
                    .ToList();

                foreach (var dup in duplicateVinRows)
                {
                    skippedRows.Add($"Dòng {string.Join(",", dup.rows)} (Trùng số VIN trong file: {dup.vin})");
                }

                var duplicateVins = duplicateVinRows
                    .Select(x => x.vin)
                    .Distinct()
                    .OrderBy(x => x)
                    .ToList();

                return Result<BulkImportModel>.Ok(new BulkImportModel
                {
                    AddedCount = addedCount,
                    UpdatedCount = updatedCount,
                    SkippedRows = skippedRows,
                    DuplicatedVins = duplicateVins,
                    DuplicatedVinRows = duplicateVinRows
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while bulk importing cars from Excel file.");
                return Result<BulkImportModel>.Fail($"Failed to import cars: {ex.Message}");
            }
        }
    }
}
