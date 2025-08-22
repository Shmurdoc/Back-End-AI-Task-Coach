using System;

namespace Application.DTOs.APIDtos;

/// <summary>
/// Standard API response wrapper for consistent API output.
/// </summary>
/// <typeparam name="T">Type of the data payload.</typeparam>
public record ApiResponseDto<T>(
    bool Success,
    T? Data,
    string? Message,
    string? Error,
    DateTime Timestamp,
    string? CorrelationId,
    object? Metadata
);

// This DTO is designed to be used in WebAPI controllers that are configured
// via appsettings.json (including connection strings, JWT, OpenAI, etc.)
// and is compatible with professional, clean .NET WebAPI project structure.
