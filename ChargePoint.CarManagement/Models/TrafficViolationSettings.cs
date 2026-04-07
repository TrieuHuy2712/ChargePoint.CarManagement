namespace ChargePoint.CarManagement.Models
{
    public class TrafficViolationSettings
    {
        /// <summary>
        /// Provider tra cứu phạt nguội: "CheckPhatNguoiVn" hoặc "PhatNguoiApp"
        /// </summary>
        public string Provider { get; set; } = "PhatNguoiApp";
    }
}
