using ChargePoint.CarManagement.Data;
using ChargePoint.CarManagement.Models;
using ChargePoint.CarManagement.Models.ViewModels.TireViewModels;
using ChargePoint.CarManagement.Models.ViewModels;
using ChargePoint.CarManagement.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Text.Json;
using Microsoft.Extensions.Caching.Memory;
using ChargePoint.CarManagement.Models.Enums;

namespace ChargePoint.CarManagement.Controllers
{
    [Authorize]
    public class TireController(
        ApplicationDbContext context,
        IImageUploadService imageUploadService,
        ILogger<TireController> logger,
        IMemoryCache memoryCache) : Controller
    {
        private readonly ApplicationDbContext _context = context;
        private readonly IImageUploadService _imageUploadService = imageUploadService;
        private readonly ILogger<TireController> _logger = logger;
        private readonly IMemoryCache _memoryCache= memoryCache;

        // GET: Tire
        public async Task<IActionResult> Index(string q, int page = 1, int pageSize = 10)
        {
            if (page < 1) page = 1;
            if (pageSize < 1) pageSize = 10;

            // Base query for cars
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

            var pagedCars = await carQuery
                .OrderBy(c => c.Stt)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            var carIds = pagedCars.Select(c => c.Id).ToList();

            // Load tire records only for paged cars
            var tireRecords = await _context.TireRecords
                .Where(t => carIds.Contains(t.CarId))
                .ToListAsync();

            // Compose view models
            var items = pagedCars.Select(car => new TireIndexViewModel
            {
                Car = car,
                TireRecords = tireRecords
                    .Where(t => t.CarId == car.Id)
                    .GroupBy(t => t.ViTriLop)
                    .Select(g => new TireRecordDetailIndexVM
                    {
                        ViTri = g.Key,
                        LastRecord = g.OrderByDescending(t => t.NgayThucHien).FirstOrDefault()
                    })
                    .ToList(),
                TotalRecords = tireRecords.Count(t => t.CarId == car.Id)
            }).ToList();

            var viewModel = new PagedResult<TireIndexViewModel>
            {
                Items = items,
                PageNumber = page,
                PageSize = pageSize,
                TotalCount = totalCount,
                SearchQuery = q ?? string.Empty
            };

            return View(viewModel);
        }

        // GET: Tire/CarDetail/5
        public async Task<IActionResult> CarDetail(int? id)
        {
            if (id == null)
                return NotFound();

            var car = await _context.Cars.FindAsync(id);
            if (car == null)
                return NotFound();

            // Lấy tất cả tire records của xe này
            var tireRecords = await _context.TireRecords
                .Where(t => t.CarId == id)
                .ToListAsync();

            // Xử lý trong memory
            var tiresByPosition = tireRecords
                .GroupBy(t => t.ViTriLop)
                .Select(g => new TireRecordDetailIndexVM
                {
                    ViTri = g.Key,
                    LastRecord = g.OrderByDescending(t => t.NgayThucHien).FirstOrDefault()
                })
                .ToList();

            return View(new TireCareDetailVM
            {
                Car = car,
                TireRecordsPosition = tiresByPosition
            });
        }

        // GET: Tire/History/5 (CarId)
        public async Task<IActionResult> History(int? id, ViTriLop? viTri = null)
        {
            if (id == null) return NotFound();


            var car = await _context.Cars.FindAsync(id);
            if (car == null) return NotFound();

            var query = _context.TireRecords.Where(t => t.CarId == id);

            if (viTri.HasValue) query = query.Where(t => t.ViTriLop == viTri.Value);

            var records = await query
                .OrderByDescending(t => t.NgayThucHien) // Sắp xếp giảm dần (mới nhất lên trên)
                .ToListAsync();

            return View(new TireHistoryVM
            {
                Car = car,
                ViTriLop = viTri,
                Records = records
            });
        }

        private string GetMaintenanceDraftCacheKey(int carId)
            => $"MaintenanceCreate_{carId}_{User.Identity?.Name}";

        private string GetTireDraftCacheKey(int carId)
            => $"TireCreate_{carId}_{User.Identity?.Name}";

        // GET: Tire/Create/5 (CarId)
        public async Task<IActionResult> Create(int? id, ViTriLop? viTri = null, bool fromDraft = false)
        {
            if (id == null) return NotFound();

            var car = await _context.Cars.FindAsync(id);
            if (car == null) return NotFound();

            TireRecord model;
            var tireDraftKey = GetTireDraftCacheKey(car.Id);
            if (_memoryCache.TryGetValue(tireDraftKey, out TireRecord? tireDraft) && tireDraft != null)
            {
                tireDraft.CarId = car.Id;
                model = tireDraft;
            }
            else
            {
                model = new TireRecord
                {
                    CarId = car.Id,
                    NgayThucHien = DateTime.Now,
                    OdoThayLop = car.OdoXe,
                    NguoiTao = User.Identity?.Name,
                    ViTriLop = viTri ?? ViTriLop.TruocTrai
                };
            }

            ViewBag.FromDraft = fromDraft;
            return View(new TireCreateVM
            {
                Car = car,
                TireRecord = model
            });
        }

        // POST: Tire/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        [RequestSizeLimit(50 * 1024 * 1024)]
        public async Task<IActionResult> Create(
            [Bind(Prefix = "TireRecord")] TireRecord model,
            List<IFormFile>? HinhAnhChungTuFiles,
            List<IFormFile>? HinhAnhDOTFiles,
            ButtonAction action = ButtonAction.Save,
            bool fromDraft = false)
        {
            var car = await _context.Cars.FindAsync(model.CarId);
            if (car == null) return NotFound();

            var vm = new TireCreateVM
            {
                Car = car,
                TireRecord = model
            };

            ViewBag.FromDraft = fromDraft;

            if (!ModelState.IsValid) return View(vm);

            var maintenanceDraftKey = GetMaintenanceDraftCacheKey(model.CarId);
            var tireDraftKey = GetTireDraftCacheKey(model.CarId);

            if (action == ButtonAction.SaveMaintenance)
            {
                _memoryCache.Set(tireDraftKey, model, TimeSpan.FromMinutes(30));
                return RedirectToAction("Create", "Maintenance", new { id = model.CarId });
            }

            if (fromDraft || action == ButtonAction.Complete)
            {
                if (!_memoryCache.TryGetValue(maintenanceDraftKey, out MaintenanceRecord? maintenanceDraft) || maintenanceDraft == null)
                {
                    ModelState.AddModelError("", "Khong tim thay du lieu bao duong tam. Vui long quay lai Maintenance de tao lai.");
                    return View(vm);
                }

                var validationResults = new List<System.ComponentModel.DataAnnotations.ValidationResult>();
                var validationContext = new System.ComponentModel.DataAnnotations.ValidationContext(maintenanceDraft);
                if (!System.ComponentModel.DataAnnotations.Validator.TryValidateObject(maintenanceDraft, validationContext, validationResults, true))
                {
                    ModelState.AddModelError("", "Du lieu bao duong tam khong hop le. Vui long quay lai Maintenance de kiem tra.");
                    return View(vm);
                }

                try
                {
                    var bienSo = car.BienSo ?? "NoPlate";

                    if (HinhAnhChungTuFiles != null && HinhAnhChungTuFiles.Count > 0)
                    {
                        var imageUrls = new List<string>();
                        foreach (var file in HinhAnhChungTuFiles.Where(f => f != null && f.Length > 0))
                        {
                            var url = await _imageUploadService.UploadFileAsync(
                                file, bienSo, $"Lop_{model.ViTriLop}_{model.NgayThucHien:yyyyMMdd}");
                            imageUrls.Add(url);
                        }
                        model.HinhAnhChungTu = JsonSerializer.Serialize(imageUrls);
                    }

                    if (HinhAnhDOTFiles != null && HinhAnhDOTFiles.Count > 0)
                    {
                        var dotImageUrls = new List<string>();
                        foreach (var file in HinhAnhDOTFiles.Where(f => f != null && f.Length > 0))
                        {
                            var url = await _imageUploadService.UploadFileAsync(
                                file, bienSo, $"DOT_{model.ViTriLop}_{model.NgayThucHien:yyyyMMdd}");
                            dotImageUrls.Add(url);
                        }
                        model.HinhAnhDOT = JsonSerializer.Serialize(dotImageUrls);
                    }

                    var newMaintenance = new MaintenanceRecord
                    {
                        CarId = maintenanceDraft.CarId,
                        NgayBaoDuong = maintenanceDraft.NgayBaoDuong,
                        SoKmBaoDuong = maintenanceDraft.SoKmBaoDuong,
                        CapBaoDuong = maintenanceDraft.LoaiHoSo == LoaiHoSo.SuaChua ? null : maintenanceDraft.CapBaoDuong,
                        SoKmBaoDuongTiepTheo = maintenanceDraft.SoKmBaoDuongTiepTheo,
                        NoiDungBaoDuong = maintenanceDraft.NoiDungBaoDuong,
                        ChiPhi = maintenanceDraft.ChiPhi,
                        NoiBaoDuong = maintenanceDraft.NoiBaoDuong,
                        GhiChu = maintenanceDraft.GhiChu,
                        LoaiHoSo = maintenanceDraft.LoaiHoSo,
                        NgayTao = DateTime.Now,
                        NguoiTao = User.Identity?.Name
                    };

                    model.NgayTao = DateTime.Now;
                    model.NguoiTao = User.Identity?.Name;

                    await using var tx = await _context.Database.BeginTransactionAsync();

                    _context.MaintenanceRecords.Add(newMaintenance);
                    _context.TireRecords.Add(model);

                    var settingAutoOdoMaintenance = await _context.SystemSettings
                        .FirstOrDefaultAsync(s => s.Key == SystemSettingKeys.AutoUpdateOdo_Maintenance);
                    if (settingAutoOdoMaintenance != null && settingAutoOdoMaintenance.Value == "true" && newMaintenance.SoKmBaoDuong > car.OdoXe)
                    {
                        car.OdoXe = newMaintenance.SoKmBaoDuong;
                        car.NgayCapNhat = DateTime.Now;
                    }

                    var settingAutoOdoTire = await _context.SystemSettings
                        .FirstOrDefaultAsync(s => s.Key == SystemSettingKeys.AutoUpdateOdo_Tire);
                    if (settingAutoOdoTire != null && settingAutoOdoTire.Value == "true" && model.OdoThayLop > car.OdoXe)
                    {
                        car.OdoXe = model.OdoThayLop;
                        car.NgayCapNhat = DateTime.Now;
                    }

                    await _context.SaveChangesAsync();
                    await tx.CommitAsync();

                    _memoryCache.Remove(maintenanceDraftKey);
                    _memoryCache.Remove(tireDraftKey);

                    TempData["SuccessMessage"] = "Hoan tat: da luu ho so bao duong va ho so lop thanh cong!";
                    return RedirectToAction(nameof(CarDetail), new { id = model.CarId });
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Loi khi hoan tat draft Bao duong + Lop");
                    ModelState.AddModelError("", $"Co loi xay ra khi hoan tat: {ex.Message}");
                    return View(vm);
                }
            }

            try
            {
                var bienSo = car.BienSo ?? "NoPlate";

                if (HinhAnhChungTuFiles != null && HinhAnhChungTuFiles.Count > 0)
                {
                    var imageUrls = new List<string>();
                    foreach (var file in HinhAnhChungTuFiles.Where(f => f != null && f.Length > 0))
                    {
                        var url = await _imageUploadService.UploadFileAsync(
                            file, bienSo, $"Lop_{model.ViTriLop}_{model.NgayThucHien:yyyyMMdd}");
                        imageUrls.Add(url);
                    }
                    model.HinhAnhChungTu = JsonSerializer.Serialize(imageUrls);
                }

                if (HinhAnhDOTFiles != null && HinhAnhDOTFiles.Count > 0)
                {
                    var dotImageUrls = new List<string>();
                    foreach (var file in HinhAnhDOTFiles.Where(f => f != null && f.Length > 0))
                    {
                        var url = await _imageUploadService.UploadFileAsync(
                            file, bienSo, $"DOT_{model.ViTriLop}_{model.NgayThucHien:yyyyMMdd}");
                        dotImageUrls.Add(url);
                    }
                    model.HinhAnhDOT = JsonSerializer.Serialize(dotImageUrls);
                }

                model.NgayTao = DateTime.Now;
                model.NguoiTao = User.Identity?.Name;

                var settingAutoOdo = await _context.SystemSettings
                    .FirstOrDefaultAsync(s => s.Key == SystemSettingKeys.AutoUpdateOdo_Tire);
                if (settingAutoOdo != null && settingAutoOdo.Value == "true" && model.OdoThayLop > car.OdoXe)
                {
                    car.OdoXe = model.OdoThayLop;
                    car.NgayCapNhat = DateTime.Now;
                }

                _context.TireRecords.Add(model);
                await _context.SaveChangesAsync();
                _memoryCache.Remove(tireDraftKey);

                TempData["SuccessMessage"] = $"Thêm hồ sơ lốp ({model.TenViTriLop}) thành công!";
                return RedirectToAction(nameof(CarDetail), new { id = model.CarId });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Lỗi khi tạo hồ sơ lốp");
                ModelState.AddModelError("", $"Co loi xay ra: {ex.Message}");
            }

            return View(vm);
        }
        // GET: Tire/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null) return NotFound();

            var record = await _context.TireRecords
                .Include(t => t.Car)
                .FirstOrDefaultAsync(t => t.Id == id);

            if (record == null || record.Car == null) return NotFound();

            return View(new TireEditVM
            {
                Car = record.Car,
                Record = record
            });
        }

