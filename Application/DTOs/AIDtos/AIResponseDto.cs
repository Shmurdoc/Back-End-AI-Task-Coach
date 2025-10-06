namespace Application.DTOs.AIDtos;

public record AIResponseDto(
    string Suggestion,
    string PatternAnalysis,
    DateTime GeneratedAt
);
