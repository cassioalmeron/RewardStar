using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using RewardStar.Api.DTOs;
using RewardStar.Api.Services;
using RewardStart.Core;
using RewardStart.Core.Models;
using RewardStart.Core.Utils;

namespace RewardStar.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class AuthController : ControllerBase
{
    private readonly RewardStartDbContext _dbContext;
    private readonly JwtService _jwtService;
    private readonly GoogleAuthService _googleAuthService;
    private readonly ILogger<AuthController> _logger;

    public AuthController(
        RewardStartDbContext dbContext,
        JwtService jwtService,
        GoogleAuthService googleAuthService,
        ILogger<AuthController> logger)
    {
        _dbContext = dbContext;
        _jwtService = jwtService;
        _googleAuthService = googleAuthService;
        _logger = logger;
    }

    /// <summary>
    /// Register new user with email and password
    /// </summary>
    [HttpPost("register")]
    public async Task<ActionResult<AuthResponseDto>> Register([FromBody] RegisterRequestDto request)
    {
        try
        {
            // Validate input
            if (string.IsNullOrWhiteSpace(request.Email) ||
                string.IsNullOrWhiteSpace(request.Password) ||
                string.IsNullOrWhiteSpace(request.Name))
                return BadRequest(new { message = "Name, email, and password are required" });

            // Check if email already exists
            var existingUser = await _dbContext.Users
                .FirstOrDefaultAsync(u => u.Email == request.Email);

            if (existingUser != null)
                return Conflict(new { message = "Email already registered" });

            // Hash password
            var hashedPassword = PasswordHasher.HashPassword(request.Password);

            // Create user
            var user = new User
            {
                Name = request.Name,
                Email = request.Email,
                Password = hashedPassword,
                Active = true,
                CreatedAt = DateTime.UtcNow
            };

            _dbContext.Users.Add(user);
            await _dbContext.SaveChangesAsync();

            // Generate token
            var token = _jwtService.GenerateToken(user);

            _logger.LogInformation("User registered successfully: {Email}", user.Email);

            return Ok(new AuthResponseDto
            {
                UserId = user.Id,
                Name = user.Name,
                Email = user.Email,
                Token = token,
                Active = user.Active
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error during user registration");
            return StatusCode(500, new { message = "An error occurred during registration" });
        }
    }

    /// <summary>
    /// Register or login user with Google OAuth
    /// </summary>
    [HttpPost("register/google")]
    public async Task<ActionResult<AuthResponseDto>> RegisterWithGoogle([FromBody] GoogleRegisterRequestDto request)
    {
        try
        {
            // Validate Google token
            var googlePayload = await _googleAuthService.ValidateGoogleToken(request.IdToken);
            if (googlePayload == null)
                return Unauthorized(new { message = "Invalid Google token" });

            // Check if user exists (use AsTracking for updates)
            var existingUser = await _dbContext.Users
                .AsTracking()
                .FirstOrDefaultAsync(u => u.Email == googlePayload.Email || u.GoogleAuthId == googlePayload.Subject);

            User user;

            if (existingUser != null)
            {
                // User exists - update Google Auth ID if needed
                if (string.IsNullOrEmpty(existingUser.GoogleAuthId))
                    existingUser.GoogleAuthId = googlePayload.Subject;

                existingUser.LastLoginAt = DateTime.UtcNow;
                await _dbContext.SaveChangesAsync();
                user = existingUser;

                _logger.LogInformation("Existing user logged in with Google: {Email}", user.Email);
            }
            else
            {
                // Create new user
                user = new User
                {
                    Name = googlePayload.Name,
                    Email = googlePayload.Email,
                    GoogleAuthId = googlePayload.Subject,
                    Active = true,
                    CreatedAt = DateTime.UtcNow,
                    LastLoginAt = DateTime.UtcNow
                };

                _dbContext.Users.Add(user);
                await _dbContext.SaveChangesAsync();

                _logger.LogInformation("New user registered with Google: {Email}", user.Email);
            }

            // Generate token
            var token = _jwtService.GenerateToken(user);

            return Ok(new AuthResponseDto
            {
                UserId = user.Id,
                Name = user.Name,
                Email = user.Email,
                Token = token,
                Active = user.Active,
                GoogleAuthId = user.GoogleAuthId
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error during Google authentication");
            return StatusCode(500, new { message = "An error occurred during Google authentication" });
        }
    }

    /// <summary>
    /// Login with email and password
    /// </summary>
    [HttpPost("login")]
    public async Task<ActionResult<AuthResponseDto>> Login([FromBody] LoginRequestDto request)
    {
        try
        {
            // Validate input
            if (string.IsNullOrWhiteSpace(request.Email) || string.IsNullOrWhiteSpace(request.Password))
                return BadRequest(new { message = "Email and password are required" });

            // Find user (use AsTracking for updates)
            var user = await _dbContext.Users
                .AsTracking()
                .FirstOrDefaultAsync(u => u.Email == request.Email);

            if (user == null)
                return Unauthorized(new { message = "Invalid email or password" });

            // Check if account uses Google Sign-In only
            if (string.IsNullOrEmpty(user.Password))
                return BadRequest(new { message = "This account uses Google Sign-In. Please use 'Sign in with Google' button." });

            // Verify password
            if (!PasswordHasher.VerifyPassword(request.Password, user.Password))
                return Unauthorized(new { message = "Invalid email or password" });

            // Check if account is active
            if (!user.Active)
                return Unauthorized(new { message = "Your account has been deactivated. Please contact support." });

            // Update last login
            user.LastLoginAt = DateTime.UtcNow;
            await _dbContext.SaveChangesAsync();

            // Generate token
            var token = _jwtService.GenerateToken(user);

            _logger.LogInformation("User logged in successfully: {Email}", user.Email);

            return Ok(new AuthResponseDto
            {
                UserId = user.Id,
                Name = user.Name,
                Email = user.Email,
                Token = token,
                Active = user.Active
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error during login");
            return StatusCode(500, new { message = "An error occurred during login" });
        }
    }

    /// <summary>
    /// Login with Google OAuth
    /// </summary>
    [HttpPost("login/google")]
    public async Task<ActionResult<AuthResponseDto>> LoginWithGoogle([FromBody] GoogleLoginRequestDto request)
    {
        try
        {
            // Validate Google token
            var googlePayload = await _googleAuthService.ValidateGoogleToken(request.IdToken);
            if (googlePayload == null)
                return Unauthorized(new { message = "Invalid Google token" });

            // Find user (use AsTracking for updates)
            var user = await _dbContext.Users
                .AsTracking()
                .FirstOrDefaultAsync(u => u.GoogleAuthId == googlePayload.Subject);

            if (user == null)
                return Unauthorized(new { message = "No account found with this Google account. Please register first." });

            // Check if account is active
            if (!user.Active)
                return Unauthorized(new { message = "Your account has been deactivated. Please contact support." });

            // Update last login
            user.LastLoginAt = DateTime.UtcNow;
            await _dbContext.SaveChangesAsync();

            // Generate token
            var token = _jwtService.GenerateToken(user);

            _logger.LogInformation("User logged in with Google: {Email}", user.Email);

            return Ok(new AuthResponseDto
            {
                UserId = user.Id,
                Name = user.Name,
                Email = user.Email,
                Token = token,
                Active = user.Active,
                GoogleAuthId = user.GoogleAuthId
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error during Google login");
            return StatusCode(500, new { message = "An error occurred during Google login" });
        }
    }
}