        // POST: Tire/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        [RequestSizeLimit(50 * 1024 * 1024)]
        public async Task<IActionResult> Edit(
            int id,
            [Bind(Prefix = "Record")] TireRecord model,
            List<IFormFile>? HinhAnhChungTuFiles,
            List<IFormFile>? HinhAnhDOTFiles)
        {
            if (id != model.Id)
                return NotFound();

            // Load existing record first to get CarId
            var existingRecord = await _context.TireRecords.FindAsync(id);
            if (existingRecord == null)
                return NotFound();

            // Load car information
            var car = await _context.Cars.FindAsync(existingRecord.CarId);
            if (car == null)
            {
                return NotFound();
            }

            // Create view model for potential return
            var vm = new TireEditVM
            {
                Car = car,
                Record = model
            };

            if (!ModelState.IsValid)
            {
                return View(vm);
            }

            try
            {
                // Cập nhật thông tin (không update CarId, NgayTao, NguoiTao)
                existingRecord.ViTriLop = model.ViTriLop;
                existingRecord.LoaiThaoTac = model.LoaiThaoTac;
                existingRecord.NgayThucHien = model.NgayThucHien;
                existingRecord.OdoThayLop = model.OdoThayLop;
                existingRecord.HangLop = model.HangLop;
                existingRecord.ModelLop = model.ModelLop;
                existingRecord.KichThuocLop = model.KichThuocLop;
                existingRecord.OdoThayTiepTheo = model.OdoThayTiepTheo;
                existingRecord.ChiPhi = model.ChiPhi;
                existingRecord.NoiThucHien = model.NoiThucHien;
                existingRecord.GhiChu = model.GhiChu;
                existingRecord.NgayCapNhat = DateTime.Now;
                existingRecord.NguoiCapNhat = User.Identity?.Name;

                var bienSo = car.BienSo ?? "NoPlate";

                // Upload hình ảnh chứng từ mới
                if (HinhAnhChungTuFiles != null && HinhAnhChungTuFiles.Count > 0)
                {
                    var existingImages = existingRecord.DanhSachHinhAnh;

                    foreach (var file in HinhAnhChungTuFiles)
                    {
                        if (file.Length > 0)
                        {
                            var url = await _imageUploadService.UploadFileAsync(
                                file, bienSo, $"Lop_{existingRecord.ViTriLop}_{existingRecord.NgayThucHien:yyyyMMdd}");
                            existingImages.Add(url);
                        }
                    }

                    existingRecord.HinhAnhChungTu = JsonSerializer.Serialize(existingImages);
                }

                // Upload hình ảnh DOT mới
                if (HinhAnhDOTFiles != null && HinhAnhDOTFiles.Count > 0)
                {
                    var existingDOTImages = existingRecord.DanhSachHinhAnhDOT;

                    foreach (var file in HinhAnhDOTFiles)
                    {
                        if (file.Length > 0)
                        {
                            var url = await _imageUploadService.UploadFileAsync(
                                file, bienSo, $"DOT_{existingRecord.ViTriLop}_{existingRecord.NgayThucHien:yyyyMMdd}");
                            existingDOTImages.Add(url);
                        }
                    }

                    existingRecord.HinhAnhDOT = JsonSerializer.Serialize(existingDOTImages);
                }

                // Kiểm tra setting trước khi ghi đè ODO
                var settingAutoOdo = await _context.SystemSettings
                    .FirstOrDefaultAsync(s => s.Key == SystemSettingKeys.AutoUpdateOdo_Tire);

                if (settingAutoOdo != null && settingAutoOdo.Value == "true")
                {
                    if (existingRecord.OdoThayLop > car.OdoXe)
                    {
                        car.OdoXe = existingRecord.OdoThayLop;
                        car.NgayCapNhat = DateTime.Now;
                    }
                }

                await _context.SaveChangesAsync();

                TempData["SuccessMessage"] = "Cập nhật hồ sơ lốp thành công!";
                return RedirectToAction(nameof(CarDetail), new { id = existingRecord.CarId });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Lỗi khi cập nhật hồ sơ lốp");
                ModelState.AddModelError("", $"Có lỗi xảy ra: {ex.Message}");
            }

            // Return view with populated data on error
            return View(vm);
        }

