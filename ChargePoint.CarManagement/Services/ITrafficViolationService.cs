using ChargePoint.CarManagement.Models;

namespace ChargePoint.CarManagement.Services
{
    public interface ITrafficViolationService
    {
        Task<TrafficViolationResult> CheckViolationAsync(string bienSo);
    }
}
