using ChargePoint.CarManagement.Domain.Models.TrafficViolation;

namespace ChargePoint.CarManagement.Application.Interfaces
{
    public interface ITrafficViolationService
    {

        Task<TrafficViolationResult> CheckViolationAsync(string bienSo);
    }
}
