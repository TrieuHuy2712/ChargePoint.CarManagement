using ChargePoint.CarManagement.Application.Interfaces;
using ChargePoint.CarManagement.Application.Common.Extensions;
using ChargePoint.CarManagement.Application.Interfaces.Common;
using ChargePoint.CarManagement.Domain.ViewModels;
using ChargePoint.CarManagement.Domain.ViewModels.MaintenanceViewModels;
using Mediator;
using Microsoft.EntityFrameworkCore;


namespace ChargePoint.CarManagement.Application.Maintenance.Queries
{
    public class GetMaintenanceIndexQuery : IRequest<PagedResult<MaintenanceIndexViewModel>>
    {
        public string Q { get; set; }
        public int Page { get; set; } = 1;
        public int PageSize { get; set; } = 10;
    }

    public class GetMaintenanceIndexQueryHandler(IUnitOfWork unitOfWork) : IRequestHandler<GetMaintenanceIndexQuery, PagedResult<MaintenanceIndexViewModel>>
    {
        private readonly IUnitOfWork _unitOfWork = unitOfWork;
        public async ValueTask<PagedResult<MaintenanceIndexViewModel>> Handle(GetMaintenanceIndexQuery getMaintenanceIndexQuery, CancellationToken cancellationToken = default)
        {
            var carQuery = await _unitOfWork.Cars.AsQueryable();

            // Apply search filter using reusable extension method
            carQuery = carQuery.ApplySearch(getMaintenanceIndexQuery.Q);

            var totalCount = await carQuery.CountAsync(cancellationToken);
            var totalPages = (int)Math.Ceiling(totalCount / (double)getMaintenanceIndexQuery.PageSize);
            if (getMaintenanceIndexQuery.Page > totalPages && totalPages > 0) 
                getMaintenanceIndexQuery.Page = totalPages;

            // Apply ordering and pagination using reusable extension methods
            var pagedCars = await carQuery
                .ApplyDefaultOrdering()
                .ApplyPagination(getMaintenanceIndexQuery.Page, getMaintenanceIndexQuery.PageSize)
                .ToListAsync(cancellationToken);

            var carIds = pagedCars.Select(c => c.Id).ToList();

            // load maintenance records only for current page cars
            var maintenanceRecords = await _unitOfWork.MaintenanceRecords.FindAsync(
                m => carIds.Contains(m.CarId), 
                cancellationToken: cancellationToken);

            var items = pagedCars.Select(car => new MaintenanceIndexViewModel
            {
                Car = car,
                LastMaintenance = maintenanceRecords
                    .Where(m => m.CarId == car.Id)
                    .OrderByDescending(m => m.NgayBaoDuong)
                    .FirstOrDefault(),
                TotalMaintenances = maintenanceRecords.Count(m => m.CarId == car.Id)
            }).ToList();

            var viewModel = new PagedResult<MaintenanceIndexViewModel>
            {
                Items = items,
                PageNumber = getMaintenanceIndexQuery.Page,
                PageSize = getMaintenanceIndexQuery.PageSize,
                TotalCount = totalCount,
                SearchQuery = getMaintenanceIndexQuery.Q ?? string.Empty
            };

            return viewModel;
        }
    }
}
