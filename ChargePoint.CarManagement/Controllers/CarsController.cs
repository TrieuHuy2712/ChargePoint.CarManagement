using ChargePoint.CarManagement.Application.Car.Commands;
using ChargePoint.CarManagement.Application.Car.Queries;
using ChargePoint.CarManagement.Domain.Constants;
using ChargePoint.CarManagement.Domain.Entities;
using ChargePoint.CarManagement.Domain.ViewModels;
using Mediator;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace ChargePoint.CarManagement.Controllers
{
    [Authorize]
    public class CarsController(IMediator mediator) : Controller
    {
        private readonly IMediator _mediator = mediator;

        // GET: Cars
        public async Task<IActionResult> Index(string q, int page = 1, int pageSize = 10, CancellationToken cancellationToken = default)
        {
            if (page < 1) page = 1;
            if (pageSize < 1) pageSize = 10;

            var result = await _mediator.Send(new GetCarIndexQuery
            {
                Q = q,
                PageNumber = page,
                PageSize = pageSize
            }, cancellationToken);

            return View(result);
        }

        // GET: Cars/Details/5
        public async Task<IActionResult> Details(int? id, CancellationToken cancellationToken = default)
        {
            if (id == null) return NotFound();

            var vm = await _mediator.Send(new GetCarDetailQuery { CarId = id.Value }, cancellationToken);
            return View(vm);
        }

        // GET: Cars/Create
        public async Task<IActionResult> Create(CancellationToken cancellationToken = default)
        {
            var car = await _mediator.Send(new GetCarCreateQuery(), cancellationToken);
            return View(car);
        }

        // POST: Cars/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(
            [Bind("Id,Stt,TenXe,SoLuong,MauXe,SoVIN,BienSo,BienSoCu,MauBienSo,TenKhachHang,ThongTinChoThue,NgayThue,NgayHetHan,OdoXe")] Car car,
            IFormFile? PrimaryImageFile,
            CancellationToken cancellationToken = default)
        {
            if (await _mediator.Send(new GetCarVINExistQuery { VINNumber = car.SoVIN }, cancellationToken))
            {
                ModelState.AddModelError("SoVIN", "Số VIN này đã tồn tại trong hệ thống.");
            }

            if (ModelState.IsValid)
            {
                var result = await _mediator.Send(new CreateCarCommand
                {
                    Model = car,
                    PrimaryImageFile = PrimaryImageFile
                }, cancellationToken);

                if (result.Success)
                {
                    return RedirectToAction(nameof(Index));
                }
                else
                {
                    ModelState.AddModelError(string.Empty, result.Error);
                }
            }

            return View(car);
        }

        // GET: Cars/Edit/5
        public async Task<IActionResult> Edit(int? id, CancellationToken cancellationToken = default)
        {
            if (id == null) return NotFound();

            var vm = await _mediator.Send(new GetCarDetailQuery { CarId = id.Value }, cancellationToken);
            return View(vm);
        }

        // POST: Cars/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(
            int id,
            CarViewModel vm,
            IFormFile? PrimaryImageFile,
            CancellationToken cancellationToken = default)
        {
            if (id != vm.Id) return NotFound();

            if (await _mediator.Send(new GetCarVINExistQuery { VINNumber = vm.SoVIN }, cancellationToken) && vm.Id != id)
            {
                ModelState.AddModelError("SoVIN", "Số VIN này đã tồn tại trong hệ thống.");
            }

            if (!ModelState.IsValid)
            {
                // reload existing media and preserve user-entered fields for the form
                var existingForView = await _mediator.Send(new GetCarDetailQuery { CarId = id }, cancellationToken);
                if (existingForView != null)
                {
                    var populated = existingForView;
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

            var result = await _mediator.Send(new EditCarCommand
            {
                Model = vm,
                PrimaryImageFile = PrimaryImageFile
            }, cancellationToken);

            if (result.Success)
            {
                TempData[nameof(Messages.SuccessMessage)] = "Cập nhật thông tin xe thành công!";
                return RedirectToAction(nameof(Index));
            }
            else {
                ModelState.AddModelError(string.Empty, result.Error);
            }

            // on error reload media for view
            var reloadVM = await _mediator.Send(new GetCarDetailQuery { CarId = id }, cancellationToken);
            return View(reloadVM ?? vm);
        }

        // GET: Cars/Delete/5
        public async Task<IActionResult> Delete(int? id, CancellationToken cancellationToken = default)
        {
            if (id == null) return NotFound();

            var car = await _mediator.Send(new GetCarDetailQuery { CarId = id.Value }, cancellationToken);
            return View(car);
        }

        // POST: Cars/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id, CancellationToken cancellationToken = default)
        {
            var result = await _mediator.Send(new DeleteCarCommand { CarId = id }, cancellationToken);
            if (result.Success)
            {
                TempData[nameof(Messages.SuccessMessage)] = "Xóa xe thành công!";
            }
            else
            {
                TempData[nameof(Messages.ErrorMessage)] = result.Error ?? "Có lỗi xảy ra khi xóa xe.";
            }

            return RedirectToAction(nameof(Index));
        }

        // POST: Cars/DeleteMultiple
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteMultiple([FromForm] int[] ids, CancellationToken cancellationToken = default)
        {
            if (ids == null || ids.Length == 0)
            {
                TempData["ErrorMessage"] = "Vui lòng chọn ít nhất một xe để xóa.";
                return RedirectToAction(nameof(Index));
            }

            var handler = await _mediator.Send(new DeleteMultipleCarCommand { Ids = ids }, cancellationToken);

            if(handler.Success)
            {
                TempData[nameof(Messages.SuccessMessage)] = $"Xóa {ids.Length} xe thành công!";
            }
            else
            {
                TempData[nameof(Messages.ErrorMessage)] = handler.Error ?? "Có lỗi xảy ra khi xóa các xe.";
            }
            return RedirectToAction(nameof(Index));
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteMedia([FromBody] DeleteMediaRequest request, CancellationToken cancellationToken = default)
        {
            if (request == null || request.Id <= 0) return BadRequest();

            var handler = await _mediator.Send(new DeleteCarMediaCommand { MediaId = request.Id }, cancellationToken);

            if (handler.Success)
            {
                return Json(new { success = true,  request.Id});
            }
            else
            {
                return Json(new { success = false, error = handler.Error });
            }
        }

        // POST: Cars/BulkImport
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> BulkImport(IFormFile file, CancellationToken cancellationToken = default)
        {
            if (file == null || file.Length == 0)
            {
                return Json(new { success = false, message = "Vui lòng chọn file Excel cần import." });
            }

            if (!file.FileName.EndsWith(".xlsx", StringComparison.OrdinalIgnoreCase))
            {
                return Json(new { success = false, message = "Hệ thống chỉ hỗ trợ upload định dạng .xlsx" });
            }

            var handler = await _mediator.Send(new BulkImportCarsCommand { File = file }, cancellationToken);
            if (handler.Success)
            {
                return Json(new
                {
                    success = true,
                    addedCount = handler.Value.AddedCount,
                    updatedCount = handler.Value.UpdatedCount,
                    skippedRows = handler.Value.SkippedRows,
                    duplicatedVins = handler.Value.DuplicatedVins,
                    duplicateVinRows = handler.Value.DuplicatedVinRows
                });
            }
            else
            {
                return Json(new { success = false, message = handler.Error ?? "Có lỗi xảy ra trong quá trình import dữ liệu." });
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
                    TempData[nameof(Messages.ErrorMessage)] = "Không tìm thấy file mẫu trên máy chủ (wwwroot/template/DATA XE-TEMPLATE.xlsx).";
                    return RedirectToAction(nameof(Index));
                }

                var fileBytes = System.IO.File.ReadAllBytes(filePath);
                string excelName = "Template_Import_Xe.xlsx";
                return File(fileBytes, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", excelName);
            }
            catch (Exception ex)
            {
                TempData[nameof(Messages.ErrorMessage)] = "Có lỗi xảy ra trong quá trình tải file mẫu.";
                return RedirectToAction(nameof(Index));
            }
        }
    }

    public class DeleteMediaRequest
    {
        public int Id { get; set; }
    }
}

