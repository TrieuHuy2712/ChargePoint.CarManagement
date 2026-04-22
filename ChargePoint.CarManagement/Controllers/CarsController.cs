using ChargePoint.CarManagement.Data;
using ChargePoint.CarManagement.Models;
using ChargePoint.CarManagement.Models.ViewModels;
using ChargePoint.CarManagement.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using OfficeOpenXml;

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
                var keyUpper = key.ToUpper();
                var keyNormalized = keyLower.Replace("-", "").Replace(".", "");

                query = query.Where(c =>
                    (c.BienSo != null && (c.BienSo.ToLower().Contains(keyLower) || c.BienSo.ToUpper().Contains(keyUpper) || c.BienSo.Contains(key) ||
                                          c.BienSo.Replace("-", "").Replace(".", "").ToLower().Contains(keyNormalized))) ||
                    (c.TenXe != null && (c.TenXe.ToLower().Contains(keyLower) || c.TenXe.ToUpper().Contains(keyUpper) || c.TenXe.Contains(key))) ||
                    (c.TenKhachHang != null && (c.TenKhachHang.ToLower().Contains(keyLower) || c.TenKhachHang.ToUpper().Contains(keyUpper) || c.TenKhachHang.Contains(key))) ||
                    (c.SoVIN != null && (c.SoVIN.ToLower().Contains(keyLower) || c.SoVIN.ToUpper().Contains(keyUpper) || c.SoVIN.Contains(key))) ||
                    (c.MauXe != null && (c.MauXe.ToLower().Contains(keyLower) || c.MauXe.ToUpper().Contains(keyUpper) || c.MauXe.Contains(key)))
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
            [Bind("Id,Stt,TenXe,SoLuong,MauXe,SoVIN,BienSo,BienSoCu,MauBienSo,TenKhachHang,ThongTinChoThue,NgayThue,NgayHetHan,OdoXe")] Car car,
            IFormFile? PrimaryImageFile)
        {
            if (await _context.Cars.AnyAsync(c => c.SoVIN.ToLower() == car.SoVIN.ToLower()))
            {
                ModelState.AddModelError("SoVIN", "Số VIN này đã tồn tại trong hệ thống.");
            }

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
                    var imageFiles = HttpContext.Request.Form.Files.Where(f => f.Name.StartsWith("ImageFiles[")).ToList();
                    if (imageFiles.Any())
                    {
                        foreach (var file in imageFiles)
                        {
                            if (file != null && file.Length > 0)
                            {
                                var name = file.Name;
                                var startIdx = name.IndexOf('[') + 1;
                                var endIdx = name.IndexOf(']');

                                if (startIdx > 0 && endIdx > startIdx)
                                {
                                    var typeStr = name.Substring(startIdx, endIdx - startIdx);
                                    if (Enum.TryParse<MediaType>(typeStr, out var mediaType))
                                    {
                                        var typeDisplayName = mediaType.GetDisplayName();
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
                    car.NguoiTao = User.Identity?.Name;
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
            IFormFile? PrimaryImageFile)
        {
            if (id != vm.Id)
            {
                return NotFound();
            }

            if (await _context.Cars.AnyAsync(c => c.SoVIN.ToLower() == vm.SoVIN.ToLower() && c.Id != id))
            {
                ModelState.AddModelError("SoVIN", "Số VIN này đã tồn tại trong hệ thống.");
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
                    populated.BienSoCu = vm.BienSoCu;
                    populated.MauBienSo = vm.MauBienSo;
                    populated.TenKhachHang = vm.TenKhachHang;
                    populated.ThongTinChoThue = vm.ThongTinChoThue;
                    populated.NgayThue = vm.NgayThue;
                    populated.NgayHetHan = vm.NgayHetHan;
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
                existing.BienSoCu = vm.BienSoCu;
                existing.MauBienSo = vm.MauBienSo;
                existing.TenKhachHang = vm.TenKhachHang;
                existing.ThongTinChoThue = vm.ThongTinChoThue;
                existing.NgayThue = vm.NgayThue;
                existing.NgayHetHan = vm.NgayHetHan;
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
                var imageFiles = HttpContext.Request.Form.Files.Where(f => f.Name.StartsWith("ImageFiles[")).ToList();
                if (imageFiles.Any())
                {
                    foreach (var file in imageFiles)
                    {
                        if (file != null && file.Length > 0)
                        {
                            var name = file.Name;
                            var startIdx = name.IndexOf('[') + 1;
                            var endIdx = name.IndexOf(']');

                            if (startIdx > 0 && endIdx > startIdx)
                            {
                                var typeStr = name.Substring(startIdx, endIdx - startIdx);
                                if (Enum.TryParse<MediaType>(typeStr, out var mediaType))
                                {
                                    var typeDisplayName = mediaType.GetDisplayName();
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
                existing.NguoiCapNhat = User.Identity?.Name;
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

                    TempData["SuccessMessage"] = "XÃ³a xe thÃ nh cÃ´ng!";
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

        // POST: Cars/BulkImport
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> BulkImport(IFormFile file)
        {
            if (file == null || file.Length == 0)
            {
                return Json(new { success = false, message = "Vui lòng chọn file Excel cần import." });
            }

            if (!file.FileName.EndsWith(".xlsx", StringComparison.OrdinalIgnoreCase))
            {
                return Json(new { success = false, message = "Hệ thống chỉ hỗ trợ upload định dạng .xlsx" });
            }

            try
            {
                ExcelPackage.License.SetNonCommercialPersonal("Your Name or Organization's Name");

                using var stream = new MemoryStream();
                await file.CopyToAsync(stream);
                using var package = new ExcelPackage(stream);

                var worksheet = package.Workbook.Worksheets.FirstOrDefault();
                if (worksheet == null)
                {
                    return Json(new { success = false, message = "File Excel không chứa Sheet dữ liệu nào." });
                }

                int rowCount = worksheet.Dimension?.Rows ?? 0;
                if (rowCount < 2)
                {
                    return Json(new { success = false, message = "File Excel trống, không có dữ liệu để thêm mới." });
                }
                int addedCount = 0;
                int updatedCount = 0;
                var skippedRows = new List<string>();
                var duplicateInFile = new Dictionary<string, List<int>>();

                var existingVinSet = await _context.Cars
                    .Where(c => !string.IsNullOrEmpty(c.SoVIN))
                    .Select(c => c.SoVIN.ToLower())
                    .ToListAsync();
                var existingVinLookup = new HashSet<string>(existingVinSet);

                var rowData = new List<(int Row, string? RawVin, string? RawTenXe, string? RawBienSo, string? RawBienSoCu, string? RawMauXe, string? RawLoaiBien, string? RawKhachHang, string? RawOdo, string? RawNgayThue, string? RawNgayHetHan)>();
                var vinRowsMap = new Dictionary<string, List<int>>();

                for (int row = 2; row <= rowCount; row++)
                {
                    var rawVin = worksheet.Cells[row, 2].Value?.ToString()?.Trim();
                    var rawTenXe = worksheet.Cells[row, 3].Value?.ToString()?.Trim();
                    var rawBienSo = worksheet.Cells[row, 4].Value?.ToString()?.Trim();
                    var rawBienSoCu = worksheet.Cells[row, 5].Value?.ToString()?.Trim();
                    var rawMauXe = worksheet.Cells[row, 6].Value?.ToString()?.Trim();
                    var rawLoaiBien = worksheet.Cells[row, 7].Value?.ToString()?.Trim();
                    var rawKhachHang = worksheet.Cells[row, 8].Value?.ToString()?.Trim();
                    var rawOdo = worksheet.Cells[row, 9].Value?.ToString()?.Trim();
                    var rawNgayThue = worksheet.Cells[row, 10].Value?.ToString()?.Trim();
                    var rawNgayHetHan = worksheet.Cells[row, 11].Value?.ToString()?.Trim();

                    rowData.Add((row, rawVin, rawTenXe, rawBienSo, rawBienSoCu, rawMauXe, rawLoaiBien, rawKhachHang, rawOdo, rawNgayThue, rawNgayHetHan));

                    if (!string.IsNullOrWhiteSpace(rawVin) && rawVin.Length == 17)
                    {
                        var vinLower = rawVin.ToLower();
                        if (!vinRowsMap.ContainsKey(vinLower))
                        {
                            vinRowsMap[vinLower] = new List<int>();
                        }
                        vinRowsMap[vinLower].Add(row);
                    }
                }

                foreach (var item in vinRowsMap.Where(x => x.Value.Count > 1))
                {
                    duplicateInFile[item.Key] = item.Value;
                }

                var newCars = new List<Car>();
                int maxStt = await _context.Cars.AnyAsync() ? await _context.Cars.MaxAsync(c => c.Stt) : 0;

                foreach (var item in rowData)
                {
                    if (string.IsNullOrWhiteSpace(item.RawVin) || item.RawVin.Length != 17)
                    {
                        skippedRows.Add($"Dòng {item.Row} (Sai định dạng 17 ký tự VIN)");
                        continue;
                    }

                    var vinLower = item.RawVin.ToLower();

                    if (duplicateInFile.ContainsKey(vinLower))
                    {
                        continue;
                    }

                    if (existingVinLookup.Contains(vinLower))
                    {
                        // VIN already exists in system: skip saving silently (do not show in duplicate list/UI).
                        continue;
                    }

                    var loaiBienLower = item.RawLoaiBien?.ToLower() ?? "";
                    bool isTrang = loaiBienLower.Contains("trang");
                    bool isVang = loaiBienLower.Contains("vang");

                    if (!string.IsNullOrWhiteSpace(loaiBienLower) && !isTrang && !isVang)
                    {
                        skippedRows.Add($"Dòng {item.Row} (Loại biển chỉ nhận chứa từ 'Trắng' hoặc 'Vàng')");
                        continue;
                    }

                    MauBienSo mauBien = isVang ? MauBienSo.Vang : MauBienSo.Trang;

                    int odo = 0;
                    if (!string.IsNullOrWhiteSpace(item.RawOdo))
                    {
                        int.TryParse(item.RawOdo, out odo);
                    }

                    DateTime? ngayThue = null;
                    if (!string.IsNullOrWhiteSpace(item.RawNgayThue) && DateTime.TryParseExact(item.RawNgayThue, "dd/MM/yyyy", System.Globalization.CultureInfo.InvariantCulture, System.Globalization.DateTimeStyles.None, out var nt))
                    {
                        ngayThue = nt;
                    }

                    DateTime? ngayHetHan = null;
                    if (!string.IsNullOrWhiteSpace(item.RawNgayHetHan) && DateTime.TryParseExact(item.RawNgayHetHan, "dd/MM/yyyy", System.Globalization.CultureInfo.InvariantCulture, System.Globalization.DateTimeStyles.None, out var nh))
                    {
                        ngayHetHan = nh;
                    }

                    maxStt++;
                    var car = new Car
                    {
                        Stt = maxStt,
                        SoVIN = item.RawVin,
                        TenXe = item.RawTenXe,
                        BienSo = string.IsNullOrWhiteSpace(item.RawBienSo) ? null : item.RawBienSo,
                        BienSoCu = string.IsNullOrWhiteSpace(item.RawBienSoCu) ? null : item.RawBienSoCu,
                        MauXe = item.RawMauXe,
                        MauBienSo = mauBien,
                        TenKhachHang = string.IsNullOrWhiteSpace(item.RawKhachHang) ? null : item.RawKhachHang,
                        OdoXe = odo,
                        NgayThue = ngayThue,
                        NgayHetHan = ngayHetHan,
                        SoLuong = 1,
                        NgayTao = DateTime.Now,
                        NguoiTao = User.Identity?.Name,
                    };

                    newCars.Add(car);
                    addedCount++;
                }

                if (newCars.Any())
                {
                    _context.Cars.AddRange(newCars);
                    await _context.SaveChangesAsync();
                }

                var duplicateVinRows = duplicateInFile
                    .Select(x => new { vin = x.Key.ToUpper(), rows = x.Value.OrderBy(r => r).ToList(), source = "File" })
                    .OrderBy(x => x.vin)
                    .ToList();

                foreach (var dup in duplicateVinRows)
                {
                    skippedRows.Add($"Dòng {string.Join(",", dup.rows)} (Trùng số VIN trong file: {dup.vin})");
                }

                var duplicateVins = duplicateVinRows
                    .Select(x => x.vin)
                    .Distinct()
                    .OrderBy(x => x)
                    .ToList();

                return Json(new
                {
                    success = true,
                    addedCount = addedCount,
                    updatedCount = updatedCount,
                    skippedRows = skippedRows,
                    duplicateVins = duplicateVins,
                    duplicateVinRows = duplicateVinRows
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Lỗi xảy ra trong khi import hàng loạt");
                var innerMsg = ex.InnerException != null ? ex.InnerException.Message : "";
                return Json(new { success = false, message = $"Có lỗi xảy ra: {ex.Message} {innerMsg}" });
            }
        }

        // GET: Cars/DownloadTemplate
        [AllowAnonymous]
        public IActionResult DownloadTemplate([FromServices] IWebHostEnvironment env)
        {
            try
            {
                var filePath = Path.Combine(env.WebRootPath, "template", "DATA XE-TEMPLATE.xlsx");

                if (!System.IO.File.Exists(filePath))
                {
                    TempData["ErrorMessage"] = "Không tìm thấy file mẫu trên máy chủ (wwwroot/template/DATA XE-TEMPLATE.xlsx).";
                    return RedirectToAction(nameof(Index));
                }

                var fileBytes = System.IO.File.ReadAllBytes(filePath);
                string excelName = "Template_Import_Xe.xlsx";
                return File(fileBytes, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", excelName);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Lỗi xảy ra khi tải file Template mẫu");
                TempData["ErrorMessage"] = "Có lỗi xảy ra trong quá trình tải file mẫu.";
                return RedirectToAction(nameof(Index));
            }
        }
    }

    public class DeleteMediaRequest
    {
        public int Id { get; set; }
    }
}

