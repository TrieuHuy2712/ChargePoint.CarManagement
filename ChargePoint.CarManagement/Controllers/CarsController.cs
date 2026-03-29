using ChargePoint.CarManagement.Data;
using ChargePoint.CarManagement.Models;
using ChargePoint.CarManagement.Models.ViewModels;
using ChargePoint.CarManagement.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace ChargePoint.CarManagement.Controllers
{
    [Authorize]
    public class CarsController(
        ApplicationDbContext context,
        IImageUploadService imageUploadService,
        ILogger<CarsController> logger) : Controller
    {
        private readonly ApplicationDbContext _context = context;
        private readonly IImageUploadService _imageUploadService = imageUploadService;
        private readonly ILogger<CarsController> _logger = logger;

        // GET: Cars
        public async Task<IActionResult> Index(string q, int page = 1, int pageSize = 10)
        {
            if (page < 1) page = 1;
            if (pageSize < 1) pageSize = 10;

            var query = _context.Cars.AsQueryable();

            if (!string.IsNullOrWhiteSpace(q))
            {
                var key = q.Trim();
                var keyLower = key.ToLower();

                query = query.Where(c =>
                    (c.BienSo != null && c.BienSo.ToLower().Contains(keyLower)) ||
                    (c.TenXe != null && c.TenXe.ToLower().Contains(keyLower)) ||
                    (c.TenKhachHang != null && c.TenKhachHang.ToLower().Contains(keyLower)) ||
                    (c.SoVIN != null && c.SoVIN.ToLower().Contains(keyLower)) ||
                    (c.MauXe != null && c.MauXe.ToLower().Contains(keyLower))
                );
            }

            var totalCount = await query.CountAsync();

            var totalPages = (int)Math.Ceiling(totalCount / (double)pageSize);
            if (page > totalPages && totalPages > 0) page = totalPages;

            var cars = await query
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

        // GET: Cars/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var car = await _context.Cars.FirstOrDefaultAsync(m => m.Id == id);
            if (car == null)
            {
                return NotFound();
            }

            return View(car);
        }

        // GET: Cars/Create
        public IActionResult Create()
        {
            var maxStt = _context.Cars.Any() ? _context.Cars.Max(c => c.Stt) : 0;
            var car = new Car { Stt = maxStt + 1 };
            return View(car);
        }

        // POST: Cars/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        [RequestSizeLimit(150 * 1024 * 1024)] // 150MB cho video
        public async Task<IActionResult> Create(
            [Bind("Id,Stt,TenXe,SoLuong,MauXe,SoVIN,BienSo,MauBienSo,TenKhachHang,ThongTinChoThue,OdoXe")] Car car,
            IFormFile? HinhAnhNhanBanGiao,
            IFormFile? HinhAnhBanGiaoKH,
            IFormFile? VideoNhanBanGiao,
            IFormFile? VideoBanGiaoKH)
        {
            if (ModelState.IsValid)
            {
                try
                {
                    var bienSo = car.BienSo ?? "NoPlate";

                    // Upload hình ảnh
                    if (HinhAnhNhanBanGiao != null && HinhAnhNhanBanGiao.Length > 0)
                    {
                        car.HinhAnhNhanBanGiao = await _imageUploadService.UploadFileAsync(
                            HinhAnhNhanBanGiao, bienSo, "NhanBanGiao_GSM");
                    }

                    if (HinhAnhBanGiaoKH != null && HinhAnhBanGiaoKH.Length > 0)
                    {
                        car.HinhAnhBanGiaoKH = await _imageUploadService.UploadFileAsync(
                            HinhAnhBanGiaoKH, bienSo, "BanGiao_KH");
                    }

                    // Upload video
                    if (VideoNhanBanGiao != null && VideoNhanBanGiao.Length > 0)
                    {
                        car.VideoNhanBanGiao = await _imageUploadService.UploadVideoAsync(
                            VideoNhanBanGiao, bienSo, "Video_NhanBanGiao");
                    }

                    if (VideoBanGiaoKH != null && VideoBanGiaoKH.Length > 0)
                    {
                        car.VideoBanGiaoKH = await _imageUploadService.UploadVideoAsync(
                            VideoBanGiaoKH, bienSo, "Video_BanGiaoKH");
                    }

                    car.NgayTao = DateTime.Now;
                    _context.Add(car);
                    await _context.SaveChangesAsync();

                    TempData["SuccessMessage"] = "Thêm xe mới thành công!";
                    return RedirectToAction(nameof(Index));
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error creating car");
                    ModelState.AddModelError("", $"Có lỗi xảy ra: {ex.Message}");
                }
            }
            return View(car);
        }

        // GET: Cars/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var car = await _context.Cars.FindAsync(id);
            if (car == null)
            {
                return NotFound();
            }
            return View(car);
        }

        // POST: Cars/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        [RequestSizeLimit(150 * 1024 * 1024)] // 150MB cho video
        public async Task<IActionResult> Edit(
            int id,
            [Bind("Id,Stt,TenXe,SoLuong,MauXe,SoVIN,BienSo,MauBienSo,TenKhachHang,ThongTinChoThue,OdoXe,HinhAnhNhanBanGiao,HinhAnhBanGiaoKH,VideoNhanBanGiao,VideoBanGiaoKH,NgayTao")] Car car,
            IFormFile? HinhAnhNhanBanGiaoFile,
            IFormFile? HinhAnhBanGiaoKHFile,
            IFormFile? VideoNhanBanGiaoFile,
            IFormFile? VideoBanGiaoKHFile)
        {
            if (id != car.Id)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    var bienSo = car.BienSo ?? "NoPlate";

                    // Upload hình ảnh mới
                    if (HinhAnhNhanBanGiaoFile != null && HinhAnhNhanBanGiaoFile.Length > 0)
                    {
                        if (!string.IsNullOrEmpty(car.HinhAnhNhanBanGiao))
                        {
                            await _imageUploadService.DeleteFileAsync(car.HinhAnhNhanBanGiao);
                        }
                        car.HinhAnhNhanBanGiao = await _imageUploadService.UploadFileAsync(
                            HinhAnhNhanBanGiaoFile, bienSo, "NhanBanGiao_GSM");
                    }

                    if (HinhAnhBanGiaoKHFile != null && HinhAnhBanGiaoKHFile.Length > 0)
                    {
                        if (!string.IsNullOrEmpty(car.HinhAnhBanGiaoKH))
                        {
                            await _imageUploadService.DeleteFileAsync(car.HinhAnhBanGiaoKH);
                        }
                        car.HinhAnhBanGiaoKH = await _imageUploadService.UploadFileAsync(
                            HinhAnhBanGiaoKHFile, bienSo, "BanGiao_KH");
                    }

                    // Upload video mới
                    if (VideoNhanBanGiaoFile != null && VideoNhanBanGiaoFile.Length > 0)
                    {
                        if (!string.IsNullOrEmpty(car.VideoNhanBanGiao))
                        {
                            await _imageUploadService.DeleteVideoAsync(car.VideoNhanBanGiao);
                        }
                        car.VideoNhanBanGiao = await _imageUploadService.UploadVideoAsync(
                            VideoNhanBanGiaoFile, bienSo, "Video_NhanBanGiao");
                    }

                    if (VideoBanGiaoKHFile != null && VideoBanGiaoKHFile.Length > 0)
                    {
                        if (!string.IsNullOrEmpty(car.VideoBanGiaoKH))
                        {
                            await _imageUploadService.DeleteVideoAsync(car.VideoBanGiaoKH);
                        }
                        car.VideoBanGiaoKH = await _imageUploadService.UploadVideoAsync(
                            VideoBanGiaoKHFile, bienSo, "Video_BanGiaoKH");
                    }

                    car.NgayCapNhat = DateTime.Now;
                    _context.Update(car);
                    await _context.SaveChangesAsync();

                    TempData["SuccessMessage"] = "Cập nhật thông tin xe thành công!";
                    return RedirectToAction(nameof(Index));
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!CarExists(car.Id))
                    {
                        return NotFound();
                    }
                    throw;
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error updating car");
                    ModelState.AddModelError("", $"Có lỗi xảy ra: {ex.Message}");
                }
            }
            return View(car);
        }

        // GET: Cars/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var car = await _context.Cars.FirstOrDefaultAsync(m => m.Id == id);
            if (car == null)
            {
                return NotFound();
            }

            return View(car);
        }

        // POST: Cars/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var car = await _context.Cars.FindAsync(id);
            if (car != null)
            {
                try
                {
                    // Xóa hình ảnh
                    if (!string.IsNullOrEmpty(car.HinhAnhNhanBanGiao))
                    {
                        await _imageUploadService.DeleteFileAsync(car.HinhAnhNhanBanGiao);
                    }
                    if (!string.IsNullOrEmpty(car.HinhAnhBanGiaoKH))
                    {
                        await _imageUploadService.DeleteFileAsync(car.HinhAnhBanGiaoKH);
                    }

                    // Xóa video
                    if (!string.IsNullOrEmpty(car.VideoNhanBanGiao))
                    {
                        await _imageUploadService.DeleteVideoAsync(car.VideoNhanBanGiao);
                    }
                    if (!string.IsNullOrEmpty(car.VideoBanGiaoKH))
                    {
                        await _imageUploadService.DeleteVideoAsync(car.VideoBanGiaoKH);
                    }

                    _context.Cars.Remove(car);
                    await _context.SaveChangesAsync();

                    TempData["SuccessMessage"] = "Xóa xe thành công!";
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error deleting car");
                    TempData["ErrorMessage"] = "Có lỗi xảy ra khi xóa xe.";
                }
            }

            return RedirectToAction(nameof(Index));
        }

        private bool CarExists(int id)
        {
            return _context.Cars.Any(e => e.Id == id);
        }
    }
}
