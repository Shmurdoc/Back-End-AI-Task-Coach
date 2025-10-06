using Microsoft.AspNetCore.Http;
using Application.IService;

namespace Infrastructure.Services.ExternalIntegrations;

public class OCRService : IOCRService
{
    public async Task<string> ExtractTextAsync(byte[] imageData, CancellationToken cancellationToken = default)
    {
        // TODO: Integrate with Tesseract or Azure OCR
        return await Task.FromResult($"Extracted text from {imageData.Length} bytes of image data");
    }
}
