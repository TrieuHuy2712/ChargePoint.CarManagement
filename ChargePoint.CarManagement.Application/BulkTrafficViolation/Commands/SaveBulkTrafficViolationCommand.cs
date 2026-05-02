using ChargePoint.CarManagement.Application.Interfaces;
using ChargePoint.CarManagement.Application.Interfaces.Common;
using ChargePoint.CarManagement.Domain.Entities;
using ChargePoint.CarManagement.Domain.Enums;
using ChargePoint.CarManagement.Domain.Models;
using ChargePoint.CarManagement.Domain.Models.TrafficViolation;
using ChargePoint.CarManagement.Domain.ViewModels.TrafficViolationViewModels;
using Mediator;
using Microsoft.Extensions.Logging;

namespace ChargePoint.CarManagement.Application.BulkTrafficViolation.Commands
{
    public class SaveBulkTrafficViolationCommand : IRequest<Result<BulkTrafficViolationResult>>
    {
        public List<TrafficViolationBulkCheckVM> CheckVMs { get; set; }
    }

    public class SaveBulkTrafficViolationCommandHandler(
        IUnitOfWork unitOfWork,
        ILogger<SaveBulkTrafficViolationCommandHandler> logger) : IRequestHandler<SaveBulkTrafficViolationCommand, Result<BulkTrafficViolationResult>>
    {
        private readonly IUnitOfWork _unitOfWork = unitOfWork;
        private readonly ILogger<SaveBulkTrafficViolationCommandHandler> _logger = logger;
        public async ValueTask<Result<BulkTrafficViolationResult>> Handle(SaveBulkTrafficViolationCommand cmd, CancellationToken cancellationToken)
        {
            try
            {
                if (cmd.CheckVMs == null || !cmd.CheckVMs.Any())
                {
                    return Result<BulkTrafficViolationResult>.Fail("No traffic violation data provided.");
                }
                var validResults = cmd.CheckVMs.Where(r => !r.IsError).ToList();
                if (!validResults.Any())
                {
                    return Result<BulkTrafficViolationResult>.Fail("No valid traffic violation data to save.");
                }
                var savedCount = 0;
                var errors = new List<string>();
                List<TrafficViolationCheck> newRecords = [];
                foreach (var item in validResults)
                {
                    try
                    {
                        var newRecord = new TrafficViolationCheck
                        {
                            CarId = item.CarId,
                            NgayKiemTra = DateTime.Now,
                            CoViPham = item.SoLuongViPham > 0,
                            SoLuongViPham = item.SoLuongViPham,
                            TrangThaiXuLy = ViolationStatus.DaBao,
                            NgayCapNhatTrangThai = DateTime.Now,
                            GhiChu = item.GhiChu
                        };

                        if (item.SoLuongViPham > 0 && item.DanhSachViPham != null && item.DanhSachViPham.Any())
                        {
                            var noiDungArr = new List<string>();
                            var diaDiemArr = new List<string>();
                            DateTime? firstNgayGio = null;

                            for (int i = 0; i < item.DanhSachViPham.Count; i++)
                            {
                                var vp = item.DanhSachViPham[i];
                                var prefix = item.DanhSachViPham.Count > 1 ? $"[{i + 1}] " : "";

                                if (!string.IsNullOrWhiteSpace(vp.HanhVi)) noiDungArr.Add(prefix + vp.HanhVi);
                                if (!string.IsNullOrWhiteSpace(vp.DiaDiem)) diaDiemArr.Add(prefix + vp.DiaDiem);

                                if (firstNgayGio == null && !string.IsNullOrWhiteSpace(vp.NgayViPham))
                                {
                                    var dateStr = vp.NgayViPham.Replace("  ", " ").Trim();
                                    if (DateTime.TryParseExact(dateStr, "HH:mm, dd/MM/yyyy", null, System.Globalization.DateTimeStyles.None, out var d))
                                    {
                                        firstNgayGio = d;
                                    }
                                    else if (DateTime.TryParse(dateStr, out var defD))
                                    {
                                        firstNgayGio = defD;
                                    }
                                }
                            }

                            newRecord.NoiDungViPham = noiDungArr.Any() ? string.Join("\n", noiDungArr) : item.NoiDungViPham;
                            newRecord.DiaDiemViPham = diaDiemArr.Any() ? string.Join("\n", diaDiemArr) : item.DiaDiemViPham;
                            newRecord.NgayGioViPham = firstNgayGio ?? item.NgayGioViPham;

                            var detailGhiChu = $"Tra cứu tự động lúc {DateTime.Now:dd/MM/yyyy HH:mm:ss}\nPhát hiện {item.SoLuongViPham} vi phạm:\n";
                            for (int i = 0; i < item.DanhSachViPham.Count; i++)
                            {
                                var vp = item.DanhSachViPham[i];
                                detailGhiChu += $"{i + 1}. {vp.NgayViPham} - Lỗi: {vp.HanhVi} | Đơn vị: {vp.DonViPhatHien}\n";
                            }
                            newRecord.GhiChu = detailGhiChu;
                        }
                        else
                        {
                            newRecord.NgayGioViPham = item.NgayGioViPham;
                            newRecord.NoiDungViPham = item.NoiDungViPham;
                            newRecord.DiaDiemViPham = item.DiaDiemViPham;
                        }

                        newRecords.Add(newRecord);
                        savedCount++;
                    }
                    catch (Exception ex)
                    {
                        errors.Add($"Biển số {item.BienSo}: {ex.Message}");
                    }
                }

                if (savedCount > 0)
                {
                    await _unitOfWork.TrafficViolations.AddRangeAsync(newRecords, cancellationToken);
                    await _unitOfWork.SaveChangesAsync(cancellationToken);
                }

                return Result<BulkTrafficViolationResult>.Ok(new BulkTrafficViolationResult
                {
                    Success = true,
                    SavedCount = savedCount,
                    TotalCount = validResults.Count,
                    Errors = errors,
                    Message = $"Đã lưu {savedCount}/{validResults.Count} kết quả kiểm tra"
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error saving bulk traffic violations");
                return Result<BulkTrafficViolationResult>.Fail("An error occurred while saving traffic violations.");
            }
        }
    }
}
