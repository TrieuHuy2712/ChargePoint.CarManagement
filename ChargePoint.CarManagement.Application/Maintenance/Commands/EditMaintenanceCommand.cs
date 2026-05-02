using ChargePoint.CarManagement.Application.Interfaces;
using ChargePoint.CarManagement.Application.Interfaces.Common;
using ChargePoint.CarManagement.Domain.Entities;
using ChargePoint.CarManagement.Domain.Enums;
using ChargePoint.CarManagement.Domain.Models;
using ChargePoint.CarManagement.Models;
using Mediator;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using System.Text.Json;
using ZiggyCreatures.Caching.Fusion;

namespace ChargePoint.CarManagement.Application.Maintenance.Commands
{
    public class EditMaintenanceCommand : IRequest<Result>
    {
        public MaintenanceRecord Model { get; set; }
        public MaintenanceRecord? CurrentModel { get; set; } // Thêm thuộc tính để truyền dữ liệu hiện tại của bản ghi (nếu cần)
        public List<IFormFile>? HinhAnhChungTuFiles { get; set; }
        public ButtonAction Action { get; set; }
        public Domain.Entities.Car Car { get; set; }
    }

    public class EditMaintenanceHandler(
        IUnitOfWork unitOfWork,
        IAuthenService authenService,
        IImageUploadService imageUploadService,
        IFusionCache memoryCache,
        ILogger<EditMaintenanceHandler> logger) : IRequestHandler<EditMaintenanceCommand, Result>
    {
        private readonly IAuthenService _authenService = authenService;
        private readonly IImageUploadService _imageUploadService = imageUploadService;
        private readonly IUnitOfWork _unitOfWork = unitOfWork;
        private readonly IFusionCache _memoryCache = memoryCache;
        private readonly ILogger<EditMaintenanceHandler> _logger = logger;
        public async ValueTask<Result> Handle(EditMaintenanceCommand cmd, CancellationToken cancellationToken)
        {
            var bienSo = cmd.Model.Car?.BienSo ?? "NoPlate";
            var stateCarChange = false;
            // Nếu chọn "Lưu & Tiếp theo", lưu tạm vào cache và chuyển qua trang tạo Tire
            if (cmd.Action == ButtonAction.Next)
            {
                var editRecord = new MaintenanceRecord
                {
                    Id = cmd.Model.Id,
                    CarId = cmd.Model.CarId,
                    NgayBaoDuong = cmd.Model.NgayBaoDuong,
                    SoKmBaoDuong = cmd.Model.SoKmBaoDuong,
                    CapBaoDuong = cmd.Model.LoaiHoSo == DocumentType.SuaChua ? null : cmd.Model.CapBaoDuong,
                    SoKmBaoDuongTiepTheo = cmd.Model.SoKmBaoDuongTiepTheo,
                    NoiDungBaoDuong = cmd.Model.NoiDungBaoDuong,
                    ChiPhi = cmd.Model.ChiPhi,
                    NoiBaoDuong = cmd.Model.NoiBaoDuong,
                    GhiChu = cmd.Model.GhiChu,
                    LoaiHoSo = cmd.Model.LoaiHoSo
                };

                var draftCache = new MaintenanceDraftCache
                {
                    MaintenanceRecord = editRecord,
                    HinhAnhChungTuFiles = cmd.HinhAnhChungTuFiles?
                                        .Where(f => f != null && f.Length > 0)
                                        .Select(file =>
                                        {
                                            using var stream = new MemoryStream();
                                            file.CopyTo(stream);
                                            return new CachedFileData
                                            {
                                                FileName = file.FileName,
                                                ContentType = file.ContentType,
                                                Data = stream.ToArray()
                                            };
                                        })
                                        .ToList() ?? []
                };

                var cacheKey = CacheKey.GetMaintenanceEditDraftCacheKey(cmd.Model.CarId, _authenService.GetCurrentUserName());
                _memoryCache.Set(cacheKey, draftCache, TimeSpan.FromMinutes(30)); // Cache tồn tại 30 phút
                return Result.Ok();
            }


            try
            {
                // Upload new images (append)
                if (cmd.HinhAnhChungTuFiles != null && cmd.HinhAnhChungTuFiles.Count > 0)
                {
                    var existingImages = cmd.CurrentModel?.DanhSachHinhAnh ?? new List<string>();

                    foreach (var file in cmd.HinhAnhChungTuFiles)
                    {
                        if (file.Length > 0)
                        {
                            var url = await _imageUploadService.UploadFileAsync(
                                file, bienSo, $"BaoDuong_{cmd.Model.NgayBaoDuong:yyyyMMdd}");
                            existingImages.Add(url);
                        }
                    }

                    cmd.Model.HinhAnhChungTu = JsonSerializer.Serialize(existingImages);
                }

                // Kiem tra Setting
                var settingAutoOdo = await _unitOfWork.SystemSettings.FirstOrDefaultAsync(s => s.Key == SystemSettingKeys.AutoUpdateOdo_Maintenance, cancellationToken);
                if (settingAutoOdo != null && settingAutoOdo.Value == "true")
                {
                    if (cmd.Model.SoKmBaoDuong > cmd.Car.OdoXe)
                    {
                        cmd.Car.OdoXe = cmd.Model.SoKmBaoDuong;
                        stateCarChange = true;
                    }

                }

                // Update
                if (stateCarChange)
                {
                    _unitOfWork.Cars.Update(cmd.Car);
                }
                _unitOfWork.MaintenanceRecords.Update(cmd.Model);
                await _unitOfWork.SaveChangesAsync(cancellationToken);
                return Result.Ok();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error editing maintenance record for CarId {CarId}", cmd.Model.CarId);
                return Result.Fail("An error occurred while editing the maintenance record. Please try again.");
            }
        }
    }
}
