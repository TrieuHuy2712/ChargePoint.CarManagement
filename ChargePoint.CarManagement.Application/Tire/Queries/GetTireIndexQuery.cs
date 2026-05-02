using ChargePoint.CarManagement.Application.Common.Extensions;
using ChargePoint.CarManagement.Application.Interfaces.Common;
using ChargePoint.CarManagement.Domain.ViewModels;
using ChargePoint.CarManagement.Domain.ViewModels.TireViewModels;
using Mediator;
using Microsoft.EntityFrameworkCore;

namespace ChargePoint.CarManagement.Application.Tire.Queries
{
    public class GetTireIndexQuery : IRequest<PagedResult<TireIndexViewModel>>
    {
        public string? Q { get; set; }
        public int Page { get; set; } = 1;
        public int PageSize { get; set; } = 10;
    }

    public class GetTireIndexQueryHandler(IUnitOfWork unitOfWork) : IRequestHandler<GetTireIndexQuery, PagedResult<TireIndexViewModel>>
    {
        private readonly IUnitOfWork _unitOfWork = unitOfWork;

        public async ValueTask<PagedResult<TireIndexViewModel>> Handle(GetTireIndexQuery getTireIndexQuery, CancellationToken cancellationToken = default)
        {
            if (getTireIndexQuery.Page < 1) getTireIndexQuery.Page = 1;
            if (getTireIndexQuery.PageSize < 1) getTireIndexQuery.PageSize = 10;

            var carQuery = await _unitOfWork.Cars.AsQueryable();

            // Apply search filter using reusable extension method
            carQuery = carQuery.ApplySearch(getTireIndexQuery.Q);

            var totalCount = await carQuery.CountAsync(cancellationToken);
            var totalPages = (int)Math.Ceiling(totalCount / (double)getTireIndexQuery.PageSize);
            if (getTireIndexQuery.Page > totalPages && totalPages > 0) getTireIndexQuery.Page = totalPages;

            // Apply ordering and pagination using reusable extension methods
            var pagedCars = await carQuery
                .ApplyDefaultOrdering()
                .ApplyPagination(getTireIndexQuery.Page, getTireIndexQuery.PageSize)
                .ToListAsync(cancellationToken);

            var carIds = pagedCars.Select(c => c.Id).ToList();

            // Load tire records only for paged cars
            var tireRecords = await _unitOfWork.TireRecords.FindAsync(t => carIds.Contains(t.CarId), cancellationToken: cancellationToken);

            // Compose ViewModel
            var items = pagedCars.Select(car => new TireIndexViewModel
            {
                Car = car,
                TireRecords = [.. tireRecords
                   .Where(t => t.CarId == car.Id)
                   .GroupBy(t => t.ViTriLop)
                   .Select(g => new TireRecordDetailIndexVM
                   {
                       ViTri = g.Key,
                       LastRecord = g.OrderByDescending(t => t.NgayThucHien).FirstOrDefault()
                   })],
                TotalRecords = tireRecords.Count(t => t.CarId == car.Id)
            }).ToList();

            return new PagedResult<TireIndexViewModel>
            {
                Items = items,
                PageNumber = getTireIndexQuery.Page,
                PageSize = getTireIndexQuery.PageSize,
                TotalCount = totalCount,
                SearchQuery = getTireIndexQuery.Q ?? string.Empty
            };
        }
    }
}
