using Application.DTOs.AuthDtos;

namespace Application.DTOs.Login;

public record LoginResponseDto(
    string Token,
    DateTime ExpiresAt,
    UserDto User
);
