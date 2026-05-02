using ChargePoint.CarManagement.Application.BulkTrafficViolation.Commands;
using ChargePoint.CarManagement.Application.BulkTrafficViolation.Queries;
using ChargePoint.CarManagement.Domain.Models.TrafficViolation;
using ChargePoint.CarManagement.Domain.ViewModels.TrafficViolationViewModels;
using Mediator;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace ChargePoint.CarManagement.Controllers
{
    [Authorize]
    public class BulkTrafficViolationController : Controller
    {
        private readonly IMediator _mediator;

        public BulkTrafficViolationController(IMediator mediator)
        {
            _mediator = mediator;
        }

        // GET: BulkTrafficViolation/Index
        public async Task<IActionResult> Index(string q, int page = 1, int pageSize = 50, CancellationToken cancellationToken = default)
        {
            if (page < 1) page = 1;
            if (pageSize < 1) pageSize = 50;

            var handleQuery = await _mediator.Send(new GetBulkTrafficViolationIndexQuery
            {
                Q = q,
                Page = page,
                PageSize = pageSize
            }, cancellationToken);

            return View(handleQuery);
        }

        // POST: BulkTrafficViolation/CheckOnline
        [HttpPost]
        public async Task<IActionResult> CheckOnline([FromBody] List<CarCheckRequest> requests, CancellationToken cancellationToken = default)
        {
            var handleCommand = await _mediator.Send(new CheckBulkTrafficViolationOnlineCommand
            {
                CarCheckRequests = requests
            }, cancellationToken);

            if (handleCommand.Success)
            {
                return Json(new { success = true, data = handleCommand.Value });
            }
            else
            {
                return Json(new { success = false, message = handleCommand.Error });
            }
        }

        // POST: BulkTrafficViolation/Save
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Save([FromBody] List<TrafficViolationBulkCheckVM> results, CancellationToken cancellationToken =default)

        {
            var handleCommand = await _mediator.Send(new SaveBulkTrafficViolationCommand
            {
                CheckVMs = results
            }, cancellationToken);
            if (handleCommand.Success) {
                return Json(new { success = true , message = handleCommand.Value.Message });
            }
            else
            {
                return Json(new { success = false, message = handleCommand.Error });
            }
        }

        // POST: BulkTrafficViolation/ExportResults
        [HttpPost]
        public async Task<IActionResult> ExportResults([FromBody] List<TrafficViolationBulkCheckVM> results, CancellationToken cancellationToken = default)
        {
            var result = await _mediator.Send(new ExportBulkTrafficViolationCommand
            {
                CheckVMs = results
            }, cancellationToken);
            return File(result.Value.FileContents, result.Value.ContentType, result.Value.FileDownloadName);
        }

    }
}
