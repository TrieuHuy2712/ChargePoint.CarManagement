using ChargePoint.CarManagement.Application.Interfaces;
using ChargePoint.CarManagement.Application.Interfaces.Common;
using ChargePoint.CarManagement.Domain.Models;
using ChargePoint.CarManagement.Domain.ViewModels;
using Mediator;
using Microsoft.Extensions.Logging;

namespace ChargePoint.CarManagement.Application.Car.Commands
{
    public class DeleteMultipleCarCommand : IRequest<Result>
    {
        public int[] Ids { get; set; }
    }

    public class DeleteMultipleCarCommandHandler(
        IImageUploadService imageUploadService,
        IUnitOfWork unitOfWork,
        ILogger<DeleteMultipleCarCommandHandler> logger) : IRequestHandler<DeleteMultipleCarCommand, Result>
    {
        private readonly IImageUploadService _imageUploadService = imageUploadService;
        private readonly IUnitOfWork _unitOfWork = unitOfWork;
        private readonly ILogger<DeleteMultipleCarCommandHandler> _logger = logger;

        public async ValueTask<Result> Handle(DeleteMultipleCarCommand command, CancellationToken cancellationToken)
        {
            var ids = command.Ids;
            var cars = await _unitOfWork.Cars.FindAsync(t => ids.Contains(t.Id), cancellationToken: cancellationToken);

            var vms = cars.Select(CarViewModel.FromCar).ToList();
            if (cars.Count == 0)
            {
                return Result.Fail("Some cars not found");
            }

            try
            {
                foreach (var car in cars)
                {
                    if (car.Media != null)
                    {
                        foreach (var m in car.Media)
                        {
                            try
                            {
                                if (!string.IsNullOrEmpty(m.Url))
                                {
                                    await _imageUploadService.DeleteFileAsync(m.Url);
                                }
                            }
                            catch (Exception ex)
                            {
                                _logger.LogWarning(ex, "Failed to delete media {MediaId} for car {CarId}", m.Id, car.Id);
                            }
                        }
                    }
                }
                _unitOfWork.Cars.RemoveRange(cars);
                await _unitOfWork.SaveChangesAsync(cancellationToken);
                return Result.Ok();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to delete multiple cars");
                return Result.Fail("Failed to delete multiple cars");
            }
        }
    }
}
