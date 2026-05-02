using ChargePoint.CarManagement.Application.Interfaces;
using ChargePoint.CarManagement.Application.Interfaces.Common;
using ChargePoint.CarManagement.Domain.Models;
using Mediator;
using Microsoft.Extensions.Logging;

namespace ChargePoint.CarManagement.Application.Car.Commands
{
    public class DeleteCarCommand : IRequest<Result>
    {
        public int CarId { get; set; }
    }

    public class DeleteCarCommandHandler(
        IUnitOfWork unitOfWork,
        IImageUploadService imageUploadService,
        ILogger<DeleteCarCommandHandler> logger
        ) : IRequestHandler<DeleteCarCommand, Result>
    {
        private readonly IUnitOfWork _unitOfWork = unitOfWork;
        private readonly IImageUploadService _imageUploadService = imageUploadService;
        private readonly ILogger<DeleteCarCommandHandler> _logger = logger;
        public async ValueTask<Result> Handle(DeleteCarCommand cmd, CancellationToken cancellationToken)
        {
            try
            {
                var car = await _unitOfWork.Cars.GetByIdAsync(cmd.CarId, cancellationToken);
                if (car == null)
                {
                    return Result.Fail("Car not found.");
                }
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
                
                _unitOfWork.Cars.Remove(car);
                await _unitOfWork.SaveChangesAsync(cancellationToken);
                return Result.Ok();
            }
            catch (Exception ex)
            {
                // Log the exception if you have a logging mechanism
                _logger.LogError(ex, "An error occurred while deleting the car with ID {CarId}", cmd.CarId);
                return Result.Fail($"An error occurred while deleting the car: {ex.Message}");
            }
        }
    }
}
