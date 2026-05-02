using ChargePoint.CarManagement.Application.Interfaces.Common;
using ChargePoint.CarManagement.Domain.ViewModels.TireViewModels;
using Mediator;
using Microsoft.EntityFrameworkCore;

namespace ChargePoint.CarManagement.Application.Tire.Queries
{
    public class GetTireCarDetailEditViewQuery : IRequest<TireEditVM>
    {
        public int TireId { get; set; }

    }

    public class GetTireCarDetailEditViewQueryHandler(IUnitOfWork unitOfWork) : IRequestHandler<GetTireCarDetailEditViewQuery, TireEditVM>
    {
        private readonly IUnitOfWork _unitOfWork = unitOfWork;
        public async ValueTask<TireEditVM?> Handle(GetTireCarDetailEditViewQuery query, CancellationToken cancellationToken)
        {
            var tireRecord = await _unitOfWork.TireRecords.GetByIdAsync(query.TireId, includeBuilder: builder => builder.Include(t => t.Car), cancellationToken: cancellationToken);
            if (tireRecord == null || tireRecord.Car == null) { return null; }
            return new TireEditVM { Car = tireRecord.Car, Record = tireRecord };
        }
    }

}
