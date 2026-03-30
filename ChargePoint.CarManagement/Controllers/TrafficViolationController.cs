using ChargePoint.CarManagement.Data;
using ChargePoint.CarManagement.Models;
using ChargePoint.CarManagement.Models.ViewModels;
using ChargePoint.CarManagement.Models.ViewModels.TrafficViolationViewModels;
using ChargePoint.CarManagement.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace ChargePoint.CarManagement.Controllers
{
    [Authorize]
    public class TrafficViolationController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly ITrafficViolationService _violationService;

        public TrafficViolationController(
            ApplicationDbContext context,
            ITrafficViolationService violationService)
        {
            _context = context;
            _violationService = violationService;
        }

        // GET: TrafficViolation
        public async Task<IActionResult> Index(string q, int page = 1, int pageSize = 10)
        {
            if (page < 1) page = 1;
            if (pageSize < 1) pageSize = 10;

            // Base query
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

            // Load checks only for current page cars
            var checks = await _context.TrafficViolationChecks
                .Where(v => carIds.Contains(v.CarId))
                .ToListAsync();

            var items = pagedCars.Select(c => new TrafficViolationIndexVM
            {
                Car = c,
                LastCheck = checks.Where(v => v.CarId == c.Id)
                                  .OrderByDescending(v => v.NgayKiemTra)
                                  .FirstOrDefault(),
            }).ToList();

            var viewModel = new PagedResult<TrafficViolationIndexVM>
            {
                Items = items,
                PageNumber = page,
                PageSize = pageSize,
                TotalCount = totalCount,
                SearchQuery = q ?? string.Empty
            };

            return View(viewModel);
        }

        // GET: TrafficViolation/History/5
        public async Task<IActionResult> History(int? id)
        {
            if (id == null) return NotFound();

            var car = await _context.Cars.FindAsync(id);
            if (car == null) return NotFound();

            var checks = await _context.TrafficViolationChecks
                .Where(v => v.CarId == id)
                .OrderByDescending(v => v.NgayKiemTra)
                .ToListAsync();

            return View(new TrafficViolationHistoryVM
            {
                Car = car,
                Checks = checks
            });
        }

        // GET: TrafficViolation/Check/5
        public async Task<IActionResult> Check(int? id)
        {
            if (id == null) return NotFound();

            var car = await _context.Cars.FindAsync(id);
            if (car == null) return NotFound();

            var model = new TrafficViolationCheck
            {
                CarId = car.Id,
                NgayKiemTra = DateTime.Now,
                NguoiKiemTra = User.Identity?.Name
            };

            return View(new TrafficViolationCheckVM
            {
                TrafficViolationCheck = model,
                Car = car
            });
        }

        // POST: TrafficViolation/CheckOnline - API tra cứu trực tuyến
        [HttpPost]
        public async Task<IActionResult> CheckOnline(int carId)
        {
            var car = await _context.Cars.FindAsync(carId);
            if (car == null || string.IsNullOrEmpty(car.BienSo))
            {
                return Json(new { success = false, message = "Không tìm thấy thông tin xe hoặc biển số" });
            }

            var result = await _violationService.CheckViolationAsync(car.BienSo);
            return Json(result);
        }

        // POST: TrafficViolation/Check
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Check(TrafficViolationCheck model)
        {
            // Loại bỏ validation cho Id vì sẽ tự sinh
            ModelState.Remove("Id");

            if (ModelState.IsValid)
            {
                try
                {
                    // Tạo entity mới để tránh conflict Id
                    var newRecord = new TrafficViolationCheck
                    {
                        CarId = model.CarId,
                        NgayKiemTra = DateTime.Now,
                        NguoiKiemTra = User.Identity?.Name,
                        CoViPham = model.CoViPham,
                        SoLuongViPham = model.SoLuongViPham,
                        TongTienPhat = model.TongTienPhat,
                        GhiChu = model.GhiChu
                    };

                    _context.TrafficViolationChecks.Add(newRecord);
                    await _context.SaveChangesAsync();

                    TempData["SuccessMessage"] = "Đã lưu kết quả kiểm tra phạt nguội!";
                    return RedirectToAction(nameof(Index));
                }
                catch (Exception ex)
                {
                    ModelState.AddModelError("", $"Lỗi khi lưu: {ex.Message}");
                }
            }

            var car = await _context.Cars.FindAsync(model.CarId);
            return View(new TrafficViolationCheckVM
            {
                Car = car,
                TrafficViolationCheck = model,
            });
        }

        // POST: TrafficViolation/Delete/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(int id)
        {
            var check = await _context.TrafficViolationChecks.FindAsync(id);
            if (check != null)
            {
                _context.TrafficViolationChecks.Remove(check);
                await _context.SaveChangesAsync();
                TempData["SuccessMessage"] = "Đã xóa bản ghi kiểm tra!";
            }
            return RedirectToAction(nameof(Index));
        }
    }
}
