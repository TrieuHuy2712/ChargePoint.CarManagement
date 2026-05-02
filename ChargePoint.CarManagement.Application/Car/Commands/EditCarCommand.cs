using ChargePoint.CarManagement.Application.Interfaces;
using ChargePoint.CarManagement.Application.Interfaces.Common;
using ChargePoint.CarManagement.Domain.Entities;
using ChargePoint.CarManagement.Domain.Enums;
using ChargePoint.CarManagement.Domain.Models;
using ChargePoint.CarManagement.Domain.ViewModels;
using ChargePoint.CarManagement.Models;
using Mediator;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace ChargePoint.CarManagement.Application.Car.Commands
{
    public class EditCarCommand : IRequest<Result>
    {
        public CarViewModel Model { get; set; }

        public IFormFile? PrimaryImageFile { get; set; }
    }

    public class EditCarCommandHandler(
        IUnitOfWork unitOfWork,
        IImageUploadService imageUploadService,
        ILogger<EditCarCommandHandler> logger,
        IHttpContextAccessor httpContextAccessor) : IRequestHandler<EditCarCommand, Result>
    {
        private readonly IUnitOfWork _unitOfWork = unitOfWork;
        private readonly IImageUploadService _imageUploadService = imageUploadService;
        private readonly ILogger<EditCarCommandHandler> _logger = logger;
        private readonly IHttpContextAccessor _httpContextAccessor = httpContextAccessor;

        public async ValueTask<Result> Handle(EditCarCommand command, CancellationToken cancellationToken)
        {
            var bienSo = command.Model.BienSo ?? "NoPlate";
            var existingCar = await _unitOfWork.Cars.GetByIdAsync(command.Model.Id, cancellationToken);
            if (existingCar == null)
            {
                return Result.Fail($"Car with ID {command.Model.Id} not found.");
            }
            // Update the existing car's properties with the new values
            existingCar.Stt = command.Model.Stt;
            existingCar.TenXe = command.Model.TenXe;
            existingCar.SoLuong = command.Model.SoLuong;
            existingCar.MauXe = command.Model.MauXe;
            existingCar.SoVIN = command.Model.SoVIN;
            existingCar.BienSo = command.Model.BienSo;
            existingCar.BienSoCu = command.Model.BienSoCu;
            existingCar.MauBienSo = command.Model.MauBienSo;
            existingCar.TenKhachHang = command.Model.TenKhachHang;
            existingCar.ThongTinChoThue = command.Model.ThongTinChoThue;
            existingCar.NgayThue = command.Model.NgayThue;
            existingCar.NgayHetHan = command.Model.NgayHetHan;
            existingCar.OdoXe = command.Model.OdoXe;

            existingCar.Media ??= new List<CarMedia>();

            try
            {
                // If a new primary image file was provided, upload it and make it primary.
                if (command.PrimaryImageFile != null && command.PrimaryImageFile.Length > 0)
                {
                    var primaryUrl = await _imageUploadService.UploadFileAsync(command.PrimaryImageFile, bienSo, "Primary");
                    var primaryMedia = new CarMedia
                    {
                        CarId = existingCar.Id,
                        Type = MediaType.Image_Primary,
                        Url = primaryUrl,
                        FileName = command.PrimaryImageFile.FileName,
                        IsPrimary = true
                    };

                    // clear any existing primary flags
                    foreach (var m in existingCar.Media)
                    {
                        m.IsPrimary = false;
                    }

                    existingCar.Media.Add(primaryMedia);
                    existingCar.PrimaryImageUrl = primaryUrl;
                }

                // Handle new categorized image uploads (append)
                var httpContext = _httpContextAccessor.HttpContext;
                if (httpContext != null && httpContext.Request.HasFormContentType)
                {
                    var imageFiles = httpContext.Request.Form.Files.Where(f => f.Name.StartsWith("ImageFiles[")).ToList();
                    if (imageFiles.Any())
                    {
                        foreach (var file in imageFiles)
                        {
                            if (file != null && file.Length > 0)
                            {
                                var name = file.Name;
                                var startIdx = name.IndexOf('[') + 1;
                                var endIdx = name.IndexOf(']');

                                if (startIdx > 0 && endIdx > startIdx)
                                {
                                    var typeStr = name.Substring(startIdx, endIdx - startIdx);
                                    if (Enum.TryParse<MediaType>(typeStr, out var mediaType))
                                    {
                                        var typeDisplayName = mediaType.GetDisplayName();
                                        var url = await _imageUploadService.UploadFileAsync(file, bienSo, typeDisplayName);
                                        existingCar.Media.Add(new CarMedia
                                        {
                                            CarId = existingCar.Id,
                                            Type = mediaType,
                                            Url = url,
                                            FileName = file.FileName
                                        });
                                    }
                                }
                            }
                        }
                    }
                }

                // If no PrimaryImageFile was uploaded, respect vm.PrimaryImageUrl (selection from existing images)
                if (command.PrimaryImageFile == null || command.PrimaryImageFile.Length == 0)
                {
                    if (!string.IsNullOrEmpty(command.Model.PrimaryImageUrl))
                    {
                        if (existingCar.Media != null)
                        {
                            foreach (var m in existingCar.Media)
                            {
                                m.IsPrimary = string.Equals(m.Url, command.Model.PrimaryImageUrl, StringComparison.OrdinalIgnoreCase);
                            }
                        }
                        existingCar.PrimaryImageUrl = command.Model.PrimaryImageUrl;
                    }
                }

                _unitOfWork.Cars.Update(existingCar);
                await _unitOfWork.SaveChangesAsync(cancellationToken);
                return Result.Ok();
            }
            catch (DbUpdateConcurrencyException)
            {

                if (await _unitOfWork.Cars.AnyAsync(c => c.Id == existingCar.Id, cancellationToken))
                {
                    return Result.Fail("The car was updated by another user. Please reload and try again.");
                }
                else
                {
                    return Result.Fail("The car no longer exists.");
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating car with ID {CarId}", existingCar.Id);
                return Result.Fail("An error occurred while updating the car. Please try again.");
            }
        }
    }
}
