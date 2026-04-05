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
                var keyNormalized = keyLower.Replace("-", "").Replace(".", "");

                query = query.Where(c =>
                    (c.BienSo != null && (c.BienSo.ToLower().Contains(keyLower) || 
                                          c.BienSo.Replace("-", "").Replace(".", "").ToLower().Contains(keyNormalized))) ||
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

            var car = await _context.Cars
                .Include(c => c.Media)
                .FirstOrDefaultAsync(m => m.Id == id);
            if (car == null)
            {
                return NotFound();
            }

            var vm = CarViewModel.FromCar(car);
            return View(vm);
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
        public async Task<IActionResult> Create(
            [Bind("Id,Stt,TenXe,SoLuong,MauXe,SoVIN,BienSo,MauBienSo,TenKhachHang,ThongTinChoThue,OdoXe")] Car car,
            IFormFile? PrimaryImageFile,
            Dictionary<string, List<IFormFile>>? ImageFiles)
        {
            if (ModelState.IsValid)
            {
                try
                {
                    var bienSo = car.BienSo ?? "NoPlate";

                    // Prepare media list
                    var mediaList = new List<CarMedia>();

                    // 1) PRIMARY IMAGE: if provided, upload and mark as primary.
                    if (PrimaryImageFile != null && PrimaryImageFile.Length > 0)
                    {
                        var primaryUrl = await _imageUploadService.UploadFileAsync(PrimaryImageFile, bienSo, "Primary");
                        var primaryMedia = new CarMedia
                        {
                            Type = MediaType.Image_Primary,
                            Url = primaryUrl,
                            FileName = PrimaryImageFile.FileName,
                            IsPrimary = true
                        };
                        mediaList.Add(primaryMedia);
                        car.PrimaryImageUrl = primaryUrl;
                    }

                    // 2) Upload categorized images
                    if (ImageFiles != null && ImageFiles.Any())
                    {
                        foreach (var kvp in ImageFiles)
                        {
                            if (Enum.TryParse<MediaType>(kvp.Key, out var mediaType))
                            {
                                var typeDisplayName = mediaType.GetDisplayName();
                                foreach (var file in kvp.Value)
                                {
                                    if (file != null && file.Length > 0)
                                    {
                                        var url = await _imageUploadService.UploadFileAsync(file, bienSo, typeDisplayName);
                                        mediaList.Add(new CarMedia
                                        {
                                            Type = mediaType,
                                            Url = url,
                                            FileName = file.FileName
                                        });
                                    }
                                }
                            }
                        }
                    }

                    car.Media = mediaList;
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

            var car = await _context.Cars
                .Include(c => c.Media)
                .FirstOrDefaultAsync(c => c.Id == id);

            if (car == null)
            {
                return NotFound();
            }

            var vm = CarViewModel.FromCar(car);
            return View(vm);
        }

        // POST: Cars/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(
            int id,
            CarViewModel vm,
            IFormFile? PrimaryImageFile,
            Dictionary<string, List<IFormFile>>? ImageFiles)
        {
            if (id != vm.Id)
            {
                return NotFound();
            }

            if (!ModelState.IsValid)
            {
                // reload existing media and preserve user-entered fields for the form
                var existingForView = await _context.Cars.Include(c => c.Media).FirstOrDefaultAsync(c => c.Id == id);
                if (existingForView != null)
                {
                    var populated = CarViewModel.FromCar(existingForView);
                    // preserve submitted scalar values so user doesn't lose edits
                    populated.Stt = vm.Stt;
                    populated.TenXe = vm.TenXe;
                    populated.SoLuong = vm.SoLuong;
                    populated.MauXe = vm.MauXe;
                    populated.SoVIN = vm.SoVIN;
                    populated.BienSo = vm.BienSo;
                    populated.MauBienSo = vm.MauBienSo;
                    populated.TenKhachHang = vm.TenKhachHang;
                    populated.ThongTinChoThue = vm.ThongTinChoThue;
                    populated.OdoXe = vm.OdoXe;
                    populated.PrimaryImageUrl = vm.PrimaryImageUrl;
                    return View(populated);
                }

                return View(vm);
            }

            try
            {
                var bienSo = vm.BienSo ?? "NoPlate";

                var existing = await _context.Cars
                    .Include(c => c.Media)
                    .FirstOrDefaultAsync(c => c.Id == id);

                if (existing == null) return NotFound();

                // map scalar fields from vm to entity
                existing.Stt = vm.Stt;
                existing.TenXe = vm.TenXe;
                existing.SoLuong = vm.SoLuong;
                existing.MauXe = vm.MauXe;
                existing.SoVIN = vm.SoVIN;
                existing.BienSo = vm.BienSo;
                existing.MauBienSo = vm.MauBienSo;
                existing.TenKhachHang = vm.TenKhachHang;
                existing.ThongTinChoThue = vm.ThongTinChoThue;
                existing.OdoXe = vm.OdoXe;

                existing.Media ??= new List<CarMedia>();

                // If a new primary image file was provided, upload it and make it primary.
                if (PrimaryImageFile != null && PrimaryImageFile.Length > 0)
                {
                    var primaryUrl = await _imageUploadService.UploadFileAsync(PrimaryImageFile, bienSo, "Primary");
                    var primaryMedia = new CarMedia
                    {
                        CarId = existing.Id,
                        Type = MediaType.Image_Primary,
                        Url = primaryUrl,
                        FileName = PrimaryImageFile.FileName,
                        IsPrimary = true
                    };

                    // clear any existing primary flags
                    foreach (var m in existing.Media)
                    {
                        m.IsPrimary = false;
                    }

                    existing.Media.Add(primaryMedia);
                    existing.PrimaryImageUrl = primaryUrl;
                }

                // Handle new categorized image uploads (append)
                if (ImageFiles != null && ImageFiles.Any())
                {
                    foreach (var kvp in ImageFiles)
                    {
                        if (Enum.TryParse<MediaType>(kvp.Key, out var mediaType))
                        {
                            var typeDisplayName = mediaType.GetDisplayName();
                            foreach (var file in kvp.Value)
                            {
                                if (file != null && file.Length > 0)
                                {
                                    var url = await _imageUploadService.UploadFileAsync(file, bienSo, typeDisplayName);
                                    existing.Media.Add(new CarMedia
                                    {
                                        CarId = existing.Id,
                                        Type = mediaType,
                                        Url = url,
                                        FileName = file.FileName
                                    });
                                }
                            }
                        }
                    }
                }

                // If no PrimaryImageFile was uploaded, respect vm.PrimaryImageUrl (selection from existing images)
                if (PrimaryImageFile == null || PrimaryImageFile.Length == 0)
                {
                    if (!string.IsNullOrEmpty(vm.PrimaryImageUrl))
                    {
                        if (existing.Media != null)
                        {
                            foreach (var m in existing.Media)
                            {
                                m.IsPrimary = string.Equals(m.Url, vm.PrimaryImageUrl, StringComparison.OrdinalIgnoreCase);
                            }
                        }
                        existing.PrimaryImageUrl = vm.PrimaryImageUrl;
                    }
                }

                existing.NgayCapNhat = DateTime.Now;
                _context.Update(existing);
                await _context.SaveChangesAsync();

                TempData["SuccessMessage"] = "Cập nhật thông tin xe thành công!";
                return RedirectToAction(nameof(Index));
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!CarExists(vm.Id))
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

            // on error reload media for view
            var reload = await _context.Cars.Include(c => c.Media).FirstOrDefaultAsync(c => c.Id == id);
            return View(reload != null ? CarViewModel.FromCar(reload) : vm);
        }

        // GET: Cars/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null) return NotFound();

            var car = await _context.Cars
                .Include(c => c.Media)
                .FirstOrDefaultAsync(m => m.Id == id);

            if (car == null) return NotFound();

            var vm = CarViewModel.FromCar(car);
            return View(vm);
        }

        // POST: Cars/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var car = await _context.Cars.Include(c => c.Media).FirstOrDefaultAsync(c => c.Id == id);
            if (car != null)
            {
                try
                {
                    // Delete all media files
                    if (car.Media != null)
                    {
                        foreach (var m in car.Media)
                        {
                            try
                            {
                                if (!string.IsNullOrEmpty(m.Url))
                                {
                                    await _imageUploadService.DeleteFileAsync(m.Url);
                                }
                            }
                            catch (Exception ex)
                            {
                                _logger.LogWarning(ex, "Failed to delete media {MediaId} for car {CarId}", m.Id, car.Id);
                            }
                        }
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

        // POST: Cars/DeleteMultiple
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteMultiple([FromForm] int[] ids)
        {
            if (ids == null || ids.Length == 0)
            {
                TempData["ErrorMessage"] = "Vui lòng chọn ít nhất một xe để xóa.";
                return RedirectToAction(nameof(Index));
            }

            var cars = await _context.Cars
                .Include(c => c.Media)
                .Where(c => ids.Contains(c.Id))
                .ToListAsync();

            var vms = cars.Select(CarViewModel.FromCar).ToList();
            // If you want to show a confirmation page: return View("Delete", vms);
            // If you are performing deletion directly, continue with existing deletion logic using `cars`.

            if (cars.Count == 0)
            {
                TempData["ErrorMessage"] = "Không tìm thấy xe nào tương ứng.";
                return RedirectToAction(nameof(Index));
            }

            try
            {
                foreach (var car in cars)
                {
                    if (car.Media != null)
                    {
                        foreach (var m in car.Media)
                        {
                            try
                            {
                                if (!string.IsNullOrEmpty(m.Url))
                                {
                                    await _imageUploadService.DeleteFileAsync(m.Url);
                                }
                            }
                            catch (Exception ex)
                            {
                                _logger.LogWarning(ex, "Failed to delete media {MediaId} for car {CarId}", m.Id, car.Id);
                            }
                        }
                    }
                }

                _context.Cars.RemoveRange(cars);
                await _context.SaveChangesAsync();

                TempData["SuccessMessage"] = $"Xóa {cars.Count} xe thành công!";
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting multiple cars");
                TempData["ErrorMessage"] = "Có lỗi xảy ra khi xóa các xe.";
            }

            return RedirectToAction(nameof(Index));
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteMedia([FromBody] DeleteMediaRequest request)
        {
            if (request == null || request.Id <= 0) return BadRequest();

            var media = await _context.CarMedias.Include(m => m.Car).FirstOrDefaultAsync(m => m.Id == request.Id);
            if (media == null) return NotFound();

            try
            {
                // delete physical file
                if (!string.IsNullOrEmpty(media.Url))
                    await _imageUploadService.DeleteFileAsync(media.Url);

                var car = media.Car;
                var deletedUrl = media.Url;

                _context.CarMedias.Remove(media);
                await _context.SaveChangesAsync();

                // If deleted item was primary, attempt to pick another image as primary
                if (car != null && car.PrimaryImageUrl == deletedUrl)
                {
                    var replacement = await _context.CarMedias
                        .Where(m => m.CarId == car.Id && m.Type != MediaType.Image_Primary)
                        .OrderByDescending(m => m.IsPrimary)
                        .ThenBy(m => m.CreatedAt)
                        .FirstOrDefaultAsync();

                    if (replacement != null)
                    {
                        replacement.IsPrimary = true;
                        car.PrimaryImageUrl = replacement.Url;
                        _context.Update(replacement);
                        _context.Update(car);
                        await _context.SaveChangesAsync();
                    }
                    else
                    {
                        car.PrimaryImageUrl = null;
                        _context.Update(car);
                        await _context.SaveChangesAsync();
                    }
                }

                return Json(new { success = true, deletedUrl });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting media id {MediaId}", request.Id);
                return Json(new { success = false });
            }
        }

        private bool CarExists(int id)
        {
            return _context.Cars.Any(e => e.Id == id);
        }
    }

    public class DeleteMediaRequest
    {
        public int Id { get; set; }
    }
}
