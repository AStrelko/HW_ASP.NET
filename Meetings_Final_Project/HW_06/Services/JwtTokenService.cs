using System.Security.Claims;
using System.Text;
using HW_06.Models;
using HW_06.Services.Interfaces;
using HW_06.Storage.Configurations;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.JsonWebTokens;
using Microsoft.IdentityModel.Tokens;

namespace HW_06.Services;

public sealed class JwtTokenService : ITokenService
{
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly JwtOptions _jwtOptions;

    public JwtTokenService(
        UserManager<ApplicationUser> userManager,
        IOptions<JwtOptions> jwtOptions)
    {
        _userManager = userManager;
        _jwtOptions = jwtOptions.Value;
        
    }
 
    public async Task<AccessTokenResult> CreateAccessTokenAsync(ApplicationUser user,
        CancellationToken cancellationToken = default)
    {
        var issuer = _jwtOptions.Issuer ?? throw new InvalidOperationException("Jwt:Issuer не налаштовано");
        var audience = _jwtOptions.Audience ?? throw new InvalidOperationException("Jwt:Audience не налаштовано");
        var key = _jwtOptions.Key ?? throw new InvalidOperationException("Jwt:Key не налаштовано");
        var minutes = _jwtOptions.AccessTokenMinutes;
 
        var now = DateTime.UtcNow;
        var expiresAt = now.AddMinutes(minutes);
 
 
        var claims = new List<Claim>
        {
            new(JwtRegisteredClaimNames.Sub, user.Id),
            new(JwtRegisteredClaimNames.Email, user.Email!),
            new(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
            new("username", user.UserName!),
            new("email_verified", user.EmailConfirmed ? "true" : "false")
        };
        
        foreach (var role in await _userManager.GetRolesAsync(user))
        {
            claims.Add(new Claim("role", role));
        }
 
        claims.AddRange(await _userManager.GetClaimsAsync(user));
 
        var signingKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(key));
        var credentials = new SigningCredentials(signingKey, SecurityAlgorithms.HmacSha256);
 
        var descriptor = new SecurityTokenDescriptor
        {
            Issuer = issuer,
            Audience = audience,
            Subject = new ClaimsIdentity(claims),
            IssuedAt = now,
            NotBefore = now,
            Expires = expiresAt,
            SigningCredentials = credentials
        };
 
        var handler = new JsonWebTokenHandler();
        var token = handler.CreateToken(descriptor);
 
        return new AccessTokenResult(token, expiresAt);
    }
}
