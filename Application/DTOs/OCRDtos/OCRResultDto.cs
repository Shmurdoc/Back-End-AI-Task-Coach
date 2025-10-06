namespace Application.DTOs.OCRDtos;

public record OCRResultDto(
    string ExtractedText,
    string[] ParsedTasks,
    DateTime ProcessedAt
);
