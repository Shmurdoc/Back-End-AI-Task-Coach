using Application.DTOs.AuthDtos;
using Application.IRepositories;
using Application.IService;
using Application.Services;
using Domain.Entities;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.ComponentModel.DataAnnotations;
using System.Security.Claims;

namespace WebAPI.Controllers;

/// <summary>
/// Authentication controller for user registration, login, and token management
/// </summary>
[ApiController]
[Route("api/[controller]")]
[Produces("application/json")]
public class AuthController : ControllerBase
{
    private readonly IUserRepository _userRepository;
    private readonly ITokenService _tokenService;
    private readonly ILogger<AuthController> _logger;

    public AuthController(
        IUserRepository userRepository,
        ITokenService tokenService,
        ILogger<AuthController> logger)
    {
        _userRepository = userRepository;
        _tokenService = tokenService;
        _logger = logger;
    }

    /// <summary>
    /// Register a new user account
    /// </summary>
    /// <param name="request">User registration details</param>
    /// <returns>Registration result with user information and token</returns>
    [HttpPost("register")]
    public async Task<IActionResult> Register([FromBody] RegisterRequest request)
    {
        try
        {
            _logger.LogInformation("Registration attempt for email: {Email}", request.Email);

            // Validate request
            if (string.IsNullOrWhiteSpace(request.Email) || 
                string.IsNullOrWhiteSpace(request.Password) || 
                string.IsNullOrWhiteSpace(request.Name))
            {
                return BadRequest(new AuthResponse
                {
                    Success = false,
                    Message = "Email, password, and name are required"
                });
            }

            if (request.Password.Length < 6)
            {
                return BadRequest(new AuthResponse
                {
                    Success = false,
                    Message = "Password must be at least 6 characters long"
                });
            }

            // Check if user already exists
            var existingUser = await _userRepository.GetByEmailAsync(request.Email);
            if (existingUser != null)
            {
                return BadRequest(new AuthResponse
                {
                    Success = false,
                    Message = "User with this email already exists"
                });
            }

            // Create new user
            var user = new User
            {
                Email = request.Email.ToLower().Trim(),
                Name = request.Name.Trim(),
                // PhoneNumber = request.PhoneNumber?.Trim(), // TODO: Add back when database supports it
                PasswordHash = TokenService.HashPassword(request.Password),
                IsActive = true
            };

            var createdUser = await _userRepository.AddAsync(user);

            // Generate token
            var token = _tokenService.GenerateToken(createdUser.Id, createdUser.Email, createdUser.Name);

            _logger.LogInformation("User registered successfully: {UserId} - {Email}", createdUser.Id, createdUser.Email);

            return Ok(new AuthResponse
            {
                Success = true,
                Message = "Registration successful",
                Token = token,
                User = new UserDto
                {
                    Id = createdUser.Id,
                    Email = createdUser.Email,
                    Name = createdUser.Name,
                    // PhoneNumber = createdUser.PhoneNumber, // TODO: Add back when database supports it
                    IsActive = createdUser.IsActive
                }
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error during registration for email: {Email}", request.Email);
            return StatusCode(500, new AuthResponse
            {
                Success = false,
                Message = "An error occurred during registration. Please try again."
            });
        }
    }

    /// <summary>
    /// User login
    /// </summary>
    /// <param name="request">Login credentials</param>
    /// <returns>Login result with user information and token</returns>
    [HttpPost("login")]
    public async Task<IActionResult> Login([FromBody] LoginRequest request)
    {
        try
        {
            _logger.LogInformation("Login attempt for email: {Email}", request.Email);

            // Validate request
            if (string.IsNullOrWhiteSpace(request.Email) || string.IsNullOrWhiteSpace(request.Password))
            {
                return BadRequest(new AuthResponse
                {
                    Success = false,
                    Message = "Email and password are required"
                });
            }

            // Get user by email
            var user = await _userRepository.GetByEmailAsync(request.Email);
            if (user == null)
            {
                _logger.LogWarning("Login failed - user not found: {Email}", request.Email);
                return Unauthorized(new AuthResponse
                {
                    Success = false,
                    Message = "Invalid email or password"
                });
            }

            // Check if user is active
            if (!user.IsActive)
            {
                _logger.LogWarning("Login failed - inactive user: {Email}", request.Email);
                return Unauthorized(new AuthResponse
                {
                    Success = false,
                    Message = "User account is inactive"
                });
            }

            // Verify password
            if (!TokenService.VerifyPassword(request.Password, user.PasswordHash))
            {
                _logger.LogWarning("Login failed - invalid password: {Email}", request.Email);
                return Unauthorized(new AuthResponse
                {
                    Success = false,
                    Message = "Invalid email or password"
                });
            }

            // Generate token
            var token = _tokenService.GenerateToken(user.Id, user.Email, user.Name);

            _logger.LogInformation("User logged in successfully: {UserId} - {Email}", user.Id, user.Email);

            return Ok(new AuthResponse
            {
                Success = true,
                Message = "Login successful",
                Token = token,
                User = new UserDto
                {
                    Id = user.Id,
                    Email = user.Email,
                    Name = user.Name,
                    // PhoneNumber = user.PhoneNumber, // TODO: Add back when database supports it
                    IsActive = user.IsActive,
                    CreatedAt = user.CreatedAt
                }
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error during login for email: {Email}", request.Email);
            return StatusCode(500, new AuthResponse
            {
                Success = false,
                Message = "An error occurred during login. Please try again."
            });
        }
    }

    /// <summary>
    /// Refresh authentication token
    /// </summary>
    /// <param name="request">Refresh token request</param>
    /// <returns>New authentication token</returns>
    [HttpPost("refresh")]
    public async Task<IActionResult> RefreshToken([FromBody] RefreshTokenRequest request)
    {
        try
        {
            _logger.LogInformation("Token refresh attempt");

            if (string.IsNullOrWhiteSpace(request.RefreshToken))
            {
                return BadRequest(new AuthResponse
                {
                    Success = false,
                    Message = "Refresh token is required"
                });
            }

            var newToken = await _tokenService.RefreshTokenAsync(request.RefreshToken);

            _logger.LogInformation("Token refreshed successfully");

            return Ok(new AuthResponse
            {
                Success = true,
                Message = "Token refreshed successfully",
                Token = newToken
            });
        }
        catch (UnauthorizedAccessException ex)
        {
            _logger.LogWarning(ex, "Token refresh failed - unauthorized");
            return Unauthorized(new AuthResponse
            {
                Success = false,
                Message = "Invalid or expired refresh token"
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error during token refresh");
            return StatusCode(500, new AuthResponse
            {
                Success = false,
                Message = "An error occurred during token refresh. Please try again."
            });
        }
    }
}
