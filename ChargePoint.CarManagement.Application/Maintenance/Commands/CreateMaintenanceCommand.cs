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
    public class CreateMaintenanceCommand : IRequest<Result>
    {
        public Domain.Entities.Car Car { get; set; }
        public MaintenanceRecord Model { get; set; }
        public ButtonAction ButtonAction { get; set; }
        public List<IFormFile>? HinhAnhChungTuFiles { get; set; }

    }

    public class CreateMaintenanceHandler(
        IUnitOfWork unitOfWork,
        IAuthenService authenService,
        IImageUploadService imageUploadService,
        IFusionCache memoryCache,
        ILogger<CreateMaintenanceHandler> logger) : IRequestHandler<CreateMaintenanceCommand, Result>
    {
        private readonly IAuthenService _authenService = authenService;
        private readonly IImageUploadService _imageUploadService = imageUploadService;
        private readonly IFusionCache _memoryCache = memoryCache;
        private readonly ILogger<CreateMaintenanceHandler> _logger = logger;
        private readonly IUnitOfWork _unitOfWork = unitOfWork;
        public async ValueTask<Result> Handle(CreateMaintenanceCommand cmd, CancellationToken cancellationToken)
        {
            // Create a new entity instance to avoid inserting with a supplied Id
            var bienSo = cmd.Car.BienSo ?? "NoPlate";
            var stateCarChanged = false;
            var newRecord = new MaintenanceRecord
            {
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

            // Nếu chọn "Lưu & Tiếp tục", lưu tạm vào cache và chuyển qua trang tạo Tire
            if (cmd.ButtonAction == ButtonAction.Next)
            {
                var draftCache = new MaintenanceDraftCache
                {
                    MaintenanceRecord = newRecord,
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
                var cacheKey = CacheKey.GetMaintenanceCreateDraftCacheKey(cmd.Model.CarId, _authenService.GetCurrentUserName());
                _memoryCache.Set(cacheKey, draftCache, TimeSpan.FromMinutes(30));
                _logger.LogInformation("Saved maintenance draft to cache with key {CacheKey}", cacheKey);
                return Result.Ok();
            }

            // Upload hình ảnh chứng từ (if any)
            if (cmd.HinhAnhChungTuFiles != null && cmd.HinhAnhChungTuFiles.Count > 0)
            {
                var imageUrls = new List<string>();

                foreach (var file in cmd.HinhAnhChungTuFiles)
                {
                    if (file.Length > 0)
                    {
                        var url = await _imageUploadService.UploadFileAsync(
                            file, bienSo, $"BaoDuong_{newRecord.NgayBaoDuong:yyyyMMdd}");
                        imageUrls.Add(url);
                    }
                }

                newRecord.HinhAnhChungTu = JsonSerializer.Serialize(imageUrls);
            }

            // Kiểm tra setting
            var settingAutoOdo = await _unitOfWork.SystemSettings.FirstOrDefaultAsync(s => s.Key == SystemSettingKeys.AutoUpdateOdo_Maintenance, cancellationToken);

            if (settingAutoOdo != null && settingAutoOdo.Value == "true")
            {
                if (newRecord.SoKmBaoDuong > cmd.Car.OdoXe)
                {
                    cmd.Car.OdoXe = newRecord.SoKmBaoDuong;
                    cmd.Car.NgayCapNhat = DateTime.Now;
                    stateCarChanged = true;
                }
            }

            try
            {

                await _unitOfWork.MaintenanceRecords.AddAsync(newRecord, cancellationToken);
                if (stateCarChanged)
                {
                    _unitOfWork.Cars.Update(cmd.Car);
                }
                    
                await _unitOfWork.SaveChangesAsync(cancellationToken);
                _memoryCache.Remove(CacheKey.GetMaintenanceCreateDraftCacheKey(newRecord.CarId, _authenService.GetCurrentUserName()));
                return Result.Ok();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating maintenance record for CarId {CarId}", cmd.Model.CarId);
                return Result.Fail($"An error occurred while creating the maintenance record. {ex.Message}");

            }  
        }
    }
}
