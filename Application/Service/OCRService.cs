using Microsoft.AspNetCore.Http;

namespace Application.Services;

public interface IOCRService
{
    Task<string> ExtractTextAsync(IFormFile file);
}

public class OCRService : IOCRService
{
    public async Task<string> ExtractTextAsync(IFormFile file)
    {
        // TODO: Integrate with Tesseract or Azure OCR
        return await Task.FromResult("Extracted text from " + file.FileName);
    }
}
