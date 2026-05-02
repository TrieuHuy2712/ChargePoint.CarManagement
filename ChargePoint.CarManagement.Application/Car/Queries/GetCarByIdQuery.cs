using ChargePoint.CarManagement.Application.Interfaces;
using ChargePoint.CarManagement.Application.Interfaces.Common;
using Mediator;

namespace ChargePoint.CarManagement.Application.Car.Queries
{
    public class GetCarByIdQuery : IRequest<Domain.Entities.Car>
    {
        public int CarId { get; set; }
    }
    public class GetCarByIdQueryHandler(IUnitOfWork unitOfWork) : IRequestHandler<GetCarByIdQuery, Domain.Entities.Car>
    {
        private readonly IUnitOfWork _unitOfWork = unitOfWork;
        public async ValueTask<Domain.Entities.Car?> Handle(GetCarByIdQuery query, CancellationToken cancellationToken)
        {
            var car = await _unitOfWork.Cars.GetByIdAsync(query.CarId, cancellationToken);
            return car;
        }
    }
}
