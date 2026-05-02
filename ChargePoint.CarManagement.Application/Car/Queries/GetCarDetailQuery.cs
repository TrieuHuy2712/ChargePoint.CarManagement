using ChargePoint.CarManagement.Application.Interfaces.Common;
using ChargePoint.CarManagement.Domain.ViewModels;
using Mediator;
using Microsoft.EntityFrameworkCore;

namespace ChargePoint.CarManagement.Application.Car.Queries
{
    public class GetCarDetailQuery : IRequest<CarViewModel>
    {
        public int CarId { get; set; }
    }

    public class GetCarDetailQueryHandler(IUnitOfWork unitOfWork) : IRequestHandler<GetCarDetailQuery, CarViewModel>
    {
        private readonly IUnitOfWork _unitOfWork = unitOfWork;
        public async ValueTask<CarViewModel?> Handle(GetCarDetailQuery query, CancellationToken cancellationToken)
        {
            var car = await _unitOfWork.Cars.GetByIdAsync(query.CarId, includeBuilder: q => q.Include(c => c.Media), cancellationToken);
            if (car == null) {
                return null;
            }

            return CarViewModel.FromCar(car);
        }
    }
}
