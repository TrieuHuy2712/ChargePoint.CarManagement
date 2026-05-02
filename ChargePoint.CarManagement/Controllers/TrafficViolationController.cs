using ChargePoint.CarManagement.Application.Car.Queries;
using ChargePoint.CarManagement.Application.TrafficViolation.Commands;
using ChargePoint.CarManagement.Application.TrafficViolation.Queries;
using ChargePoint.CarManagement.Domain.Constants;
using ChargePoint.CarManagement.Domain.Enums;
using ChargePoint.CarManagement.Domain.ViewModels.TrafficViolationViewModels;
using Mediator;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace ChargePoint.CarManagement.Controllers
{
    [Authorize]
    public class TrafficViolationController(
        IMediator mediator) : Controller
    {
        private readonly IMediator _mediator = mediator;

        // GET: TrafficViolation
        public async Task<IActionResult> Index(string q, ViolationStatus? status, int page = 1, int pageSize = 10)
        {
            if (page < 1) page = 1;
            if (pageSize < 1) pageSize = 10;

            // Base query
            var result = await _mediator.Send(new GetTrafficViolationIndexQuery
            {
                Q = q,
                Status = status,
                Page = page,
                PageSize = pageSize
            });

            ViewBag.CurrentStatus = status;
            return View(result);
        }

        // GET: TrafficViolation/History/5
        public async Task<IActionResult> History(int? id, CancellationToken cancellationToken = default)
        {
            if (id == null) return NotFound();

            var car = await _mediator.Send(new GetCarByIdQuery { CarId = id.Value }, cancellationToken);
            if (car == null) return NotFound();

           var result = await _mediator.Send(new GetTrafficViolationHistoryQuery { Car = car, Id = id.Value }, cancellationToken);

            return View(result);
        }

        // GET: TrafficViolation/Check/5
        public async Task<IActionResult> Check(int? id,
            int? soLuongViPham, string? noiDungViPham, string? diaDiemViPham,
            string? ngayGioViPham, string? ghiChu, CancellationToken cancellationToken = default)
        {
            if (id == null) return NotFound();

            var car = await _mediator.Send(new GetCarByIdQuery { CarId = id.Value }, cancellationToken);
            if (car == null) return NotFound();

            // Tạo model với dữ liệu đã có (nếu có)
            var result = await _mediator.Send(new GetTrafficViolationCheckQuery
            {
                Car = car,
                SoLuongViPham = soLuongViPham,
                NoiDungViPham = noiDungViPham,
                DiaDiemViPham = diaDiemViPham,
                NgayGioViPham = ngayGioViPham,
                GhiChu = ghiChu
            }, cancellationToken);

            return View(result);
        }

        // POST: TrafficViolation/CheckOnline - API tra cứu trực tuyến
        [HttpPost]
        public async Task<IActionResult> CheckOnline(int carId, CancellationToken cancellationToken)
        {
            var car = await _mediator.Send(new GetCarDetailQuery { CarId = carId }, cancellationToken);
            if (car == null || string.IsNullOrEmpty(car.BienSo))
            {
                return Json(new { success = false, message = "Không tìm thấy thông tin xe hoặc biển số" });
            }

            var result = await _mediator.Send(new GetTrafficViolationCheckOnlineQuery { BienSo = car.BienSo }, cancellationToken);
            return Json(result);
        }

        // POST: TrafficViolation/Check
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Check(TrafficViolationCheckVM viewModel, CancellationToken cancellationToken = default)
        {
            // Loại bỏ validation cho các field không cần thiết khi tạo mới
            ModelState.Remove("TrafficViolationCheck.Id");
            ModelState.Remove("TrafficViolationCheck.Car");
            ModelState.Remove("TrafficViolationCheck.NguoiTao");
            ModelState.Remove("Car");

            var model = viewModel.TrafficViolationCheck!;

            if (ModelState.IsValid)
            {
                var result = await _mediator.Send(new CreateTrafficViolationCommand { Model = model }, cancellationToken);

                if (result.Success)
                {
                    TempData[nameof(Messages.SuccessMessage)] = "Đã lưu kết quả kiểm tra phạt nguội!";
                    return RedirectToAction(nameof(Index));
                }
            }

            var car = await _mediator.Send(new GetCarByIdQuery { CarId = model.CarId }, cancellationToken);
            viewModel.Car = car!;
            return View(viewModel);
        }

        // POST: TrafficViolation/Delete/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(int id, CancellationToken cancellationToken)
        {
            var result = await _mediator.Send(new DeleteTrafficViolationCommand { Id = id }, cancellationToken);
            if (result.Success)
            {
                TempData[nameof(Messages.SuccessMessage)] = "Đã xóa bản ghi kiểm tra!";
            }
            else
            {
                TempData[nameof(Messages.ErrorMessage)] = result.Error;
            }
            return RedirectToAction(nameof(History), new { id = result.Value });
        }

        // GET: TrafficViolation/EditDetail/5
        public async Task<IActionResult> EditDetail(int? id, CancellationToken cancellationToken = default)
        {
            if (id == null) return NotFound();

            var result = await _mediator.Send(new GetTrafficViolationByIdQuery { Id = id.Value }, cancellationToken);

            if (result == null ) return NotFound();

            return View(result);
        }

        // POST: TrafficViolation/EditDetail/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> EditDetail(int id, TrafficViolationCheckVM viewModel, CancellationToken cancellationToken = default)
        {
            if (id != viewModel.TrafficViolationCheck.Id)
            {
                return NotFound();
            }

            // Remove validation for fields we don't need
            ModelState.Remove("TrafficViolationCheck.Car");
            ModelState.Remove("Car");
            ModelState.Remove("TrafficViolationCheck.NguoiTao");

            if (ModelState.IsValid)
            {

                var result = await _mediator.Send(new EditTrafficViolationCommand
                {
                    Model = viewModel.TrafficViolationCheck
                }, cancellationToken);
                if (result.Success)
                {
                    return RedirectToAction(nameof(History), new { id = viewModel.TrafficViolationCheck.CarId });
                }
                else
                {
                    ModelState.AddModelError("", $"Lỗi khi cập nhật: {result.Error}");
                }
            }

            // If we got this far, something failed, reload the data
            var car = await _mediator.Send(new GetCarByIdQuery
            {
                CarId = viewModel.TrafficViolationCheck.CarId
            }, cancellationToken);
            viewModel.Car = car;
            return View(viewModel);
        }

        // POST: TrafficViolation/UpdateStatus
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> UpdateStatus(
            int id, ViolationStatus status, string? nguoiXuLy, CancellationToken cancellationToken = default)
        {
            var check = await _mediator.Send(new GetTrafficViolationByIdQuery { Id = id }, cancellationToken);
            if (check == null || check.TrafficViolationCheck == null)
            {
                return NotFound();
            }

            var result = await _mediator.Send(new UpdateStatusTrafficViolationCommand
            {
                Model = check.TrafficViolationCheck,
                TrangThaiXuLy = status
            }, cancellationToken);
            TempData[nameof(Messages.SuccessMessage)] = "Đã cập nhật trạng thái xử lý!";

            return RedirectToAction(nameof(History), new { id = check.Car.Id });
        }
    }
}
