using ChargePoint.CarManagement.Application.Interfaces;
using ChargePoint.CarManagement.Domain.Entities;
using ChargePoint.CarManagement.Domain.Enums;
using ChargePoint.CarManagement.Domain.Models.TrafficViolation;
using ChargePoint.CarManagement.Infrastructure.Persistence.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Text.RegularExpressions;

namespace ChargePoint.CarManagement.Services
{
    public class TrafficViolationService(
        HttpClient httpClient,
        ILogger<TrafficViolationService> logger,
        IOptions<TrafficViolationSettings> settings) : ITrafficViolationService
    {
        private readonly HttpClient _httpClient = httpClient;
        private readonly ILogger<TrafficViolationService> _logger = logger;
        private readonly TrafficViolationSettings _settings = settings.Value;

        public async Task<TrafficViolationResult> CheckViolationAsync(string bienSo)
        {
            var bienSoFormatted = Regex.Replace(bienSo, @"[-.\s]", "").ToUpper();

            _logger.LogInformation("Tra cứu phạt nguội biển số {BienSo} bằng provider: {Provider}",
                bienSoFormatted, _settings.Provider);

            try
            {
                return _settings.Provider switch
                {
                    "CheckPhatNguoiVn" => await CheckViaCheckPhatNguoiVnAsync(bienSo, bienSoFormatted),
                    "PhatNguoiApp" => await CheckViaPhatNguoiAppAsync(bienSo, bienSoFormatted),
                    _ => await CheckViaPhatNguoiAppAsync(bienSo, bienSoFormatted)
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Lỗi khi tra cứu phạt nguội cho biển số: {BienSo} (Provider: {Provider})",
                    bienSo, _settings.Provider);
                return new TrafficViolationResult
                {
                    Success = false,
                    BienSo = bienSo,
                    Message = "Không thể tra cứu. Vui lòng thử lại sau hoặc tra cứu thủ công tại csgt.vn"
                };
            }
        }

        #region Provider: PhatNguoiApp

        private async Task<TrafficViolationResult> CheckViaPhatNguoiAppAsync(string bienSoGoc, string bienSoFormatted)
        {
            // Bước 1: GET trang chủ để lấy nonce (WordPress AJAX security)
            var pageRequest = new HttpRequestMessage(HttpMethod.Get, "https://phatnguoi.app/");
            pageRequest.Headers.Add("Accept", "text/html");
            var pageResponse = await _httpClient.SendAsync(pageRequest);

            if (!pageResponse.IsSuccessStatusCode)
            {
                _logger.LogWarning("PhatNguoiApp: Không thể tải trang chủ, status {StatusCode}", pageResponse.StatusCode);
                return new TrafficViolationResult
                {
                    Success = false,
                    BienSo = bienSoGoc,
                    Message = "Không thể kết nối đến phatnguoi.app. Vui lòng tra cứu thủ công tại csgt.vn"
                };
            }

            var pageContent = await pageResponse.Content.ReadAsStringAsync();
            var nonceMatch = Regex.Match(pageContent, @"nonce[""'\s:]+[""']([a-f0-9]+)[""']");
            if (!nonceMatch.Success)
            {
                _logger.LogWarning("PhatNguoiApp: Không tìm thấy nonce trong trang chủ");
                return new TrafficViolationResult
                {
                    Success = false,
                    BienSo = bienSoGoc,
                    Message = "Không thể xác thực với phatnguoi.app. Vui lòng tra cứu thủ công tại csgt.vn"
                };
            }

            var nonce = nonceMatch.Groups[1].Value;

            // Bước 2: POST tra cứu qua WordPress AJAX
            var formData = new FormUrlEncodedContent(new Dictionary<string, string>
            {
                { "action", "phatnguoi_search" },
                { "nonce", nonce },
                { "license_plate", bienSoFormatted },
                { "vehicle_type", "1" }
            });

            var searchRequest = new HttpRequestMessage(HttpMethod.Post, "https://phatnguoi.app/wp-admin/admin-ajax.php")
            {
                Content = formData
            };
            searchRequest.Headers.Add("Referer", "https://phatnguoi.app/");

            // Copy cookies từ page response
            if (pageResponse.Headers.TryGetValues("Set-Cookie", out var setCookies))
            {
                searchRequest.Headers.Add("Cookie", string.Join("; ", setCookies.Select(c => c.Split(';')[0])));
            }

            var response = await _httpClient.SendAsync(searchRequest);

            if (!response.IsSuccessStatusCode)
            {
                _logger.LogWarning("PhatNguoiApp API trả về status {StatusCode} cho biển số {BienSo}",
                    response.StatusCode, bienSoFormatted);
                return new TrafficViolationResult
                {
                    Success = false,
                    BienSo = bienSoGoc,
                    Message = $"API trả về lỗi ({response.StatusCode}). Vui lòng tra cứu thủ công tại csgt.vn"
                };
            }

            var content = await response.Content.ReadAsStringAsync();
            _logger.LogInformation("PhatNguoiApp Response: {Content}", content);

            var apiResponse = JsonSerializer.Deserialize<PhatNguoiAppResponse>(content, new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            });

            if (apiResponse?.Success != true || apiResponse.Data == null)
            {
                return new TrafficViolationResult
                {
                    Success = false,
                    BienSo = bienSoGoc,
                    Message = "Không thể đọc dữ liệu từ phatnguoi.app. Vui lòng tra cứu thủ công tại csgt.vn"
                };
            }

            var data = apiResponse.Data;
            var violations = data.Violations ?? [];
            var hasViolations = violations.Count > 0;

            return new TrafficViolationResult
            {
                Success = true,
                BienSo = bienSoGoc,
                CoViPham = hasViolations,
                SoLuongViPham = data.TotalViolations,
                DanhSachViPham = violations.Select(v => new ViolationDetail
                {
                    MaViPham = v.Plate ?? data.Plate,
                    NgayViPham = v.Time,
                    DiaDiem = v.Location,
                    HanhVi = v.Title,
                    TrangThai = v.StatusText ?? (v.Status == "paid" ? "Đã xử phạt" : "Chưa xử phạt"),
                    SoTienPhat = 0,
                    DonViPhatHien = v.Unit,
                    NoiGiaiQuyet = v.ResolutionLocation != null
                        ? string.Join("\n", v.ResolutionLocation)
                        : null,
                    LoaiPhuongTien = v.VehicleType,
                    MauBien = v.PlateColor
                }).ToList(),
                Message = hasViolations
                    ? $"Tìm thấy {data.TotalViolations} vi phạm ({data.UnpaidCount} chưa xử phạt)"
                    : "Không có vi phạm"
            };
        }

