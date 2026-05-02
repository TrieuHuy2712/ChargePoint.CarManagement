using ChargePoint.CarManagement.Application.Car.Queries;
using ChargePoint.CarManagement.Application.Tire.Commands;
using ChargePoint.CarManagement.Application.Tire.Queries;
using ChargePoint.CarManagement.Domain.Constants;
using ChargePoint.CarManagement.Domain.Entities;
using ChargePoint.CarManagement.Domain.Enums;
using ChargePoint.CarManagement.Domain.Models;
using ChargePoint.CarManagement.Domain.ViewModels.TireViewModels;
using Mediator;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace ChargePoint.CarManagement.Controllers
{
    [Authorize]
    public class TireController(
        IMediator mediator) : Controller
    {
        private readonly IMediator _mediator = mediator;

        // GET: Tire
        public async Task<IActionResult> Index(string q, int page = 1, int pageSize = 10)
        {
            if (page < 1) page = 1;
            if (pageSize < 1) pageSize = 10;
            var result = await _mediator.Send(new GetTireIndexQuery
            {
                Q = q,
                Page = page,
                PageSize = pageSize
            });

            return View(result);

        }

        // GET: Tire/CarDetail/5
        public async Task<IActionResult> CarDetail(int? id, CancellationToken cancellationToken)
        {
            if (id == null) return NotFound();
            var vm = await _mediator.Send(new GetTireCarDetailQuery { CarId = id }, cancellationToken);
            if (vm == null) return NotFound();
            return View(vm);
        }

        // GET: Tire/History/5 (CarId)
        public async Task<IActionResult> History(int? id, ViTriLop? viTri = null, CancellationToken cancellationToken = default)
        {
            if (id == null) return NotFound();

            var vm = await _mediator.Send(new GetTireHistoryQuery { CarId = id,  ViTri = viTri }, cancellationToken);
            if (vm == null) return NotFound();
            return View(vm);
        }

        // GET: Tire/Create/5 (CarId)
        public async Task<IActionResult> Create(int? id, ViTriLop? viTri = null, bool fromDraft = false, CancellationToken cancellationToken = default)
        {
            if (!id.HasValue) return NotFound();

            var car = await _mediator.Send(new GetCarByIdQuery { CarId = id.Value }, cancellationToken);

            if (car is null) return NotFound();

            var model = new TireRecord
            {
                CarId = car.Id,
                NgayThucHien = DateTime.Now,
                OdoThayLop = car.OdoXe,
                NguoiTao = User.Identity?.Name,
                ViTriLop = viTri ?? ViTriLop.TruocTrai,
            };

            ViewBag.FromDraft = fromDraft;
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
            [Bind(Prefix = "TireRecord")] TireRecord model,
            List<IFormFile>? HinhAnhChungTuFiles,
            List<IFormFile>? HinhAnhDOTFiles,
            List<ViTriLop>? selectedViTriLops,
            bool fromDraft = false, CancellationToken cancellationToken = default)
        {
            var car = await _mediator.Send(new GetCarByIdQuery { CarId = model.CarId }, cancellationToken);
            if (car == null) return NotFound();

            var vm = new TireCreateVM
            {
                Car = car,
                TireRecord = model
            };

            ViewBag.FromDraft = fromDraft;
            
            if (!ModelState.IsValid) return View(vm);

            var targetPositions = (selectedViTriLops ?? [])
                .Append(model.ViTriLop)
                .Distinct()
                .ToList();

            model.Car = car;
            var result = await _mediator.Send(new CreateTireCommand
            {
                Model = model,
                HinhAnhChungTuFiles = HinhAnhChungTuFiles,
                HinhAnhDOTFiles = HinhAnhDOTFiles,
                SelectedViTriLops = targetPositions,
                FromDraft = fromDraft,
            }, cancellationToken: cancellationToken);
            if (result.Success)
            {
                TempData["SuccessMessage"] = targetPositions.Count > 1
                        ? $"Hoàn tất: đã lưu hồ sơ bảo dưỡng và {targetPositions.Count} vị trí lốp thành công!"
                        : "Hoàn tất: đã lưu hồ sơ bảo dưỡng và hồ sơ lốp thành công!";
                return RedirectToAction(nameof(CarDetail), new { id = model.CarId });
            }
            else
            {
                ModelState.AddModelError("", result.Error!);
                return View(vm);
            }
        }

        // GET: Tire/Edit/5
        public async Task<IActionResult> Edit(int? id, CancellationToken cancellationToken = default)
        {
            if (id == null) return NotFound();

            var result = await _mediator.Send(new GetTireCarDetailEditViewQuery { TireId = id.Value }, cancellationToken);
            if (result == null) return NotFound();
            return View(result);
        }

        // POST: Tire/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        [RequestSizeLimit(50 * 1024 * 1024)]
        public async Task<IActionResult> Edit(
            int id,
            [Bind(Prefix = "Record")] TireRecord model,
            List<IFormFile>? HinhAnhChungTuFiles,
            List<IFormFile>? HinhAnhDOTFiles,
            List<ViTriLop>? selectedViTriLops,
            bool fromDraft = false,
            CancellationToken cancellationToken = default)
        {
            if (id != model.Id)
                return NotFound();

            // Load existing record first to get CarId
            var existingRecord = await _mediator.Send(new GetTireCarDetailEditViewQuery { TireId = id }, cancellationToken);
            if (existingRecord == null)
                return NotFound();

            // Create view model for potential return
            var vm = new TireEditVM
            {
                Car = existingRecord.Car,
                Record = model
            };


            if (!ModelState.IsValid) return View(vm);

            model.Car = existingRecord.Car;
            var result = await _mediator.Send(new EditTireCommand
            {
                Model = model,
                HinhAnhChungTuFiles = HinhAnhChungTuFiles,
                HinhAnhDOTFiles = HinhAnhDOTFiles,
                SelectedViTriLops = selectedViTriLops ?? [model.ViTriLop],
                FromDraft = fromDraft
            }, cancellationToken);
            if (result.Success)
            {
                TempData[nameof(Messages.SuccessMessage)] = (selectedViTriLops?.Count ?? 0) > 1
                        ? $"Hoàn tất: đã lưu hồ sơ bảo dưỡng và {selectedViTriLops.Count} vị trí lốp thành công!"
                        : "Hoàn tất: đã lưu hồ sơ bảo dưỡng và hồ sơ lốp thành công!";
                return RedirectToAction(nameof(CarDetail), new { id = model.CarId });
            }
            else
            {
                ModelState.AddModelError("", result.Error!);
                return View(vm);
            }
        }

        // GET: Tire/Details/5
        public async Task<IActionResult> Details(int? id, CancellationToken cancellationToken = default)
        {
            if (id == null) return NotFound();
            var result = await _mediator.Send(new GetTireDetailByIdQuery { Id = id.Value }, cancellationToken);

            if (result == null) return NotFound();
            return View(result);
        }

        // POST: Tire/Delete/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(
            int id, CancellationToken cancellationToken = default)
        {
            var result = await _mediator.Send(new DeleteTireCommand { Id = id }, cancellationToken);
            if (result.Success)
            {
                TempData[nameof(Messages.SuccessMessage)] = "Đã xóa hồ sơ lốp!";
                return RedirectToAction(nameof(History), new {id = result.Value});
            }
            else
            {
                TempData[nameof(Messages.ErrorMessage)] = $"Có lỗi xảy ra: {result.Error}";
                return RedirectToAction(nameof(History));
            }
        }

        // POST: Tire/DeleteImage
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteImage([FromBody] DeleteImageRequest request, CancellationToken cancellationToken = default)
        {
            if (request == null || string.IsNullOrEmpty(request.ImageUrl))
                return Json(new { success = false, message = "Thông tin không hợp lệ" });

            var result = await _mediator.Send(new DeleteTireImageCommand
            {
                RecordId = request.RecordId,
                ImageUrl = request.ImageUrl,
                ImageType = request.ImageType
            }, cancellationToken);

            if (result.Success)
            {
                return Json(new { success = true, message = "Đã xóa hình ảnh" });
            }
            else
            {
                return Json(new { success = false, message = result.Error });
            }
        }
    }
}
