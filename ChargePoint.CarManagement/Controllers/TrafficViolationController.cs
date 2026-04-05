using ChargePoint.CarManagement.Data;
using ChargePoint.CarManagement.Models;
using ChargePoint.CarManagement.Models.Enums;
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
        public async Task<IActionResult> Index(string q, ViolationStatus? status, int page = 1, int pageSize = 10)
        {
            if (page < 1) page = 1;
            if (pageSize < 1) pageSize = 10;

            // Base query
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

            // Load checks only for current page cars
            var checksQuery = _context.TrafficViolationChecks
                .Where(v => carIds.Contains(v.CarId));

            // Filter by status if provided
            if (status.HasValue)
            {
                checksQuery = checksQuery.Where(v => v.TrangThaiXuLy == status.Value);
            }

            var checks = await checksQuery.ToListAsync();

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

            ViewBag.CurrentStatus = status;
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

        // GET: TrafficViolation/EditDetail/5
        public async Task<IActionResult> EditDetail(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var check = await _context.TrafficViolationChecks
                .Include(c => c.Car)
                .FirstOrDefaultAsync(c => c.Id == id);

            if (check == null)
            {
                return NotFound();
            }

            var viewModel = new TrafficViolationCheckVM
            {
                Car = check.Car,
                TrafficViolationCheck = check
            };

            return View(viewModel);
        }

        // POST: TrafficViolation/EditDetail/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> EditDetail(int id, TrafficViolationCheckVM viewModel)
        {
            if (id != viewModel.TrafficViolationCheck.Id)
            {
                return NotFound();
            }

            // Remove validation for fields we don't need
            ModelState.Remove("TrafficViolationCheck.Car");
            ModelState.Remove("Car");
            ModelState.Remove("TrafficViolationCheck.NguoiKiemTra");

            if (ModelState.IsValid)
            {
                try
                {
                    var check = await _context.TrafficViolationChecks.FindAsync(id);
                    if (check == null)
                    {
                        return NotFound();
                    }

                    // Update fields
                    var model = viewModel.TrafficViolationCheck;
                    check.SoLuongViPham = model.SoLuongViPham;
                    check.CoViPham = model.SoLuongViPham > 0; // Auto-update based on count
                    check.NgayGioViPham = model.NgayGioViPham;
                    check.NoiDungViPham = model.NoiDungViPham;
                    check.DiaDiemViPham = model.DiaDiemViPham;
                    check.TrangThaiXuLy = model.TrangThaiXuLy;
                    check.NgayCapNhatTrangThai = DateTime.Now;
                    check.NguoiXuLy = model.NguoiXuLy;
                    check.GhiChu = model.GhiChu;

                    _context.Update(check);
                    await _context.SaveChangesAsync();

                    TempData["SuccessMessage"] = "Đã cập nhật thông tin kiểm tra thành công!";
                    return RedirectToAction(nameof(History), new { id = check.CarId });
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!TrafficViolationCheckExists(id))
                    {
                        return NotFound();
                    }
                    else
                    {
                        throw;
                    }
                }
                catch (Exception ex)
                {
                    ModelState.AddModelError("", $"Lỗi khi cập nhật: {ex.Message}");
                }
            }

            // If we got this far, something failed, reload the data
            var car = await _context.Cars.FindAsync(viewModel.TrafficViolationCheck.CarId);
            viewModel.Car = car;
            return View(viewModel);
        }

        // POST: TrafficViolation/UpdateStatus
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> UpdateStatus(int id, ViolationStatus status, string? nguoiXuLy)
        {
            var check = await _context.TrafficViolationChecks.FindAsync(id);
            if (check == null)
            {
                return NotFound();
            }

            check.TrangThaiXuLy = status;
            check.NgayCapNhatTrangThai = DateTime.Now;
            check.NguoiXuLy = nguoiXuLy ?? User.Identity?.Name;

            await _context.SaveChangesAsync();
            TempData["SuccessMessage"] = "Đã cập nhật trạng thái xử lý!";

            return RedirectToAction(nameof(History), new { id = check.CarId });
        }

        private bool TrafficViolationCheckExists(int id)
        {
            return _context.TrafficViolationChecks.Any(e => e.Id == id);
        }
    }
}
