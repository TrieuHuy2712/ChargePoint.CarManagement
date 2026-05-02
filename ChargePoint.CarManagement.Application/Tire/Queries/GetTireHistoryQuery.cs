using ChargePoint.CarManagement.Application.Interfaces.Common;
using ChargePoint.CarManagement.Domain.Entities;
using ChargePoint.CarManagement.Domain.ViewModels.TireViewModels;
using Mediator;

namespace ChargePoint.CarManagement.Application.Tire.Queries
{
    public class GetTireHistoryQuery : IRequest<TireHistoryVM>
    {
        public int? CarId { get; set; }
        public ViTriLop? ViTri { get; set; }
    }

    public class GetTireHistoryQueryHandler(IUnitOfWork unitOfWork) : IRequestHandler<GetTireHistoryQuery, TireHistoryVM>
    {
        private readonly IUnitOfWork _unitOfWork = unitOfWork;

        public async ValueTask<TireHistoryVM?> Handle(GetTireHistoryQuery request, CancellationToken cancellationToken = default)
        {
            if (request.CarId == null) return null;
            var car = await _unitOfWork.Cars.GetByIdAsync(request.CarId.Value, cancellationToken);
            if (car == null) return null;
            var tireHistoryRecord = new List<TireRecord>();

            var records = await _unitOfWork.TireRecords.FindAsync(t => t.CarId == request.CarId.Value && (request.ViTri == null || t.ViTriLop == request.ViTri), cancellationToken: cancellationToken);
            tireHistoryRecord = records.Count != 0
                ? [.. records.OrderByDescending(t => t.NgayThucHien)]
                : records;

            return new TireHistoryVM
            {
                Car = car,
                ViTriLop = request.ViTri,
                Records = tireHistoryRecord
            };
        }
    }
}
