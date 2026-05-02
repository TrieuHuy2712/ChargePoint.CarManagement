using ChargePoint.CarManagement.Application.Interfaces;
using ChargePoint.CarManagement.Application.Interfaces.Common;
using Mediator;

namespace ChargePoint.CarManagement.Application.Car.Queries
{
    public class GetAllCarQuery : IRequest<List<Domain.Entities.Car>>
    {
    }
    public class GetAllCarQueryHandler(IUnitOfWork unitOfWork) : IRequestHandler<GetAllCarQuery, List<Domain.Entities.Car>>
    {
        private readonly IUnitOfWork _unitOfWork = unitOfWork;

        public async ValueTask<List<Domain.Entities.Car>> Handle(GetAllCarQuery query, CancellationToken cancellationToken)
        {
            var cars = await _unitOfWork.Cars.GetAllAsync(cancellationToken);
            return cars;
        }
    }
}
