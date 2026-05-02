using ChargePoint.CarManagement.Application.Common.Extensions;
using ChargePoint.CarManagement.Application.Interfaces.Common;
using ChargePoint.CarManagement.Domain.Enums;
using ChargePoint.CarManagement.Domain.ViewModels;
using ChargePoint.CarManagement.Domain.ViewModels.TrafficViolationViewModels;
using Mediator;
using Microsoft.EntityFrameworkCore;

namespace ChargePoint.CarManagement.Application.TrafficViolation.Queries
{
    public class GetTrafficViolationIndexQuery : IRequest<PagedResult<TrafficViolationIndexVM>>
    {
        public string Q { get; set; }
        public ViolationStatus? Status { get; set; }
        public int Page { get; set; }
        public int PageSize { get; set; } = 10;
    }

    public class GetTrafficViolationIndexQueryHandler(
       IUnitOfWork unitOfWork) : IRequestHandler<GetTrafficViolationIndexQuery, PagedResult<TrafficViolationIndexVM>>
    {
        private readonly IUnitOfWork _unitOfWork = unitOfWork;
        public async ValueTask<PagedResult<TrafficViolationIndexVM>> Handle(GetTrafficViolationIndexQuery getTrafficViolationIndexQuery, CancellationToken cancellationToken = default)
        {
            var carQuery = await _unitOfWork.Cars.AsQueryable();

            // Apply search filter using reusable extension method
            carQuery = carQuery.ApplySearch(getTrafficViolationIndexQuery.Q);

            var totalCount = await carQuery.CountAsync(cancellationToken);
            var totalPages = (int)Math.Ceiling(totalCount / (double)getTrafficViolationIndexQuery.PageSize);
            if (getTrafficViolationIndexQuery.Page > totalPages && totalPages > 0) getTrafficViolationIndexQuery.Page = totalPages;

            // Apply ordering and pagination using reusable extension methods
            var pagedCars = await carQuery
                .ApplyDefaultOrdering()
                .ApplyPagination(getTrafficViolationIndexQuery.Page, getTrafficViolationIndexQuery.PageSize)
                .ToListAsync(cancellationToken);

            var checkQuery = await _unitOfWork.TrafficViolations.FindAsync(t => pagedCars.Select(c => c.Id).Contains(t.CarId) 
            && (getTrafficViolationIndexQuery.Status == null || t.TrangThaiXuLy == getTrafficViolationIndexQuery.Status), 
            cancellationToken: cancellationToken);

            // Tạo danh sách kết quả với thông tin xe và lần kiểm tra vi phạm gần nhất
            var items = pagedCars.Select(c => new TrafficViolationIndexVM
            {
                Car = c,
                LastCheck = checkQuery.Where(v => v.CarId == c.Id)
                                        .OrderByDescending(v => v.NgayKiemTra)
                                        .FirstOrDefault()
            }).ToList();

            var viewModel = new PagedResult<TrafficViolationIndexVM>
            {
                Items = items,
                PageNumber = getTrafficViolationIndexQuery.Page,
                PageSize = getTrafficViolationIndexQuery.PageSize,
                TotalCount = totalCount,
                SearchQuery = getTrafficViolationIndexQuery.Q
            };
            return viewModel;
        }
    }
}
