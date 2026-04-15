using ChargePoint.CarManagement.Data;
using ChargePoint.CarManagement.Models;
using ChargePoint.CarManagement.Models.ViewModels;
using ChargePoint.CarManagement.Models.ViewModels.MaintenanceViewModels;
using ChargePoint.CarManagement.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using System.Text.Json;

namespace ChargePoint.CarManagement.Controllers
{
    [Authorize]
    public class MaintenanceController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly IImageUploadService _imageUploadService;
        private readonly ILogger<MaintenanceController> _logger;

        public MaintenanceController(
            ApplicationDbContext context,
            IImageUploadService imageUploadService,
            ILogger<MaintenanceController> logger)
        {
            _context = context;
            _imageUploadService = imageUploadService;
            _logger = logger;
        }

        // GET: Maintenance
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

            // load maintenance records only for current page cars
            var maintenanceRecords = await _context.MaintenanceRecords
                .Where(m => carIds.Contains(m.CarId))
                .ToListAsync();

            var items = pagedCars.Select(car => new MaintenanceIndexViewModel
            {
                Car = car,
                LastMaintenance = maintenanceRecords
                    .Where(m => m.CarId == car.Id)
                    .OrderByDescending(m => m.NgayBaoDuong)
                    .FirstOrDefault(),
                TotalMaintenances = maintenanceRecords.Count(m => m.CarId == car.Id)
            }).ToList();

            var viewModel = new PagedResult<MaintenanceIndexViewModel>
            {
                Items = items,
                PageNumber = page,
                PageSize = pageSize,
                TotalCount = totalCount,
                SearchQuery = q ?? string.Empty
            };

