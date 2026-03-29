using ChargePoint.CarManagement.Data;
using ChargePoint.CarManagement.Models;
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
        public async Task<IActionResult> Index()
        {
            // Lấy danh sách xe riêng để tránh SQL APPLY
            var cars = await _context.Cars.OrderBy(c => c.Stt).ToListAsync();
            var allChecks = await _context.TrafficViolationChecks.ToListAsync();

            var carsWithLastCheck = cars.Select(c => new TrafficViolationIndexVM
            {
                Car = c,
                LastCheck = allChecks
                    .Where(v => v.CarId == c.Id)
                    .OrderByDescending(v => v.NgayKiemTra)
                    .FirstOrDefault()
            }).ToList();

            return View(carsWithLastCheck);
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
