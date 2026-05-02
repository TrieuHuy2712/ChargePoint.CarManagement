

using ChargePoint.CarManagement.Application.Interfaces;
using ChargePoint.CarManagement.Application.Interfaces.Common;
using ChargePoint.CarManagement.Application.Tire.Models;
using ChargePoint.CarManagement.Domain.Entities;
using ChargePoint.CarManagement.Domain.Mapping;
using ChargePoint.CarManagement.Domain.Models;
using ChargePoint.CarManagement.Models;
using Mediator;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using System.Text.Json;
using ZiggyCreatures.Caching.Fusion;

namespace ChargePoint.CarManagement.Application.Tire.Commands
{
    public class EditTireCommand : IRequest<Result>
    {
        public TireRecord Model { get; set; }
        public List<IFormFile>? HinhAnhChungTuFiles { get; set; }
        public List<IFormFile>? HinhAnhDOTFiles { get; set; }
        public List<ViTriLop>? SelectedViTriLops { get; set; }
        public Dictionary<ViTriLop, TirePositionDraft>? PositionDrafts { get; set; }
        public bool FromDraft { get; set; } = false;
    }

    public class EditTireHandler(
        IUnitOfWork unitOfWork,
        IImageUploadService imageUploadService, 
        IFusionCache memoryCache, 
        IAuthenService authenService,   
        ILogger<EditTireHandler> logger) : IRequestHandler<EditTireCommand, Result>
    {
        private readonly IUnitOfWork _unitOfWork = unitOfWork;
        private readonly IImageUploadService _imageUploadService = imageUploadService;
        private readonly IFusionCache _memoryCache = memoryCache;
        private readonly ILogger<EditTireHandler> _logger = logger;
        private readonly IAuthenService _authenService = authenService;
        public async ValueTask<Result> Handle(EditTireCommand cmd, CancellationToken cancellationToken)
        {
            try
            {
                var maintenanceDraftKey = CacheKey.GetMaintenanceDraftCacheKey(cmd.Model.CarId, _authenService.GetCurrentUserName());
                var bienSo = cmd.Model.Car.BienSo ?? "NoPlate";
                var targetPositions = (cmd.SelectedViTriLops ?? [])
                    .Append(cmd.Model.ViTriLop).Distinct().ToList();

                if (cmd.FromDraft)
                {
                    MaintenanceRecord? maintenanceDraft = null;
                    List<CachedFileData> maintenanceDraftFiles = [];
                    var maintenanceDraftCache = _memoryCache.TryGet<MaintenanceDraftCache>(maintenanceDraftKey);
                    if (maintenanceDraftCache.HasValue &&
                        maintenanceDraftCache.Value?.MaintenanceRecord != null)
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
                        return Result.Fail("Không tìm thấy dữ liệu bảo dưỡng tạm. Vui lòng tạo lại thông tin bảo dưỡng.");
                    }
                    var validationResults = new List<System.ComponentModel.DataAnnotations.ValidationResult>();
                    var validationContext = new System.ComponentModel.DataAnnotations.ValidationContext(maintenanceDraft);
                    if (!System.ComponentModel.DataAnnotations.Validator.TryValidateObject(maintenanceDraft, validationContext, validationResults, true))
                    {
                        _memoryCache.Remove(maintenanceDraftKey);
                        return Result.Fail("Dữ liệu bảo dưỡng tạm không hợp lệ. Vui lòng quay lại Maintenance để lưu lại.");
                    }
                    
                    var chungTuImagesByPosition = await UploadImagesByPositionAsync(
                        cmd.HinhAnhChungTuFiles, targetPositions, bienSo, "Lop", DateTime.Now);
                    var dotImagesByPosition = await UploadImagesByPositionAsync(
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

                    await EditTireTransactionAsync(
                        tireRecord: cmd.Model,
                         maintenance: maintenanceDraft, 
                         chungTuImagesByPosition: chungTuImagesByPosition, 
                         dotImagesByPosition: dotImagesByPosition, 
                         targetPositions: targetPositions,
                         positionDrafts: cmd.PositionDrafts,
                         car: cmd.Model.Car,
                         cancellationToken: cancellationToken);
                    return Result.Ok();
                }


                // Upload hình ảnh chứng từ mới
                if (cmd.HinhAnhChungTuFiles != null && cmd.HinhAnhChungTuFiles.Count > 0)
                    {
                        var existingImages = cmd.Model.DanhSachHinhAnh;
                        foreach (var file in cmd.HinhAnhChungTuFiles)
                        {
                            if (file.Length > 0)
                            {
                                var url = await _imageUploadService.UploadFileAsync(
                                    file, bienSo, $"Lop_{cmd.Model.ViTriLop}_{cmd.Model.NgayThucHien:yyyyMMdd}");
                                existingImages.Add(url);
                            }
                        }
                        cmd.Model.HinhAnhChungTu = JsonSerializer.Serialize(existingImages);
                    }

                    // Upload hình ảnh DOT mới
                    if (cmd.HinhAnhDOTFiles != null && cmd.HinhAnhDOTFiles.Count > 0)
                    {
                        var existingDOTImages = cmd.Model.DanhSachHinhAnhDOT;
                        foreach (var file in cmd.HinhAnhDOTFiles)
                        {
                            if (file.Length > 0)
                            {
                                var url = await _imageUploadService.UploadFileAsync(
                                    file, bienSo, $"DOT_{cmd.Model.ViTriLop}_{cmd.Model.NgayThucHien:yyyyMMdd}");
                                existingDOTImages.Add(url);
                            }
                        }
                        cmd.Model.HinhAnhDOT = JsonSerializer.Serialize(existingDOTImages);
                    }


                // Cập nhật thông tin (không update CarId, NgayTao, NguoiTao)
                await EditTireTransactionAsync(
                    tireRecord: cmd.Model,
                    maintenance: null,
                    chungTuImagesByPosition: cmd.SelectedViTriLops != null
                                                ? cmd.SelectedViTriLops.ToDictionary(
                                                    pos => pos,
                                                    pos => cmd.Model.HinhAnhChungTu) 
                                                : new(),
                    dotImagesByPosition: cmd.SelectedViTriLops != null
                                                ? cmd.SelectedViTriLops.ToDictionary(
                                                    pos => pos,
                                                    pos => cmd.Model.HinhAnhDOT) 
                                                : new(),
                     targetPositions: cmd.SelectedViTriLops,
                     positionDrafts: cmd.PositionDrafts,
                     car: cmd.Model.Car,
                     cancellationToken: cancellationToken);

                return Result.Ok();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Lỗi khi cập nhật hồ sơ lốp");
                return Result.Fail($"Có lỗi xảy ra: {ex.Message}");
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

        private async Task EditTireTransactionAsync(
            TireRecord tireRecord,
            MaintenanceRecord? maintenance,
            Dictionary<ViTriLop, string> chungTuImagesByPosition,
            Dictionary<ViTriLop, string> dotImagesByPosition,
            List<ViTriLop> targetPositions,
            Dictionary<ViTriLop, TirePositionDraft>? positionDrafts,
            Domain.Entities.Car car,
            CancellationToken cancellationToken = default)
        {
            await _unitOfWork.BeginTransactionAsync(cancellationToken);
            try
            {
                if (maintenance != null)
                {
                    if (maintenance.Id == 0)
                    {
                        await _unitOfWork.MaintenanceRecords.AddAsync(maintenance, cancellationToken);
                    }
                    else
                    {
                        _unitOfWork.MaintenanceRecords.Update(maintenance);
                    }
                }

                var tireRecords = targetPositions
                    .Select(position => {
                        var source = BuildRecordSourceForPosition(tireRecord, position, positionDrafts);
                        var record = TireMapping.CloneTireRecordForPosition(source, position);
                        if (chungTuImagesByPosition.TryGetValue(position, out var chungTuJson))
                            record.HinhAnhChungTu = chungTuJson;
                        if (dotImagesByPosition.TryGetValue(position, out var dotJson))
                            record.HinhAnhDOT = dotJson;
                        record.Id = 0; // Đảm bảo tạo mới
                        return record;
                    })
                    .ToList();

                // Update images if provided
                if (chungTuImagesByPosition.TryGetValue(tireRecord.ViTriLop, out var chungTuJson))
                {
                    tireRecord.HinhAnhChungTu = chungTuJson;
                }
                if (dotImagesByPosition.TryGetValue(tireRecord.ViTriLop, out var dotJson))
                {
                    tireRecord.HinhAnhDOT = dotJson;
                }
                _unitOfWork.TireRecords.Update(tireRecord);
                _unitOfWork.TireRecords.UpdateRange(tireRecords);
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
            catch
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
