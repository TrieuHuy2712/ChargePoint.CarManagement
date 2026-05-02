using ChargePoint.CarManagement.Application.Interfaces;
using ChargePoint.CarManagement.Application.Interfaces.Common;
using ChargePoint.CarManagement.Domain.ViewModels.TrafficViolationViewModels;
using Mediator;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace ChargePoint.CarManagement.Application.TrafficViolation.Queries
{
    public class GetTrafficViolationByIdQuery : IRequest<TrafficViolationCheckVM>
    {
        public int Id { get; set; }
    }

    public class GetTrafficViolationByIdQueryHandler(
        IUnitOfWork unitOfWork, 
        ILogger<GetTrafficViolationByIdQueryHandler> logger) : IRequestHandler<GetTrafficViolationByIdQuery, TrafficViolationCheckVM>
    {
        private readonly IUnitOfWork _unitOfWork = unitOfWork;
        private readonly ILogger<GetTrafficViolationByIdQueryHandler> logger = logger;
        public async ValueTask<TrafficViolationCheckVM?> Handle(GetTrafficViolationByIdQuery query, CancellationToken cancellationToken)
        {
            var check = await _unitOfWork.TrafficViolations.GetByIdAsync(
                query.Id,
                includeBuilder: q => q.Include(t => t.Car),
                cancellationToken: cancellationToken);
            if (check == null)
            {
                logger.LogWarning("No traffic violation found for car with ID {Id}.", query.Id);
                return null; // Or return an appropriate response indicating not found
            }
            var result = new TrafficViolationCheckVM
            {
                Car = check?.Car,
                TrafficViolationCheck = check
            };
            return result;
        }
    }
}