        // GET: Tire/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
                return NotFound();

            var record = await _context.TireRecords
                .Include(t => t.Car)
                .FirstOrDefaultAsync(t => t.Id == id);

            if (record == null)
                return NotFound();

            return View(record);
        }

        // POST: Tire/Delete/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(int id)
        {
            var record = await _context.TireRecords.FindAsync(id);
            if (record != null)
            {
                var carId = record.CarId;

                // Xóa hình ảnh chứng từ
                foreach (var imageUrl in record.DanhSachHinhAnh)
                {
                    await _imageUploadService.DeleteFileAsync(imageUrl);
                }

                // Xóa hình ảnh DOT
                foreach (var imageUrl in record.DanhSachHinhAnhDOT)
                {
                    await _imageUploadService.DeleteFileAsync(imageUrl);
                }

                _context.TireRecords.Remove(record);
                await _context.SaveChangesAsync();

                TempData["SuccessMessage"] = "Đã xóa hồ sơ lốp!";
                return RedirectToAction(nameof(CarDetail), new { id = carId });
            }

            return RedirectToAction(nameof(Index));
        }

        // POST: Tire/DeleteImage
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteImage([FromBody] DeleteImageRequest request)
        {
            if (request == null || string.IsNullOrEmpty(request.ImageUrl))
                return Json(new { success = false, message = "Thông tin không hợp lệ" });

            var record = await _context.TireRecords.FindAsync(request.RecordId);
            if (record == null)
                return Json(new { success = false, message = "Không tìm thấy hồ sơ" });

            try
            {
                bool imageFound = false;

                // Xóa ảnh chứng từ hoặc ảnh DOT tùy theo imageType
                if (request.ImageType == "DOT")
                {
                    var dotImages = record.DanhSachHinhAnhDOT;
                    if (dotImages.Contains(request.ImageUrl))
                    {
                        await _imageUploadService.DeleteFileAsync(request.ImageUrl);
                        dotImages.Remove(request.ImageUrl);
                        record.HinhAnhDOT = JsonSerializer.Serialize(dotImages);
                        imageFound = true;
                    }
                }
                else
                {
                    var images = record.DanhSachHinhAnh;
                    if (images.Contains(request.ImageUrl))
                    {
                        await _imageUploadService.DeleteFileAsync(request.ImageUrl);
                        images.Remove(request.ImageUrl);
                        record.HinhAnhChungTu = JsonSerializer.Serialize(images);
                        imageFound = true;
                    }
                }

                if (imageFound)
                {
                    await _context.SaveChangesAsync();
                    return Json(new { success = true, message = "Đã xóa hình ảnh" });
                }

                return Json(new { success = false, message = "Không tìm thấy hình ảnh" });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Lỗi khi xóa hình ảnh");
                return Json(new { success = false, message = ex.Message });
            }
        }

        // Helper class for DeleteImage request
        public class DeleteImageRequest
        {
            public int RecordId { get; set; }
            public string ImageUrl { get; set; } = string.Empty;
            public string ImageType { get; set; } = "ChungTu"; // "ChungTu" or "DOT"
        }
    }
}
