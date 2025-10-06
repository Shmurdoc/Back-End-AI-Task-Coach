namespace Application.DTOs.APIDtos;

public class ApiKeyResponseDto
{
    public string ApiKey { get; set; } = string.Empty;

    public string ServiceName { get; set; } = string.Empty;

    public List<string> Permissions { get; set; } = new();

    public DateTime ExpiresAt { get; set; }

    public DateTime CreatedAt { get; set; }
}
