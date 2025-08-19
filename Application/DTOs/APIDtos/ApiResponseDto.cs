namespace Application.DTOs.APIDtos;

public record ApiResponseDto<T>(
    bool Success,
    T? Data,
    string? Message,
    string? Error,
    DateTime Timestamp,
    string? CorrelationId,
    object? Metadata
);
