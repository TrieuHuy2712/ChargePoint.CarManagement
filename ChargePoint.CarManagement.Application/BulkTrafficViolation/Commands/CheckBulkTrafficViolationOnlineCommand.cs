using ChargePoint.CarManagement.Application.Interfaces;
using ChargePoint.CarManagement.Application.Interfaces.Common;
using ChargePoint.CarManagement.Domain.Models;
using ChargePoint.CarManagement.Domain.Models.TrafficViolation;
using Mediator;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace ChargePoint.CarManagement.Application.BulkTrafficViolation.Commands
{
    public class CheckBulkTrafficViolationOnlineCommand : IRequest<Result<List<CarCheckResult>>>
    {
        public List<CarCheckRequest> CarCheckRequests { get; set; }
    }

    public class CheckBulkTrafficViolationOnlineCommandHandler(
        ITrafficViolationService trafficViolationService,
        IUnitOfWork unitOfWork,
        ILogger<CheckBulkTrafficViolationOnlineCommandHandler> logger) : IRequestHandler<CheckBulkTrafficViolationOnlineCommand, Result<List<CarCheckResult>>>
    {
        private readonly ITrafficViolationService _trafficViolationService = trafficViolationService;
        private readonly IUnitOfWork _unitOfWork = unitOfWork;
        private readonly ILogger<CheckBulkTrafficViolationOnlineCommandHandler> _logger = logger;
        public async ValueTask<Result<List<CarCheckResult>>> Handle(CheckBulkTrafficViolationOnlineCommand cmd, CancellationToken cancellationToken)
        {
            try
            {
                if (cmd.CarCheckRequests == null || !cmd.CarCheckRequests.Any())
                {
                    return Result<List<CarCheckResult>>.Fail("No car check requests provided.");
                }

                var results = new List<CarCheckResult>();
                var carIds = cmd.CarCheckRequests.Select(r => r.CarId).ToList();
                var carsQuery = await _unitOfWork.Cars.AsQueryable();
                var cars = await carsQuery.Where(c => carIds.Contains(c.Id)).ToListAsync(cancellationToken);

                foreach (var req in cmd.CarCheckRequests)
                {
                    var car = cars.FirstOrDefault(c => c.Id == req.CarId);
                    var plate = req.BienSo ?? car?.BienSo;

                    if (string.IsNullOrEmpty(plate))
                    {
                        results.Add(new CarCheckResult
                        {
                            CarId = req.CarId,
                            BienSo = plate,
                            Success = false,
                            Message = "Không có biển số"
                        });
                        continue;
                    }

                    try
                    {
                        var result = await _trafficViolationService.CheckViolationAsync(plate!);
                        results.Add(new CarCheckResult
                        {
                            CarId = req.CarId,
                            BienSo = plate,
                            TenXe = car?.TenXe,
                            Success = result.Success,
                            CoViPham = result.CoViPham,
                            SoLuongViPham = result.SoLuongViPham,
                            DanhSachViPham = result.DanhSachViPham,
                            Message = result.Message
                        });
                    }
                    catch (Exception ex)
                    {
                        results.Add(new CarCheckResult
                        {
                            CarId = req.CarId,
                            BienSo = plate,
                            Success = false,
                            Message = $"Lỗi: {ex.Message}"
                        });
                    }

                    // Delay để tránh quá tải API
                    await Task.Delay(500, cancellationToken);
                }
                return Result<List<CarCheckResult>>.Ok(results);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error checking bulk traffic violations");
                return Result<List<CarCheckResult>>.Fail($"Error checking traffic violations: {ex.Message}");
            }
        }

    }
}
