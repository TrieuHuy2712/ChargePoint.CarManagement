using ChargePoint.CarManagement.Application.Interfaces;
using ChargePoint.CarManagement.Domain.Entities;
using ChargePoint.CarManagement.Domain.ViewModels.TrafficViolationViewModels;
using Mediator;

namespace ChargePoint.CarManagement.Application.TrafficViolation.Queries
{
    public class GetTrafficViolationCheckQuery : IRequest<TrafficViolationCheckVM>
    {
        public int? SoLuongViPham { get; set; }
        public string? NoiDungViPham { get; set; }
        public string? DiaDiemViPham { get; set; }
        public string? NgayGioViPham { get; set; }
        public string? GhiChu { get; set; }
        public Domain.Entities.Car Car { get; set; }
    }

    public class GetTrafficViolationCheckQueryHandler(IAuthenService authenService) : IRequestHandler<GetTrafficViolationCheckQuery, TrafficViolationCheckVM>
    {
        private readonly IAuthenService authenService = authenService;
        public ValueTask<TrafficViolationCheckVM> Handle(GetTrafficViolationCheckQuery query, CancellationToken cancellationToken = default)
        {
            var model = new TrafficViolationCheck
            {
                CarId = query.Car.Id,
                NgayKiemTra = DateTime.Now,
                NguoiTao = authenService.GetCurrentUserName(),
            };

            // Pre-fill từ kết quả tra cứu (nếu có)
            if (query.SoLuongViPham.HasValue)
            {
                model.SoLuongViPham = query.SoLuongViPham.Value;
                model.CoViPham = query.SoLuongViPham.Value > 0;
                model.NoiDungViPham = query.NoiDungViPham;
                model.DiaDiemViPham = query.DiaDiemViPham;
                model.GhiChu = query.GhiChu;

                if (DateTime.TryParse(query.NgayGioViPham, out var parsedDate))
                {
                    model.NgayGioViPham = parsedDate;
                }
            }

            return new ValueTask<TrafficViolationCheckVM>(new TrafficViolationCheckVM
            {
                Car = query.Car,
                TrafficViolationCheck = model
            });
        }
    }
}
