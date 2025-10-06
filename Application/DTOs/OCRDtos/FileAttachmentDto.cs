namespace Application.DTOs.OCRDtos;

public record FileAttachmentDto(
    string FileName,
    string Url,
    Guid UploadedBy,
    DateTime UploadedAt
);