        #endregion

        #region Provider: CheckPhatNguoiVn (legacy)

        private async Task<TrafficViolationResult> CheckViaCheckPhatNguoiVnAsync(string bienSoGoc, string bienSoFormatted)
        {
            var url = "https://api.checkphatnguoi.vn/phatnguoi";

            var requestPayload = new { bienso = bienSoFormatted };
            var jsonContent = new StringContent(
                JsonSerializer.Serialize(requestPayload),
                Encoding.UTF8,
                "application/json"
            );

            var response = await _httpClient.PostAsync(url, jsonContent);

            if (!response.IsSuccessStatusCode)
            {
                _logger.LogWarning("CheckPhatNguoiVn API trả về status {StatusCode}, chuyển sang fallback",
                    response.StatusCode);

                return new TrafficViolationResult
                {
                    Success = false,
                    BienSo = bienSoGoc,
                    Message = "API checkphatnguoi.vn không khả dụng. Vui lòng tra cứu thủ công tại csgt.vn"
                };
            }

            var content = await response.Content.ReadAsStringAsync();
            _logger.LogInformation("CheckPhatNguoiVn Response: {Content}", content);

            var apiResponse = JsonSerializer.Deserialize<CheckPhatNguoiVnResponse>(content, new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            });

            if (apiResponse == null)
            {
                return new TrafficViolationResult
                {
                    Success = false,
                    BienSo = bienSoGoc,
                    Message = "Không thể đọc dữ liệu từ API. Vui lòng tra cứu thủ công tại csgt.vn"
                };
            }

