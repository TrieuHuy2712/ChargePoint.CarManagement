using ChargePoint.CarManagement.Application.Interfaces.Common;
using ChargePoint.CarManagement.Domain.Models;
using Mediator;
using Microsoft.Extensions.Logging;

namespace ChargePoint.CarManagement.Application.TrafficViolation.Commands
{
    public class DeleteTrafficViolationCommand : IRequest<Result<int?>>
    {
        public int Id { get; set; }
    }

    public class DeleteTrafficViolationCommandHandler(
        IUnitOfWork unitOfWork,
        ILogger<DeleteTrafficViolationCommandHandler> logger) : IRequestHandler<DeleteTrafficViolationCommand, Result<int?>>
    {
        private readonly IUnitOfWork _unitOfWork = unitOfWork;
        private readonly ILogger<DeleteTrafficViolationCommandHandler> logger = logger;
        public async ValueTask<Result<int?>> Handle(DeleteTrafficViolationCommand command, CancellationToken cancellationToken)
        {
            try
            {
                var trafficViolation = await _unitOfWork.TrafficViolations.GetByIdAsync(command.Id, cancellationToken);
                if (trafficViolation == null)
                {
                    return Result<int?>.Fail("Traffic violation not found.");
                }

                _unitOfWork.TrafficViolations.Remove(trafficViolation);
                await _unitOfWork.SaveChangesAsync(cancellationToken);
                return Result<int?>.Ok(trafficViolation.CarId);
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "An error occurred while deleting the traffic violation with ID {Id}.", command.Id);
                return Result<int?>.Fail($"An error occurred while deleting the traffic violation. {ex.Message}");
            }
        }
    }
}
