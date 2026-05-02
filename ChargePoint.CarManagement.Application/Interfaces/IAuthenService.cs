namespace ChargePoint.CarManagement.Application.Interfaces
{
    public interface IAuthenService
    {
        public string GetCurrentUserName();

        public bool IsInRole(string role);
    }
}
