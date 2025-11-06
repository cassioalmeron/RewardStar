using Microsoft.AspNetCore.Mvc;
using RewardStar.Api.Models;
using RewardStar.Api.Services;
using RewardStart.Core;
using Microsoft.EntityFrameworkCore;

namespace RewardStar.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class AuthController : ControllerBase
{
    private readonly JwtService _jwtService;
    private readonly RewardStartDbContext _context;

    public AuthController(JwtService jwtService, RewardStartDbContext context)
    {
        _jwtService = jwtService;
        _context = context;
    }

    [HttpPost("login")]
    public async Task<ActionResult<LoginResponse>> Login(LoginRequest request)
    {
        // TODO: Implement real user and password verification
        // This is just a basic example
        var user = await _context.Users
            .FirstOrDefaultAsync(u => u.Email == request.Email);

        if (user == null)
        {
            return Unauthorized(new { message = "Invalid email or password" });
        }

        // TODO: Implement real password verification with hash
        // if (!VerifyPassword(request.Password, user.PasswordHash))
        // {
        //     return Unauthorized(new { message = "Invalid email or password" });
        // }

        var token = _jwtService.GenerateToken(
            userId: user.Id.ToString(),
            email: user.Email,
            role: user.Role ?? "User"
        );

        return Ok(new LoginResponse
        {
            Token = token,
            Email = user.Email,
            Role = user.Role ?? "User"
        });
    }
} 