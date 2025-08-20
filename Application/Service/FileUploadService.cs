using Application.IService;
using Microsoft.AspNetCore.Http;

namespace Application.Services;



public class FileUploadService : IFileUploadService
{
    public async Task<string> UploadFileAsync(IFormFile file, Guid userId)
    {
        // TODO: Save file and return URL/path
        return await Task.FromResult("uploaded/path/" + file.FileName);
    }
}
