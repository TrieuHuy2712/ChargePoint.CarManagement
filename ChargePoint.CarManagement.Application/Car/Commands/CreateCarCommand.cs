using ChargePoint.CarManagement.Application.Interfaces;
using ChargePoint.CarManagement.Application.Interfaces.Common;
using ChargePoint.CarManagement.Domain.Entities;
using ChargePoint.CarManagement.Domain.Enums;
using ChargePoint.CarManagement.Domain.Models;
using ChargePoint.CarManagement.Models;
using Mediator;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;

namespace ChargePoint.CarManagement.Application.Car.Commands
{
    public class CreateCarCommand : IRequest<Result>
    {
        public Domain.Entities.Car Model { get; set; }
        public IFormFile? PrimaryImageFile { get; set; }
    }

    public class CreateCarCommandHandler(
        IUnitOfWork unitOfWork,
        IImageUploadService imageUploadService,
        IHttpContextAccessor httpContextAccessor,
        ILogger<CreateCarCommandHandler> logger) : IRequestHandler<CreateCarCommand, Result>
    {
        private readonly IUnitOfWork _unitOfWork = unitOfWork;
        private readonly IImageUploadService _imageUploadService = imageUploadService;
        private readonly IHttpContextAccessor _httpContextAccessor = httpContextAccessor;
        private readonly ILogger<CreateCarCommandHandler> _logger = logger;

        public async ValueTask<Result> Handle(CreateCarCommand cmd, CancellationToken cancellationToken)
        {
            try
            {
                var bienSo = cmd.Model.BienSo ?? "NoPlate";

                // Prepare media list
                var mediaList = new List<CarMedia>();

                // 1) PRIMARY IMAGE: if provided, upload and mark as primary.
                if (cmd.PrimaryImageFile != null && cmd.PrimaryImageFile.Length > 0)
                {
                    var primaryUrl = await _imageUploadService.UploadFileAsync(cmd.PrimaryImageFile, bienSo, "Primary");
                    var primaryMedia = new CarMedia
                    {
                        Type = MediaType.Image_Primary,
                        Url = primaryUrl,
                        FileName = cmd.PrimaryImageFile.FileName,
                        IsPrimary = true
                    };
                    mediaList.Add(primaryMedia);
                    cmd.Model.PrimaryImageUrl = primaryUrl;
                }

                // 2) Upload categorized images
                var httpContext = _httpContextAccessor.HttpContext;
                if (httpContext != null && httpContext.Request.HasFormContentType)
                {
                    var imageFiles = httpContext.Request.Form.Files.Where(f => f.Name.StartsWith("ImageFiles[")).ToList();
                    if (imageFiles.Count != 0)
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
                                        mediaList.Add(new CarMedia
                                        {
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

                cmd.Model.Media = mediaList;
                await _unitOfWork.Cars.AddAsync(cmd.Model, cancellationToken);
                await _unitOfWork.SaveChangesAsync(cancellationToken);
                return Result.Ok();
            }
            catch (Exception ex)
            {

                _logger.LogError(ex, "Error creating car with license plate {BienSo}", cmd.Model.BienSo);
                return Result.Fail("An error occurred while creating the car. Please try again.");
            }
            
        }
    }
}
