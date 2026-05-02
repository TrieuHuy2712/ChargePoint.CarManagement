using ChargePoint.CarManagement.Application.Interfaces;
using Microsoft.AspNetCore.Http;

namespace ChargePoint.CarManagement.Infrastructure.Services
{
    public class AuthenService : IAuthenService
    {
        private readonly IHttpContextAccessor _httpContextAccessor;
        public AuthenService(IHttpContextAccessor httpContextAccessor)
        {
            _httpContextAccessor = httpContextAccessor;
        }

        public string GetCurrentUserName()
        {
            var userName = _httpContextAccessor.HttpContext?.User?.Identity?.Name;
            return userName ?? string.Empty;
        }

        public bool IsInRole(string role)
        {
            return _httpContextAccessor.HttpContext?.User?.IsInRole(role) ?? false;
        }
    }
}
