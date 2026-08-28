using HW_06.Models;
using MediatR;
using Microsoft.AspNetCore.Identity;
using HW_06.Common.Constants;

namespace HW_06.Features.Auth.Commands.Register;

/// <summary>
/// Обробник команди реєстрації користувача.
/// </summary>
public class RegisterCommandHandler
    : IRequestHandler<
        RegisterCommand,
        IdentityResult>
{
    private readonly DataContext _context;

    private readonly UserManager<ApplicationUser>
        _userManager;

    private readonly ILogger<RegisterCommandHandler>
        _logger;

    /// <summary>
    /// Ініціалізує обробник команди реєстрації.
    /// </summary>
    /// <param name="context">
    /// Контекст бази даних.
    /// </param>
    /// <param name="userManager">
    /// Менеджер користувачів ASP.NET Core Identity.
    /// </param>
    /// <param name="logger">
    /// Сервіс журналювання подій реєстрації.
    /// </param>
    public RegisterCommandHandler(
        DataContext context,
        UserManager<ApplicationUser> userManager,
        ILogger<RegisterCommandHandler> logger)
    {
        ArgumentNullException.ThrowIfNull(context);
        ArgumentNullException.ThrowIfNull(userManager);
        ArgumentNullException.ThrowIfNull(logger);

        _context = context;
        _userManager = userManager;
        _logger = logger;
    }

    /// <summary>
    /// Реєструє нового користувача,
    /// призначає йому роль User
    /// та створює профіль учасника.
    /// </summary>
    public async Task<IdentityResult> Handle(
        RegisterCommand request,
        CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(request);

        var dto =
            request.Dto;

        var email =
            dto.Email.Trim();

        var user =
            new ApplicationUser
            {
                Email = email,
                UserName = email
            };

        var result =
            await _userManager.CreateAsync(
                user,
                dto.Password);

        if (!result.Succeeded)
        {
            _logger.LogWarning(
                "Не вдалося зареєструвати користувача з Email: {Email}.",
                email);

            return result;
        }

        var roleResult =
            await _userManager.AddToRoleAsync(
                user,
                ApplicationRoles.User);

        if (!roleResult.Succeeded)
        {
            _logger.LogWarning(
                "Не вдалося призначити роль {Role} користувачу з Email: {Email}.",
                ApplicationRoles.User,
                email);

            await _userManager.DeleteAsync(
                user);

            return roleResult;
        }

        var participant =
            new Participant
            {
                FirstName =
                    dto.FirstName.Trim(),

                LastName =
                    dto.LastName.Trim(),

                Position =
                    dto.Position?.Trim(),

                ApplicationUserId =
                    user.Id
            };

        try
        {
            await _context.Participants.AddAsync(
                participant,
                cancellationToken);

            await _context.SaveChangesAsync(
                cancellationToken);
        }
        catch
        {
            // Якщо створення профілю учасника
            // не вдалося, видаляємо створений
            // Identity-акаунт разом із ролями.
            await _userManager.DeleteAsync(
                user);

            throw;
        }

        _logger.LogInformation(
            "Користувача з Email: {Email} успішно зареєстровано.",
            email);

        return result;
    }
}