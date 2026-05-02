using ChargePoint.CarManagement.Application.SystemSetting.Commands;
using ChargePoint.CarManagement.Application.SystemSetting.Queries;
using ChargePoint.CarManagement.Domain.Constants;
using Mediator;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace ChargePoint.CarManagement.Controllers
{
    [Authorize]
    public class SettingsController(
        IMediator mediator) : Controller
    {
        private readonly IMediator _mediator = mediator;

        public async Task<IActionResult> Index(CancellationToken cancellationToken = default)
        {
            var vm = await _mediator.Send(new GetSystemSettingIndexQuery(), cancellationToken);
            return View(vm);
        }

        public async Task<IActionResult> Save(IFormCollection form, CancellationToken cancellationToken = default)
        {
            var handleResult = await _mediator.Send(new SaveSystemSettingCommand { Form = form }, cancellationToken);
            if (handleResult.Success)
            {
                TempData[nameof(Messages.SuccessMessage)] = "Lưu thiết lập thành công!";
            }
            else
            {
                TempData[nameof(Messages.ErrorMessage)] = handleResult.Error;
            }
            return RedirectToAction(nameof(Index));
        }
    }
}
