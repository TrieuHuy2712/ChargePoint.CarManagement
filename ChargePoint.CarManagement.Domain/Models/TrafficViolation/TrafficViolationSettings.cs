namespace ChargePoint.CarManagement.Domain.Models.TrafficViolation
{
    public class TrafficViolationSettings
    {
        /// <summary>
        /// Provider tra cứu phạt nguội: "CheckPhatNguoiVn" hoặc "PhatNguoiApp"
        /// </summary>
        public string Provider { get; set; } = "PhatNguoiApp";
    }
}
