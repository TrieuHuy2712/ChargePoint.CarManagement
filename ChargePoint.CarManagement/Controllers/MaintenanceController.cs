using ChargePoint.CarManagement.Application.Car.Queries;
using ChargePoint.CarManagement.Application.Maintenance.Commands;
using ChargePoint.CarManagement.Application.Maintenance.Queries;
using ChargePoint.CarManagement.Application.Tire.Queries;
using ChargePoint.CarManagement.Domain.Constants;
using ChargePoint.CarManagement.Domain.Entities;
using ChargePoint.CarManagement.Domain.Enums;
using ChargePoint.CarManagement.Domain.ViewModels.MaintenanceViewModels;
using Mediator;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace ChargePoint.CarManagement.Controllers
{
    [Authorize]
    public class MaintenanceController(IMediator mediator) : Controller
    {
        private readonly IMediator _mediator = mediator;

        // GET: Maintenance
        public async Task<IActionResult> Index(string q, int page = 1, int pageSize = 10)
        {
            if (page < 1) page = 1;
            if (pageSize < 1) pageSize = 10;

            var query = new GetMaintenanceIndexQuery
            {
                Q = q,
                Page = page,
                PageSize = pageSize
            };

            var result = await _mediator.Send(query);
            return View(result);
        }

        // GET: Maintenance/History/5
        public async Task<IActionResult> History(int? id, CancellationToken cancellationToken = default)
        {
            if (id == null) return NotFound();

            var car = await _mediator.Send(new GetCarByIdQuery { CarId = id.Value }, cancellationToken);
            if (car == null) return NotFound();

            var results = await _mediator.Send(new GetMaintenanceHistoryQuery { CarId = id.Value, Car = car }, cancellationToken);
            return View(results);
        }

        // GET: Maintenance/Details/5
        public async Task<IActionResult> Details(int? id, CancellationToken cancellationToken = default)
        {
            if (id == null)
                return NotFound();

            var result = await _mediator.Send(new GetMaintenanceDetailQuery { Id = id.Value }, cancellationToken);

            if (result == null)
                return NotFound();

            return View(result);
        }

        // GET: Maintenance/Create/5 (CarId)
        public async Task<IActionResult> Create(int? id, CancellationToken cancellationToken = default)
        {
            var query = await _mediator.Send(new GetMaintenanceCreateQuery { CarId = id }, cancellationToken);
            if (query == null)
                return NotFound();

            return View(query);
        }

        // POST: Maintenance/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        [RequestSizeLimit(50 * 1024 * 1024)] // 50MB
        public async Task<IActionResult> Create(
            MaintenanceCreateVM viewModel,
            List<IFormFile>? HinhAnhChungTuFiles,
            string? buttonAction,
            CancellationToken cancellationToken = default)
        {
            var model = viewModel.MaintenanceRecord;
            ModelState.Remove(nameof(MaintenanceRecord.Id));
            var buttonActionValue = string.IsNullOrWhiteSpace(buttonAction)
                ? "save"
                : buttonAction;
            var buttonActionEnum = buttonActionValue.ToEnum<ButtonAction>();

            // Có 2 loại lưu từ Maintenance:
            // 1) Lưu trực tiếp vào DB.
            // 2) Chuyển qua Tire để hoàn tất lưu chung Maintenance + Tire trong 1 transaction

            if (ModelState.IsValid)
            {
                var loaiHoSo = viewModel.LoaiHoSo;
                var car = await _mediator.Send(new GetCarByIdQuery { CarId = model.CarId }, cancellationToken);
                if (car == null)
                {
                    ModelState.AddModelError("", "Không tìm thấy xe");
                    
                    var errorVM = new MaintenanceCreateVM
                    {
                        MaintenanceRecord = model,
                        Cars = [],
                        SelectListCars = new SelectList((viewModel.Cars ?? []).OrderBy(c => c.BienSo).ToList(), "Id", "BienSo"),
                        LoaiHoSo = model.LoaiHoSo
                    };
                    return View(errorVM);
                }

                var result = await _mediator.Send(new CreateMaintenanceCommand
                {
                    Car = car,
                    Model = model,
                    ButtonAction = buttonActionEnum,
                    HinhAnhChungTuFiles = HinhAnhChungTuFiles
                }, cancellationToken);

                if (result.Success)
                {
                    TempData[nameof(Messages.SuccessMessage)] = "Thêm hồ sơ bảo dưỡng thành công!";
                    if (buttonActionEnum == ButtonAction.Next)
                        return RedirectToAction(nameof(TireController.Create), "Tire", new { id = model.CarId, fromDraft = true });
                    return RedirectToAction(nameof(History), new { id = model.CarId });
                }
            }
            var createVM = new MaintenanceCreateVM
            {
                MaintenanceRecord = model,
                Cars = viewModel.Cars,
                SelectListCars = new SelectList(viewModel.Cars.OrderBy(c => c.BienSo).ToList(), "Id", "BienSo")
            };
            return View(createVM);
        }

        // GET: Maintenance/Edit/5
        public async Task<IActionResult> Edit(int? id, CancellationToken cancellationToken = default)
        {
            if (id == null) return NotFound();

            var record = await _mediator.Send(new GetMaintenanceDetailQuery { Id = id.Value }, cancellationToken);

            if (record == null) return NotFound();


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
            List<IFormFile>? HinhAnhChungTuFiles,
            string? buttonAction,
            CancellationToken cancellationToken)
        {
            if (id != model.Id)
                return NotFound();

            var buttonActionValue = string.IsNullOrWhiteSpace(buttonAction)
                ? "save"
                : buttonAction;
            var buttonActionEnum = buttonActionValue.ToEnum<ButtonAction>();
            ModelState.Remove(nameof(MaintenanceRecord.Id));

            if (ModelState.IsValid)
            {
                var existingRecord = await _mediator.Send(new GetMaintenanceDetailQuery { Id = id }, cancellationToken);
                if (existingRecord == null)
                    return NotFound();

                var car = await _mediator.Send(new GetCarByIdQuery { CarId = model.CarId }, cancellationToken);
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

                // Nếu chọn "Lưu & Tiếp theo", lưu tạm vào cache và chuyển qua trang tạo Tire
                var result = await _mediator.Send(new EditMaintenanceCommand
                {
                    Car = car,
                    Model = model,
                    Action = buttonActionEnum,
                    HinhAnhChungTuFiles = HinhAnhChungTuFiles
                }, cancellationToken);

                if (result.Success)
                {
                    if (buttonActionEnum == ButtonAction.Next)
                    {
                        var tireDetail = await _mediator.Send(new GetTireCarDetailQuery { CarId = model.CarId }, cancellationToken);
                        if (tireDetail == null)
                        {
                            return RedirectToAction(nameof(TireController.Create), "Tire", new { id = model.CarId, fromDraft = true });
                        }
                        else
                        {
                            return RedirectToAction(nameof(TireController.Edit), "Tire", new { id = tireDetail.TireRecordsPosition.FirstOrDefault().LastRecord.Id, fromDraft = true });
                        }
                                    
                    }
                    TempData[nameof(Messages.SuccessMessage)] = "Cập nhật hồ sơ bảo dưỡng thành công!";
                    return RedirectToAction(nameof(History), new { id = model.CarId });
                }
                else
                {
                    ModelState.AddModelError("", result.Error ?? "Có lỗi xảy ra");
                    var errorVM = new MaintenanceEditVM
                    {
                        Car = car,
                        Record = model
                    };
                    return View(errorVM);
                }
            }

            // Reload car for view in case of validation error
            var carForView = await _mediator.Send(new GetCarByIdQuery { CarId = model.CarId }, cancellationToken);
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
        public async Task<IActionResult> Delete(int id, CancellationToken cancellationToken = default)
        {
            var record = await _mediator.Send(new GetMaintenanceDetailQuery { Id = id }, cancellationToken);
            if (record != null)
            {
                var result = await _mediator.Send(new DeleteMaintenanceCommand { Model = record }, cancellationToken);
                if (result.Success)
                {
                    TempData[nameof(Messages.SuccessMessage)] = "Đã xóa hồ sơ bảo dưỡng!";
                    return RedirectToAction(nameof(History), new { id = record.CarId });
                }
            }

            return RedirectToAction(nameof(Index));
        }

        // POST: Maintenance/DeleteImage
        [HttpPost]
        public async Task<IActionResult> DeleteImage(int recordId, string imageUrl, CancellationToken cancellationToken = default)
        {
            var record = await _mediator.Send(new GetMaintenanceDetailQuery { Id = recordId }, cancellationToken);
            if (record == null)
                return Json(new { success = false, message = "Không tìm thấy hồ sơ" });

            var result = await _mediator.Send(
                new DeleteImageMaintenaceCommand {
                    ImageUrls = record.DanhSachHinhAnh,
                    ImageUrl = imageUrl,
                    Model = record
                }, cancellationToken);

            if (result.Success)
            {
                return Json(new { success = true, message = "Đã xóa hình ảnh" });
            }
            else
            {
                return Json(new { success = false, message = result.Error ?? "Có lỗi xảy ra" });
            }
        }
    }
}
