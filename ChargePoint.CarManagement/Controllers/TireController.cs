using ChargePoint.CarManagement.Data;
using ChargePoint.CarManagement.Models;
using ChargePoint.CarManagement.Models.ViewModels.TireViewModels;
using ChargePoint.CarManagement.Models.ViewModels;
using ChargePoint.CarManagement.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Text.Json;

namespace ChargePoint.CarManagement.Controllers
{
    [Authorize]
    public class TireController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly IImageUploadService _imageUploadService;
        private readonly ILogger<TireController> _logger;

        public TireController(
            ApplicationDbContext context,
            IImageUploadService imageUploadService,
            ILogger<TireController> logger)
        {
            _context = context;
            _imageUploadService = imageUploadService;
            _logger = logger;
        }

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

                carQuery = carQuery.Where(c =>
                    (c.BienSo != null && c.BienSo.ToLower().Contains(keyLower)) ||
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
                .OrderByDescending(t => t.NgayThucHien)
                .ToListAsync();

            return View(new TireHistoryVM
            {
                Car = car,
                ViTriLop = viTri,
            });
        }

        // GET: Tire/Create/5 (CarId)
        public async Task<IActionResult> Create(int? id, ViTriLop? viTri = null)
        {
            if (id == null) return NotFound();


            var car = await _context.Cars.FindAsync(id);
            if (car == null) return NotFound();

            var model = new TireRecord
            {
                CarId = car.Id,
                NgayThucHien = DateTime.Now,
                OdoThayLop = car.OdoXe,
                NguoiTao = User.Identity?.Name,
                ViTriLop = viTri ?? ViTriLop.TruocTrai
            };

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
            TireRecord model,
            List<IFormFile>? HinhAnhChungTuFiles)
        {
            if (ModelState.IsValid)
            {
                try
                {
                    var car = await _context.Cars.FindAsync(model.CarId);
                    if (car == null)
                    {
                        ModelState.AddModelError("", "Không tìm thấy xe");
                        return View(model);
                    }

                    // Upload hình ảnh
                    if (HinhAnhChungTuFiles != null && HinhAnhChungTuFiles.Count > 0)
                    {
                        var imageUrls = new List<string>();
                        var bienSo = car.BienSo ?? "NoPlate";

                        foreach (var file in HinhAnhChungTuFiles)
                        {
                            if (file.Length > 0)
                            {
                                var url = await _imageUploadService.UploadFileAsync(
                                    file, bienSo, $"Lop_{model.ViTriLop}_{model.NgayThucHien:yyyyMMdd}");
                                imageUrls.Add(url);
                            }
                        }

                        model.HinhAnhChungTu = JsonSerializer.Serialize(imageUrls);
                    }

                    model.NgayTao = DateTime.Now;
                    model.NguoiTao = User.Identity?.Name;

                    // Cập nhật ODO xe nếu cần
                    if (model.OdoThayLop > car.OdoXe)
                    {
                        car.OdoXe = model.OdoThayLop;
                        car.NgayCapNhat = DateTime.Now;
                    }

                    _context.TireRecords.Add(model);
                    await _context.SaveChangesAsync();

                    TempData["SuccessMessage"] = $"Thêm hồ sơ lốp ({model.TenViTriLop}) thành công!";
                    return RedirectToAction(nameof(CarDetail), new { id = model.CarId });
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Lỗi khi tạo hồ sơ lốp");
                    ModelState.AddModelError("", $"Có lỗi xảy ra: {ex.Message}");
                }
            }

            var carForView = await _context.Cars.FindAsync(model.CarId);
            return View(new TireCreateVM
            {
                Car = carForView,
                TireRecord = model
            });
        }

        // GET: Tire/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null) return NotFound();

            var record = await _context.TireRecords
                .Include(t => t.Car)
                .FirstOrDefaultAsync(t => t.Id == id);

            if (record == null) return NotFound();

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
            TireRecord model,
            List<IFormFile>? HinhAnhChungTuFiles)
        {
            if (id != model.Id)
                return NotFound();

            if (ModelState.IsValid)
            {
                try
                {
                    var existingRecord = await _context.TireRecords.FindAsync(id);
                    if (existingRecord == null)
                        return NotFound();

                    var car = await _context.Cars.FindAsync(model.CarId);
                    if (car == null)
                    {
                        ModelState.AddModelError("", "Không tìm thấy xe");
                        return View(model);
                    }

                    // Cập nhật thông tin
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

                    // Upload hình ảnh mới
                    if (HinhAnhChungTuFiles != null && HinhAnhChungTuFiles.Count > 0)
                    {
                        var existingImages = existingRecord.DanhSachHinhAnh;
                        var bienSo = car.BienSo ?? "NoPlate";

                        foreach (var file in HinhAnhChungTuFiles)
                        {
                            if (file.Length > 0)
                            {
                                var url = await _imageUploadService.UploadFileAsync(
                                    file, bienSo, $"Lop_{model.ViTriLop}_{model.NgayThucHien:yyyyMMdd}");
                                existingImages.Add(url);
                            }
                        }

                        existingRecord.HinhAnhChungTu = JsonSerializer.Serialize(existingImages);
                    }

                    await _context.SaveChangesAsync();

                    TempData["SuccessMessage"] = "Cập nhật hồ sơ lốp thành công!";
                    return RedirectToAction(nameof(CarDetail), new { id = model.CarId });
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Lỗi khi cập nhật hồ sơ lốp");
                    ModelState.AddModelError("", $"Có lỗi xảy ra: {ex.Message}");
                }
            }

            var carForView = await _context.Cars.FindAsync(model.CarId);
            return View(new TireEditVM
            {
                Car = carForView,
                Record = model
            });
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

                // Xóa hình ảnh
                foreach (var imageUrl in record.DanhSachHinhAnh)
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
        public async Task<IActionResult> DeleteImage(int recordId, string imageUrl)
        {
            var record = await _context.TireRecords.FindAsync(recordId);
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
