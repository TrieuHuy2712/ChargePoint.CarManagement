using ChargePoint.CarManagement.Application.Interfaces;
using ChargePoint.CarManagement.Application.Interfaces.Common;
using ChargePoint.CarManagement.Domain.Models;
using Mediator;
using Microsoft.Extensions.Logging;

namespace ChargePoint.CarManagement.Application.Car.Commands
{
    public class DeleteCarMediaCommand : IRequest<Result>
    {
        public int MediaId { get; set; }
    }

    public class DeleteCarMediaCommandHandler(
        IImageUploadService imageUploadService,
        IUnitOfWork unitOfWork,
        ILogger<DeleteCarMediaCommandHandler> logger) : IRequestHandler<DeleteCarMediaCommand, Result>
    {
        private readonly IImageUploadService _imageUploadService = imageUploadService;
        private readonly IUnitOfWork _unitOfWork = unitOfWork;
        private readonly ILogger<DeleteCarMediaCommandHandler> _logger = logger;
        public async ValueTask<Result> Handle(DeleteCarMediaCommand command, CancellationToken cancellationToken)
        {
            var carMedia = await _unitOfWork.CarMedia.GetByIdAsync(command.MediaId, cancellationToken);
            if (carMedia == null)
            {
                return Result.Fail($"Media with ID {command.MediaId} not found.");
            }

            try
            {
                if (!string.IsNullOrEmpty(carMedia.Url))
                    await _imageUploadService.DeleteFileAsync(carMedia.Url);

                var car = carMedia.Car;
                var deletedUrl = carMedia.Url;
                _unitOfWork.CarMedia.Remove(carMedia);

                // If deleted item was primary, attempt to pick another image as primary
                if (car != null && car.PrimaryImageUrl == deletedUrl)
                {
                    var replacement = await _unitOfWork.CarMedia.FirstOrDefaultAsync(cm => cm.CarId == car.Id && cm.Id != carMedia.Id, cancellationToken);
                    if (replacement != null)
                    {
                        replacement.IsPrimary = true;
                        car.PrimaryImageUrl = replacement.Url;
                        _unitOfWork.CarMedia.Update(replacement);
                        _unitOfWork.Cars.Update(car);
                    }
                    else
                    {
                        car.PrimaryImageUrl = null;
                        _unitOfWork.Cars.Update(car);
                    }
                }
                await _unitOfWork.SaveChangesAsync(cancellationToken);
                return Result.Ok();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting media with ID {MediaId}", command.MediaId);
                return Result.Fail($"Failed to delete media with ID {command.MediaId}: {ex.Message}");
            }
        }
    }
}
