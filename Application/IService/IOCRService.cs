namespace Application.IService;

public interface IOCRService
{
    // Add method signatures as needed, e.g.:
    // Task<string> ExtractTextAsync(Stream imageStream);

    // Controller-compatible stub
    Task<string> ExtractTextAsync(byte[] imageData, CancellationToken cancellationToken = default);
}
