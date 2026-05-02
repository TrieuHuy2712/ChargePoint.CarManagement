using ChargePoint.CarManagement.Application.Interfaces;
using ChargePoint.CarManagement.Application.Interfaces.Common;
using ChargePoint.CarManagement.Application.Interfaces.TireService;
using ChargePoint.CarManagement.Application.Tire.Models;
using ChargePoint.CarManagement.Domain.Entities;
using ChargePoint.CarManagement.Domain.Enums;
using ChargePoint.CarManagement.Domain.Mapping;
using ChargePoint.CarManagement.Domain.Models;
using ChargePoint.CarManagement.Models;
using Mediator;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using System.Text.Json;
using ZiggyCreatures.Caching.Fusion;

namespace ChargePoint.CarManagement.Application.Tire.Commands
{
    public class CreateTireCommand : IRequest<Result>
    {
        public TireRecord Model { get; set; }
        public List<IFormFile>? HinhAnhChungTuFiles { get; set; }
        public List<IFormFile>? HinhAnhDOTFiles { get; set; }
        public List<ViTriLop>? SelectedViTriLops { get; set; }
        public Dictionary<ViTriLop, TirePositionDraft>? PositionDrafts { get; set; }
        public bool FromDraft { get; set; }
    }

    public class CreateTireHandler(
        IUnitOfWork unitOfWork,
        IImageUploadService imageUploadService,
        IFusionCache memoryCache,
        ILogger<CreateTireHandler> logger,
        IAuthenService authenService) : IRequestHandler<CreateTireCommand, Result>
    {
        private readonly IImageUploadService _imageUploadService = imageUploadService;
        private readonly IFusionCache _memoryCache = memoryCache;
        private readonly ILogger<CreateTireHandler> _logger = logger;
        private readonly IAuthenService _authenService = authenService;
        private readonly IUnitOfWork _unitOfWork = unitOfWork;

        public async ValueTask<Result> Handle(CreateTireCommand cmd, CancellationToken cancellationToken)
        {
            var car = cmd.Model.Car;
            var bienSo = car.BienSo ?? "NoPlate";

            var targetPositions = (cmd.SelectedViTriLops ?? [])
                    .Append(cmd.Model.ViTriLop)
                    .Distinct()
                    .ToList();

            Dictionary<ViTriLop, string>? chungTuImagesByPosition;
            Dictionary<ViTriLop, string>? dotImagesByPosition;


            var maintenanceDraftKey = CacheKey.GetMaintenanceDraftCacheKey(cmd.Model.CarId, authenService.GetCurrentUserName());

            try
            {
                if (cmd.FromDraft)
                {
                    MaintenanceRecord? maintenanceDraft = null;
                    List<CachedFileData> maintenanceDraftFiles = [];


                    // Get cache structure when redirected from Maintenance Create/Edit page, which contains both MaintenanceRecord and uploaded files
                    var maintenanceDraftCache = _memoryCache.TryGet<MaintenanceDraftCache>(maintenanceDraftKey);
                    if (maintenanceDraftCache.HasValue && maintenanceDraftCache.Value?.MaintenanceRecord != null)
                    {
                        maintenanceDraft = maintenanceDraftCache.Value.MaintenanceRecord;
                        maintenanceDraftFiles = maintenanceDraftCache.Value.HinhAnhChungTuFiles ?? [];
                    }
                    else
                    {
                        var legacyMaintenanceDraft = _memoryCache.TryGet<MaintenanceRecord>(maintenanceDraftKey);
                        if (legacyMaintenanceDraft.HasValue)
                        {
                            maintenanceDraft = legacyMaintenanceDraft.Value;
                        }
                    }

                    if (maintenanceDraft == null)
                    {
                        _memoryCache.Remove(maintenanceDraftKey);
                        return Result.Fail("Không tìm thấy thông tin bảo dưỡng tạm. Vui lòng tạo lại thông tin bảo dưỡng.");
                    }

                    var validationResults = new List<System.ComponentModel.DataAnnotations.ValidationResult>();
                    var validationContext = new System.ComponentModel.DataAnnotations.ValidationContext(maintenanceDraft);
                    if (!System.ComponentModel.DataAnnotations.Validator.TryValidateObject(maintenanceDraft, validationContext, validationResults, true))
                    {
                        _memoryCache.Remove(maintenanceDraftKey);
                        return Result.Fail("Dữ liệu bảo dưỡng tạm không hợp lệ. Vui lòng quay lại Maintenance để lưu lại.");
                    }

                    try
                    {
                        chungTuImagesByPosition = await UploadImagesByPositionAsync(
                        cmd.HinhAnhChungTuFiles, targetPositions, bienSo, "Lop", DateTime.Now);

                        dotImagesByPosition = await UploadImagesByPositionAsync(
                            cmd.HinhAnhDOTFiles, targetPositions, bienSo, "DOT", DateTime.Now);

                        // Upload hình ảnh chứng từ bảo dưỡng từ draft nếu có
                        if (maintenanceDraftFiles.Count > 0)
                        {
                            var maintenanceImageUrls = new List<string>();
                            foreach (var cachedFile in maintenanceDraftFiles)
                            {
                                var cachedFormFile = cachedFile.ToFormFile();
                                var maintenanceImageUrl = await _imageUploadService.UploadFileAsync(
                                    cachedFormFile, bienSo, $"BaoDuong_{maintenanceDraft.NgayBaoDuong:yyyyMMdd}");
                                maintenanceImageUrls.Add(maintenanceImageUrl);
                            }

                            maintenanceDraft.HinhAnhChungTu = JsonSerializer.Serialize(maintenanceImageUrls);
                        }

                        // Sử dụng transaction để đảm bảo tính toàn vẹn dữ liệu khi lưu cả 2 record
                        await AddTireTransactionAsync(
                            tireRecord: cmd.Model,
                            maintenance: maintenanceDraft,
                            targetPositions: targetPositions,
                            chungTuImagesByPosition: chungTuImagesByPosition,
                            dotImagesByPosition: dotImagesByPosition,
                            positionDrafts: cmd.PositionDrafts,
                            car: car,
                            cancellationToken: cancellationToken);
                        _memoryCache.Remove(maintenanceDraftKey);
                        return Result.Ok();
                    }
                    catch (Exception ex)
                    {
                        _logger.LogError(ex, "Error occurred while creating tire transaction.");
                        return Result.Fail($"Có lỗi xảy ra khi tạo giao dịch lốp. {ex.Message}");
                    }
                }

                chungTuImagesByPosition = await UploadImagesByPositionAsync(
                    cmd.HinhAnhChungTuFiles, targetPositions, bienSo, "Lop", cmd.Model.NgayThucHien);
                dotImagesByPosition = await UploadImagesByPositionAsync(
                    cmd.HinhAnhDOTFiles, targetPositions, bienSo, "DOT", cmd.Model.NgayThucHien);

                // Sử dụng transaction để đảm bảo tính toàn vẹn dữ liệu khi lưu cả 2 record trường hợp maintenance null
                await AddTireTransactionAsync(
                    tireRecord: cmd.Model,
                    maintenance: null,
                    targetPositions: targetPositions,
                    chungTuImagesByPosition: chungTuImagesByPosition,
                    dotImagesByPosition: dotImagesByPosition,
                    positionDrafts: cmd.PositionDrafts,
                    car: car,
                    cancellationToken: cancellationToken);
                return Result.Ok();

            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while creating tire.");
                return Result.Fail($"Có lỗi xảy ra khi tạo lốp. {ex.Message}");
            }
        }

        private async Task<Dictionary<ViTriLop, string>> UploadImagesByPositionAsync(
            List<IFormFile>? files,
            IEnumerable<ViTriLop> positions,
            string bienSo,
            string prefix,
            DateTime ngayThucHien)
        {
            var result = new Dictionary<ViTriLop, string>();
            if (files == null || files.Count == 0)
            {
                return result;
            }

            var validFiles = files.Where(f => f != null && f.Length > 0).ToList();
            if (validFiles.Count == 0)
            {
                return result;
            }

            foreach (var position in positions)
            {
                var urls = new List<string>();
                foreach (var file in validFiles)
                {
                    var url = await _imageUploadService.UploadFileAsync(
                        file, bienSo, $"{prefix}_{position}_{ngayThucHien:yyyyMMdd}");
                    urls.Add(url);
                }
                result[position] = JsonSerializer.Serialize(urls);
            }

            return result;
        }

        private async Task AddTireTransactionAsync(
            TireRecord tireRecord,
            MaintenanceRecord? maintenance,
            List<ViTriLop> targetPositions,
            Dictionary<ViTriLop, string> chungTuImagesByPosition,
            Dictionary<ViTriLop, string> dotImagesByPosition,
            Dictionary<ViTriLop, TirePositionDraft>? positionDrafts,
            Domain.Entities.Car car,
            CancellationToken cancellationToken = default)
        {
            await _unitOfWork.BeginTransactionAsync(cancellationToken);
            try
            {
                if (maintenance != null)
                {
                    await _unitOfWork.MaintenanceRecords.AddAsync(maintenance);
                }

                // Clone the tire record for each target position and set images if provided
                var tireRecords = targetPositions
                    .Select(position =>
                    {
                        var source = BuildRecordSourceForPosition(tireRecord, position, positionDrafts);
                        var record = TireMapping.CloneTireRecordForPosition(source, position);
                        if (chungTuImagesByPosition.TryGetValue(position, out var chungTuJson))
                        {
                            record.HinhAnhChungTu = chungTuJson;
                        }
                        if (dotImagesByPosition.TryGetValue(position, out var dotJson))
                        {
                            record.HinhAnhDOT = dotJson;
                        }
                        return record;
                    })
                    .ToList();
                await _unitOfWork.TireRecords.AddRangeAsync(tireRecords, cancellationToken);
                var settingAutoOdoMaintenance = await _unitOfWork.SystemSettings
                                                .FirstOrDefaultAsync(s => s.Key == SystemSettingKeys.AutoUpdateOdo_Maintenance, cancellationToken: cancellationToken);

                if (settingAutoOdoMaintenance != null
                    && settingAutoOdoMaintenance.Value == "true"
                    && maintenance != null
                    && maintenance.SoKmBaoDuong > car.OdoXe)
                {
                    car.OdoXe = maintenance.SoKmBaoDuong;
                }

                var settingAutoOdoTire = await _unitOfWork.SystemSettings
                    .FirstOrDefaultAsync(s => s.Key == SystemSettingKeys.AutoUpdateOdo_Tire, cancellationToken: cancellationToken);
                if (settingAutoOdoTire != null
                    && settingAutoOdoTire.Value == "true"
                    && tireRecord.OdoThayLop > car.OdoXe)
                {
                    car.OdoXe = tireRecord.OdoThayLop;
                }
                await _unitOfWork.SaveChangesAsync(cancellationToken);
                await _unitOfWork.CommitAsync(cancellationToken);
            }
            catch (Exception)
            {
                await _unitOfWork.RollbackAsync(cancellationToken);
                throw;
            }
        }

        private static TireRecord BuildRecordSourceForPosition(
            TireRecord fallback,
            ViTriLop position,
            Dictionary<ViTriLop, TirePositionDraft>? positionDrafts)
        {
            if (positionDrafts == null || !positionDrafts.TryGetValue(position, out var draft) || draft == null)
            {
                return fallback;
            }

            var clone = TireMapping.CloneTireRecordForPosition(fallback, position);
            clone.LoaiThaoTac = draft.LoaiThaoTac;
            clone.NgayThucHien = draft.NgayThucHien;
            clone.OdoThayLop = draft.OdoThayLop;
            clone.HangLop = draft.HangLop;
            clone.ModelLop = draft.ModelLop;
            clone.KichThuocLop = draft.KichThuocLop;
            clone.OdoThayTiepTheo = draft.OdoThayTiepTheo;
            clone.ChiPhi = draft.ChiPhi;
            clone.NoiThucHien = draft.NoiThucHien;
            clone.GhiChu = draft.GhiChu;
            return clone;
        }
    }
}
