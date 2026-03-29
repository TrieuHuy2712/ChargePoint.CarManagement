using ChargePoint.CarManagement.Models;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Text.RegularExpressions;

namespace ChargePoint.CarManagement.Services
{
    public class TrafficViolationService : ITrafficViolationService
    {
        private readonly HttpClient _httpClient;
        private readonly ILogger<TrafficViolationService> _logger;

        public TrafficViolationService(HttpClient httpClient, ILogger<TrafficViolationService> logger)
        {
            _httpClient = httpClient;
            _logger = logger;
        }

        public async Task<TrafficViolationResult> CheckViolationAsync(string bienSo)
        {
            try
            {
                // Chuẩn hóa biển số: bỏ dấu gạch, khoảng trắng
                var bienSoFormatted = Regex.Replace(bienSo, @"[-.\s]", "").ToUpper();

                // Gọi API tra cứu bằng POST với payload
                var url = "https://api.checkphatnguoi.vn/phatnguoi";

                // Tạo request payload
                var requestPayload = new { bienso = bienSoFormatted };
                var jsonContent = new StringContent(
                    JsonSerializer.Serialize(requestPayload),
                    Encoding.UTF8,
                    "application/json"
                );

                var response = await _httpClient.PostAsync(url, jsonContent);

                if (response.IsSuccessStatusCode)
                {
                    var content = await response.Content.ReadAsStringAsync();
                    _logger.LogInformation("API Response: {Content}", content);

                    var apiResponse = JsonSerializer.Deserialize<ApiResponse>(content, new JsonSerializerOptions
                    {
                        PropertyNameCaseInsensitive = true
                    });

                    if (apiResponse != null)
                    {
                        // status = 0: không có vi phạm, status = 1: có vi phạm
                        var hasViolations = apiResponse.Status == 1 && apiResponse.Data != null && apiResponse.Data.Count > 0;

                        return new TrafficViolationResult
                        {
                            Success = true,
                            BienSo = bienSo,
                            CoViPham = hasViolations,
                            SoLuongViPham = apiResponse.DataInfo?.Total ?? 0,
                            DanhSachViPham = apiResponse.Data?.Select(v => new ViolationDetail
                            {
                                MaViPham = v.BienKiemSoat,
                                NgayViPham = v.ThoiGianViPham,
                                DiaDiem = v.DiaDiemViPham,
                                HanhVi = v.HanhViViPham,
                                TrangThai = v.TrangThai,
                                SoTienPhat = 0, // API không trả về số tiền cụ thể
                                DonViPhatHien = v.DonViPhatHienViPham,
                                NoiGiaiQuyet = v.NoiGiaiQuyetVuViec != null
                                    ? string.Join("\n", v.NoiGiaiQuyetVuViec)
                                    : null,
                                LoaiPhuongTien = v.LoaiPhuongTien,
                                MauBien = v.MauBien
                            }).ToList() ?? [],
                            Message = hasViolations
                                ? $"Tìm thấy {apiResponse.DataInfo?.Total ?? 0} vi phạm ({apiResponse.DataInfo?.ChuaXuPhat ?? 0} chưa xử phạt)"
                                : "Không có vi phạm"
                        };
                    }
                }

                // Fallback nếu API chính không hoạt động
                return await CheckViolationFallbackAsync(bienSoFormatted);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Lỗi khi tra cứu phạt nguội cho biển số: {BienSo}", bienSo);
                return new TrafficViolationResult
                {
                    Success = false,
                    BienSo = bienSo,
                    Message = "Không thể tra cứu. Vui lòng thử lại sau hoặc tra cứu thủ công tại csgt.vn"
                };
            }
        }

        private async Task<TrafficViolationResult> CheckViolationFallbackAsync(string bienSo)
        {
            // Nếu API chính fail, trả về thông báo yêu cầu tra cứu thủ công
            return await Task.FromResult(new TrafficViolationResult
            {
                Success = false,
                BienSo = bienSo,
                Message = "Không thể tra cứu tự động. Vui lòng tra cứu thủ công tại csgt.vn"
            });
        }

        #region Response Models

        private class ApiResponse
        {
            [JsonPropertyName("status")]
            public int Status { get; set; }

            [JsonPropertyName("msg")]
            public string? Msg { get; set; }

            [JsonPropertyName("data")]
            public List<ViolationData>? Data { get; set; }

            [JsonPropertyName("data_info")]
            public DataInfo? DataInfo { get; set; }
        }

        private class ViolationData
        {
            [JsonPropertyName("Biển kiểm soát")]
            public string? BienKiemSoat { get; set; }

            [JsonPropertyName("Màu biển")]
            public string? MauBien { get; set; }

            [JsonPropertyName("Loại phương tiện")]
            public string? LoaiPhuongTien { get; set; }

            [JsonPropertyName("Thời gian vi phạm")]
            public string? ThoiGianViPham { get; set; }

            [JsonPropertyName("Địa điểm vi phạm")]
            public string? DiaDiemViPham { get; set; }

            [JsonPropertyName("Hành vi vi phạm")]
            public string? HanhViViPham { get; set; }

            [JsonPropertyName("Trạng thái")]
            public string? TrangThai { get; set; }

            [JsonPropertyName("Đơn vị phát hiện vi phạm")]
            public string? DonViPhatHienViPham { get; set; }

            [JsonPropertyName("Nơi giải quyết vụ việc")]
            public List<string>? NoiGiaiQuyetVuViec { get; set; }
        }

        private class DataInfo
        {
            [JsonPropertyName("total")]
            public int Total { get; set; }

            [JsonPropertyName("chuaxuphat")]
            public int ChuaXuPhat { get; set; }

            [JsonPropertyName("daxuphat")]
            public int DaXuPhat { get; set; }

            [JsonPropertyName("latest")]
            public string? Latest { get; set; }
        }

        #endregion
    }
}