            var hasViolations = apiResponse.Status == 1 && apiResponse.Data != null && apiResponse.Data.Count > 0;

            return new TrafficViolationResult
            {
                Success = true,
                BienSo = bienSoGoc,
                CoViPham = hasViolations,
                SoLuongViPham = apiResponse.DataInfo?.Total ?? 0,
                DanhSachViPham = apiResponse.Data?.Select(v => new ViolationDetail
                {
                    MaViPham = v.BienKiemSoat,
                    NgayViPham = v.ThoiGianViPham,
                    DiaDiem = v.DiaDiemViPham,
                    HanhVi = v.HanhViViPham,
                    TrangThai = v.TrangThai,
                    SoTienPhat = 0,
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



        #endregion

        #region Response Models — PhatNguoiApp

        private class PhatNguoiAppResponse
        {
            [JsonPropertyName("success")]
            public bool Success { get; set; }

            [JsonPropertyName("data")]
            public PhatNguoiAppData? Data { get; set; }
        }

        private class PhatNguoiAppData
        {
            [JsonPropertyName("plate")]
            public string? Plate { get; set; }

            [JsonPropertyName("plate_formatted")]
            public string? PlateFormatted { get; set; }

            [JsonPropertyName("total_violations")]
            public int TotalViolations { get; set; }

            [JsonPropertyName("unpaid_count")]
            public int UnpaidCount { get; set; }

            [JsonPropertyName("paid_count")]
            public int PaidCount { get; set; }

            [JsonPropertyName("last_updated")]
            public string? LastUpdated { get; set; }

            [JsonPropertyName("violations")]
            public List<PhatNguoiAppViolation>? Violations { get; set; }
        }

        private class PhatNguoiAppViolation
        {
            [JsonPropertyName("id")]
            public int? Id { get; set; }

            [JsonPropertyName("plate")]
            public string? Plate { get; set; }

            [JsonPropertyName("plate_color")]
            public string? PlateColor { get; set; }

            [JsonPropertyName("vehicle_type")]
            public string? VehicleType { get; set; }

            [JsonPropertyName("status")]
            public string? Status { get; set; }

            [JsonPropertyName("status_text")]
            public string? StatusText { get; set; }

            [JsonPropertyName("status_color")]
            public string? StatusColor { get; set; }

            [JsonPropertyName("icon")]
            public string? Icon { get; set; }

            [JsonPropertyName("color")]
            public string? Color { get; set; }

            [JsonPropertyName("title")]
            public string? Title { get; set; }

            [JsonPropertyName("time")]
            public string? Time { get; set; }

            [JsonPropertyName("location")]
            public string? Location { get; set; }

            [JsonPropertyName("unit")]
            public string? Unit { get; set; }

            [JsonPropertyName("muc_phat")]
            public string? MucPhat { get; set; }

            [JsonPropertyName("resolution_location")]
            public List<string>? ResolutionLocation { get; set; }

            [JsonPropertyName("phone")]
            public string? Phone { get; set; }
        }

        #endregion

        #region Response Models — CheckPhatNguoiVn (legacy)

        private class CheckPhatNguoiVnResponse
        {
            [JsonPropertyName("status")]
            public int Status { get; set; }

            [JsonPropertyName("msg")]
            public string? Msg { get; set; }

            [JsonPropertyName("data")]
            public List<CheckPhatNguoiVnViolation>? Data { get; set; }

            [JsonPropertyName("data_info")]
            public CheckPhatNguoiVnDataInfo? DataInfo { get; set; }
        }

        private class CheckPhatNguoiVnViolation
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

        private class CheckPhatNguoiVnDataInfo
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
