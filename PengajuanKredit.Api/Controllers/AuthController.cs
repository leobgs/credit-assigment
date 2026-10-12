using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;
using PengajuanKredit.Api.Dtos;

namespace PengajuanKredit.Api.Controllers;

[ApiController]
[Route("api/auth")]
public class AuthController : ControllerBase
{
    private readonly IConfiguration _configuration;

    public AuthController(IConfiguration configuration)
    {
        _configuration = configuration;
    }

    [HttpPost("login")]
    [AllowAnonymous]
    public ActionResult<LoginResponse> Login([FromBody] LoginRequest request)
    {
        string? expectedUsername = _configuration["Auth:Username"];
        string? expectedPassword = _configuration["Auth:Password"];

        if (string.IsNullOrEmpty(expectedUsername) || string.IsNullOrEmpty(expectedPassword) ||
            request.Username != expectedUsername || request.Password != expectedPassword)
        {
            return Unauthorized(new ProblemDetails
            {
                Status = StatusCodes.Status401Unauthorized,
                Title = "Autentikasi Gagal",
                Detail = "Username atau password yang dimasukkan salah."
            });
        }

        string key = _configuration["Jwt:Key"]
            ?? throw new InvalidOperationException("Konfigurasi Jwt:Key tidak ditemukan.");
        string issuer = _configuration["Jwt:Issuer"] ?? "PengajuanKredit";
        string audience = _configuration["Jwt:Audience"] ?? "PengajuanKreditUsers";
        int expireMinutes = int.TryParse(_configuration["Jwt:ExpireMinutes"], out int exp) ? exp : 60;

        var tokenHandler = new JwtSecurityTokenHandler();
        byte[] keyBytes = Encoding.UTF8.GetBytes(key);
        var expiresAt = DateTime.UtcNow.AddMinutes(expireMinutes);

        var tokenDescriptor = new SecurityTokenDescriptor
        {
            Subject = new ClaimsIdentity(new[]
            {
                new Claim(ClaimTypes.NameIdentifier, request.Username),
                new Claim(ClaimTypes.Name, request.Username),
                new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString())
            }),
            Expires = expiresAt,
            Issuer = issuer,
            Audience = audience,
            SigningCredentials = new SigningCredentials(
                new SymmetricSecurityKey(keyBytes),
                SecurityAlgorithms.HmacSha256Signature)
        };

        var token = tokenHandler.CreateToken(tokenDescriptor);
        string tokenString = tokenHandler.WriteToken(token);

        return Ok(new LoginResponse(tokenString, expiresAt));
    }
}
