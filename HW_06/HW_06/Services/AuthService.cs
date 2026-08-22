using HW_06.DTOs.IdentityDTO;
using HW_06.Models;
using HW_06.Services.Interfaces;
using Microsoft.AspNetCore.Identity;


namespace HW_06.Services;

/// <summary>
/// Сервіс для реєстрації та автентифікації користувачів.
/// </summary>
public class AuthService : IAuthService
{
    private readonly DataContext _context;
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly SignInManager<ApplicationUser> _signInManager;
    private readonly ITokenService _tokenService;

    /// <summary>
    /// Ініціалізує сервіс автентифікації.
    /// </summary>
    public AuthService(
        DataContext context,
        UserManager<ApplicationUser> userManager,
        SignInManager<ApplicationUser> signInManager,
        ITokenService tokenService)
    {
        _context = context;
        _userManager = userManager;
        _signInManager = signInManager;
        _tokenService = tokenService;
    }

    /// <summary>
    /// Реєструє нового користувача
    /// та створює пов'язаний профіль учасника.
    /// </summary>
    public async Task<IdentityResult> RegisterAsync(
        RegisterDTO dto,
        CancellationToken cancellationToken = default)
    {
        var email = dto.Email.Trim();

        var user = new ApplicationUser
        {
            Email = email,
            UserName = email
        };

        var result = await _userManager.CreateAsync(
            user,
            dto.Password);

        if (!result.Succeeded)
        {
            return result;
        }
        
        var roleResult =
            await _userManager.AddToRoleAsync(
                user,
                "User");

        if (!roleResult.Succeeded)
        {
            await _userManager.DeleteAsync(user);

            return roleResult;
        }

        var participant = new Participant
        {
            FirstName = dto.FirstName.Trim(),
            LastName = dto.LastName.Trim(),
            Position = dto.Position?.Trim(),
            ApplicationUserId = user.Id
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
            // Якщо створення профілю учасника не вдалося,
            // видаляємо створений Identity-акаунт
            // разом із його зв'язками з ролями.
            await _userManager.DeleteAsync(user);

            throw;
        }

        return result;
    }

    /// <summary>
    /// Виконує вхід користувача в систему.
    /// </summary>
    public async Task<LoginResult> LoginAsync(LoginDTO dto)
    {
        var user = await _userManager.FindByEmailAsync(dto.Email.Trim());

        if (user is null)
        {
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
            return new LoginResult(
                false,
                result.IsLockedOut,
                result.IsLockedOut
                    ? "Акаунт заблоковано."
                    : "Неправильний логін або пароль.",
                null);
        }

        var token = await _tokenService.CreateAccessTokenAsync(user);

        return new LoginResult(
            true,
            false,
            null,
            new AuthResponseDto(
                Message: "Вхід виконано успішно.",
                Token: token.Token,
                ExpiresAtUtc: token.ExpiresAtUtc));
    }
}