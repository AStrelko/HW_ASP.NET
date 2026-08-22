using HW_06.Models;

namespace HW_06.Services.Interfaces;

public interface ITokenService
{
    Task<AccessTokenResult> CreateAccessTokenAsync(ApplicationUser user, CancellationToken ct = default);
}
  
public record AccessTokenResult(string Token, DateTime ExpiresAtUtc);