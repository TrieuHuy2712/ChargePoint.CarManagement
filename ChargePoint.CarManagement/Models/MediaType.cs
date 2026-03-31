using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ChargePoint.CarManagement.Models
{
    public enum MediaType
    {
        Image_Primary = -1,
        Image_GSM = 0,
        Image_KH = 1,
        Video_GSM = 2,
        Video_KH = 3
    }

    public class CarMedia
    {
        public int Id { get; set; }

        [Required]
        public int CarId { get; set; }

        [Required]
        public MediaType Type { get; set; }

        [Required]
        [StringLength(1000)]
        public string Url { get; set; } = string.Empty;

        [Display(Name = "Tên file")]
        [StringLength(260)]
        public string? FileName { get; set; }

        [Display(Name = "Ảnh chính")]
        public bool IsPrimary { get; set; } = false;

        [Display(Name = "Ngày tạo")]
        public DateTime CreatedAt { get; set; } = DateTime.Now;

        [ForeignKey(nameof(CarId))]
        public Car? Car { get; set; }
    }
}