            return View(viewModel);
        }

        // GET: Maintenance/History/5
        public async Task<IActionResult> History(int? id)
        {
            if (id == null)
                return NotFound();

            var car = await _context.Cars.FindAsync(id);
            if (car == null)
                return NotFound();

            var records = await _context.MaintenanceRecords
                .Where(m => m.CarId == id)
                .OrderByDescending(m => m.NgayBaoDuong)
                .ToListAsync();

            return View(new MaintenanceHistoryViewModel
            {
                Car = car,
                MaintenanceRecords = records
            });
        }

        // GET: Maintenance/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
                return NotFound();

            var record = await _context.MaintenanceRecords
                .Include(m => m.Car)
                .FirstOrDefaultAsync(m => m.Id == id);

            if (record == null)
                return NotFound();

            return View(record);
        }

        // GET: Maintenance/Create/5 (CarId)
        public async Task<IActionResult> Create(int? id)
        {
            if (id == null)
            {
                // Nếu không có CarId, hiển thị dropdown chọn xe
                var cars = await _context.Cars.OrderBy(c => c.BienSo).ToListAsync();
                var selectListItems = new SelectList(cars,"Id", "BienSo");
                return View(new MaintenanceCreateVM
                {
                    SelectListCars = selectListItems,
                    Cars = cars,
                    MaintenanceRecord = new()
                });
            }

            var car = await _context.Cars.FindAsync(id);
            if (car == null) return NotFound();

            var model = new MaintenanceRecord
            {
                CarId = car.Id,
                NgayBaoDuong = DateTime.Now,
                SoKmBaoDuong = car.OdoXe,
                NguoiTao = User.Identity?.Name
            };

            return View(new MaintenanceCreateVM
            {
                MaintenanceRecord = model,
                Cars = [car],
            });
        }

        // POST: Maintenance/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        [RequestSizeLimit(50 * 1024 * 1024)] // 50MB
        public async Task<IActionResult> Create(
            MaintenanceCreateVM viewModel, // ✅ Đúng: khớp với cấu trúc form
            List<IFormFile>? HinhAnhChungTuFiles)
        {
            var model = viewModel.MaintenanceRecord; // Lấy model từ ViewModel
            // Ensure EF will generate Id - remove any incoming Id validation
            ModelState.Remove(nameof(MaintenanceRecord.Id));

            if (ModelState.IsValid)
            {
                try
                {
                    var car = await _context.Cars.FindAsync(model.CarId);
                    if (car == null)
                    {
                        ModelState.AddModelError("", "Không tìm thấy xe");
                        var errorVM = new MaintenanceCreateVM
                        {
                            MaintenanceRecord = model,
                            Cars = [],
                            SelectListCars = new SelectList(await _context.Cars.OrderBy(c => c.BienSo).ToListAsync(), "Id", "BienSo")
                        };
                        return View(errorVM);
                    }

                    // Create a new entity instance to avoid inserting with a supplied Id
                    var newRecord = new MaintenanceRecord
                    {
                        CarId = model.CarId,
                        NgayBaoDuong = model.NgayBaoDuong,
                        SoKmBaoDuong = model.SoKmBaoDuong,
                        CapBaoDuong = model.CapBaoDuong,
                        SoKmBaoDuongTiepTheo = model.SoKmBaoDuongTiepTheo,
                        NoiDungBaoDuong = model.NoiDungBaoDuong,
                        ChiPhi = model.ChiPhi,
                        NoiBaoDuong = model.NoiBaoDuong,
                        GhiChu = model.GhiChu,
                        NgayTao = DateTime.Now,
                        NguoiTao = User.Identity?.Name
                    };

                    // Upload hình ảnh chứng từ (if any)
                    if (HinhAnhChungTuFiles != null && HinhAnhChungTuFiles.Count > 0)
                    {
                        var imageUrls = new List<string>();
                        var bienSo = car.BienSo ?? "NoPlate";

                        foreach (var file in HinhAnhChungTuFiles)
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
                    var settingAutoOdo = await _context.SystemSettings
                        .FirstOrDefaultAsync(s => s.Key == SystemSettingKeys.AutoUpdateOdo_Maintenance);

                    if (settingAutoOdo != null && settingAutoOdo.Value == "true")
                    {
                        if (newRecord.SoKmBaoDuong > car.OdoXe)
                        {
                            car.OdoXe = newRecord.SoKmBaoDuong;
                            car.NgayCapNhat = DateTime.Now;
                        }
                    }

                    _context.MaintenanceRecords.Add(newRecord);
                    await _context.SaveChangesAsync();

                    TempData["SuccessMessage"] = "Thêm hồ sơ bảo dưỡng thành công!";
                    return RedirectToAction(nameof(History), new { id = newRecord.CarId });
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Lỗi khi tạo hồ sơ bảo dưỡng");
                    ModelState.AddModelError("", $"Có lỗi xảy ra: {ex.Message}");
                }
            }

            var createVM = new MaintenanceCreateVM
            {
                MaintenanceRecord = model,
                Cars = [await _context.Cars.FindAsync(model.CarId)],
                SelectListCars = new SelectList(await _context.Cars.OrderBy(c => c.BienSo).ToListAsync(), "Id", "BienSo")
            };
            return View(createVM);
        }

        // GET: Maintenance/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
                return NotFound();

            var record = await _context.MaintenanceRecords
                .Include(m => m.Car)
                .FirstOrDefaultAsync(m => m.Id == id);

            if (record == null)
                return NotFound();

            return View(new MaintenanceEditVM
            {
                Car = record.Car!,
                Record = record
            });
        }

        // POST: Maintenance/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        [RequestSizeLimit(50 * 1024 * 1024)]
        public async Task<IActionResult> Edit(
            int id,
            [Bind(Prefix = "maintenance")] MaintenanceRecord model,
            List<IFormFile>? HinhAnhChungTuFiles)
        {
            if (id != model.Id)
                return NotFound();

            if (!ModelState.IsValid)
            {
                // Log validation errors
                foreach (var modelState in ModelState.Values)
                {
                    foreach (var error in modelState.Errors)
                    {
                        _logger.LogWarning("ModelState Error: {Error}", error.ErrorMessage);
                    }
                }
            }

            if (ModelState.IsValid)
            {
                try
                {
                    var existingRecord = await _context.MaintenanceRecords.FindAsync(id);
                    if (existingRecord == null)
                        return NotFound();

                    var car = await _context.Cars.FindAsync(model.CarId);
                    if (car == null)
                    {
                        ModelState.AddModelError("", "Không tìm thấy xe");
                        var errorVM = new MaintenanceEditVM
                        {
                            Car = null!,
                            Record = model
                        };
                        return View(errorVM);
                    }

                    // Update
                    existingRecord.NgayBaoDuong = model.NgayBaoDuong;
                    existingRecord.SoKmBaoDuong = model.SoKmBaoDuong;
                    existingRecord.CapBaoDuong = model.CapBaoDuong;
                    existingRecord.SoKmBaoDuongTiepTheo = model.SoKmBaoDuongTiepTheo;
                    existingRecord.NoiDungBaoDuong = model.NoiDungBaoDuong;
                    existingRecord.ChiPhi = model.ChiPhi;
                    existingRecord.NoiBaoDuong = model.NoiBaoDuong;
                    existingRecord.GhiChu = model.GhiChu;
                    existingRecord.NgayCapNhat = DateTime.Now;
                    existingRecord.NguoiCapNhat = User.Identity?.Name;

                    // Upload new images (append)
                    if (HinhAnhChungTuFiles != null && HinhAnhChungTuFiles.Count > 0)
                    {
                        var existingImages = existingRecord.DanhSachHinhAnh;
                        var bienSo = car.BienSo ?? "NoPlate";

                        foreach (var file in HinhAnhChungTuFiles)
                        {
                            if (file.Length > 0)
                            {
                                var url = await _imageUploadService.UploadFileAsync(
                                    file, bienSo, $"BaoDuong_{model.NgayBaoDuong:yyyyMMdd}");
                                existingImages.Add(url);
                            }
                        }

                        existingRecord.HinhAnhChungTu = JsonSerializer.Serialize(existingImages);
                    }

                    // Kiểm tra setting
                    var settingAutoOdo = await _context.SystemSettings
                        .FirstOrDefaultAsync(s => s.Key == SystemSettingKeys.AutoUpdateOdo_Maintenance);

                    if (settingAutoOdo != null && settingAutoOdo.Value == "true")
                    {
                        if (existingRecord.SoKmBaoDuong > car.OdoXe)
                        {
                            car.OdoXe = existingRecord.SoKmBaoDuong;
                            car.NgayCapNhat = DateTime.Now;
                        }
                    }

                    await _context.SaveChangesAsync();

                    TempData["SuccessMessage"] = "Cập nhật hồ sơ bảo dưỡng thành công!";
                    return RedirectToAction(nameof(History), new { id = model.CarId });
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Lỗi khi cập nhật hồ sơ bảo dưỡng");
                    ModelState.AddModelError("", $"Có lỗi xảy ra: {ex.Message}");
                }
            }

            // Reload car for view in case of validation error
            var carForView = await _context.Cars.FindAsync(model.CarId);
            if (carForView == null)
            {
                ModelState.AddModelError("", "Không tìm thấy thông tin xe");
                return NotFound();
            }

            var editVM = new MaintenanceEditVM
            {
                Car = carForView,
                Record = model
            };
            return View(editVM);
        }

        // POST: Maintenance/Delete/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(int id)
        {
            var record = await _context.MaintenanceRecords.FindAsync(id);
            if (record != null)
            {
                var carId = record.CarId;

                // Delete images
                foreach (var imageUrl in record.DanhSachHinhAnh)
                {
                    await _imageUploadService.DeleteFileAsync(imageUrl);
                }

                _context.MaintenanceRecords.Remove(record);
                await _context.SaveChangesAsync();

                TempData["SuccessMessage"] = "Đã xóa hồ sơ bảo dưỡng!";
                return RedirectToAction(nameof(History), new { id = carId });
            }

            return RedirectToAction(nameof(Index));
        }

        // POST: Maintenance/DeleteImage
        [HttpPost]
        public async Task<IActionResult> DeleteImage(int recordId, string imageUrl)
        {
            var record = await _context.MaintenanceRecords.FindAsync(recordId);
            if (record == null)
                return Json(new { success = false, message = "Không tìm thấy hồ sơ" });

            try
            {
                var images = record.DanhSachHinhAnh;
                if (images.Contains(imageUrl))
                {
                    await _imageUploadService.DeleteFileAsync(imageUrl);
                    images.Remove(imageUrl);
                    record.HinhAnhChungTu = JsonSerializer.Serialize(images);
                    await _context.SaveChangesAsync();

                    return Json(new { success = true, message = "Đã xóa hình ảnh" });
                }

                return Json(new { success = false, message = "Không tìm thấy hình ảnh" });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = ex.Message });
            }
        }
    }
}
