namespace HW_06.DTOs.IdentityDTO;

public record LoginResult(bool Success, bool IsLockedOut, string? Message, AuthResponseDto? Response);
public record AuthResponseDto(string Message, string Token, DateTime ExpiresAtUtc);

