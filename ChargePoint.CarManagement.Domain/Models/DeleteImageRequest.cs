namespace ChargePoint.CarManagement.Domain.Models
{
    public class DeleteImageRequest
    {
        public int RecordId { get; set; }
        public string ImageUrl { get; set; } = string.Empty;
        public string ImageType { get; set; } = "ChungTu"; // "ChungTu" or "DOT"
    }
}
