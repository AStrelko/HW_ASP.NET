using HW_06.DTOs.IdentityDTO;
using MediatR;

namespace HW_06.Features.Auth.Commands.Login;

/// <summary>
/// Команда для входу користувача в систему.
/// </summary>
/// <param name="Dto">
/// Дані для входу користувача.
/// </param>
public record LoginCommand(LoginDTO Dto) : IRequest<LoginResult>;