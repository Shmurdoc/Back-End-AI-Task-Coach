namespace Application.DTOs.APIDtos;

public class ApiResponseDto<T>
{
    public bool Success { get; set; }
    public T? Data { get; set; }
    public string? Message { get; set; }
    public string? Error { get; set; }
    public DateTime Timestamp { get; set; }
    public string? CorrelationId { get; set; }
    public object? Metadata { get; set; }
}
