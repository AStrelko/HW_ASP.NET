using HW_06.DTOs.IdentityDTO;
using HW_06.Models;
using HW_06.Services.Interfaces;
using MediatR;
using Microsoft.AspNetCore.Identity;

namespace HW_06.Features.Auth.Commands.Login;

/// <summary>
/// Обробник команди входу користувача.
/// </summary>
public class LoginCommandHandler
    : IRequestHandler<LoginCommand, LoginResult>
{
    private readonly UserManager<ApplicationUser>
        _userManager;

    private readonly SignInManager<ApplicationUser>
        _signInManager;

    private readonly ITokenService
        _tokenService;

    private readonly ILogger<LoginCommandHandler>
        _logger;

    /// <summary>
    /// Ініціалізує обробник команди входу.
    /// </summary>
    /// <param name="userManager">
    /// Менеджер користувачів ASP.NET Core Identity.
    /// </param>
    /// <param name="signInManager">
    /// Менеджер перевірки облікових даних користувача.
    /// </param>
    /// <param name="tokenService">
    /// Сервіс створення JWT access token.
    /// </param>
    /// <param name="logger">
    /// Сервіс журналювання подій входу користувача.
    /// </param>
    public LoginCommandHandler(
        UserManager<ApplicationUser> userManager,
        SignInManager<ApplicationUser> signInManager,
        ITokenService tokenService,
        ILogger<LoginCommandHandler> logger)
    {
        ArgumentNullException.ThrowIfNull(userManager);
        ArgumentNullException.ThrowIfNull(signInManager);
        ArgumentNullException.ThrowIfNull(tokenService);
        ArgumentNullException.ThrowIfNull(logger);

        _userManager = userManager;
        _signInManager = signInManager;
        _tokenService = tokenService;
        _logger = logger;
    }

    /// <summary>
    /// Перевіряє облікові дані користувача
    /// та створює JWT access token.
    /// </summary>
    public async Task<LoginResult> Handle(
        LoginCommand request,
        CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(request);

        var dto =
            request.Dto;

        var email =
            dto.Email.Trim();

        var user =
            await _userManager.FindByEmailAsync(
                email);

        if (user is null)
        {
            _logger.LogWarning(
                "Невдала спроба входу. Користувача з Email: {Email} не знайдено.",
                email);

            return new LoginResult(
                false,
                false,
                "Неправильний логін або пароль.",
                null);
        }

        var result =
            await _signInManager.CheckPasswordSignInAsync(
                user,
                dto.Password,
                lockoutOnFailure: true);

        if (!result.Succeeded)
        {
            _logger.LogWarning(
                "Невдала спроба входу користувача з Email: {Email}. LockedOut: {LockedOut}",
                email,
                result.IsLockedOut);

            return new LoginResult(
                false,
                result.IsLockedOut,
                result.IsLockedOut
                    ? "Акаунт заблоковано."
                    : "Неправильний логін або пароль.",
                null);
        }

        var token =
            await _tokenService.CreateAccessTokenAsync(
                user,
                cancellationToken);

        _logger.LogInformation(
            "Користувач з Email: {Email} успішно виконав вхід.",
            email);

        return new LoginResult(
            true,
            false,
            null,
            new AuthResponseDto(
                Message:
                    "Вхід виконано успішно.",

                Token:
                    token.Token,

                ExpiresAtUtc:
                    token.ExpiresAtUtc));
    }
}