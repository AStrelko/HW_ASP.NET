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

    private readonly UserManager<ApplicationUser>
        _userManager;

    private readonly SignInManager<ApplicationUser>
        _signInManager;

    /// <summary>
    /// Ініціалізує сервіс автентифікації.
    /// </summary>
    public AuthService(
        DataContext context,
        UserManager<ApplicationUser> userManager,
        SignInManager<ApplicationUser> signInManager)
    {
        _context = context;
        _userManager = userManager;
        _signInManager = signInManager;
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
            // Якщо створення Participant не вдалося,
            // видаляємо вже створений Identity-акаунт,
            // щоб не залишати неповні дані.
            await _userManager.DeleteAsync(user);

            throw;
        }

        return result;
    }

    /// <summary>
    /// Виконує вхід користувача в систему.
    /// </summary>
    public async Task<SignInResult> LoginAsync(
        LoginDTO dto)
    {
        var user = await _userManager.FindByEmailAsync(
            dto.Email.Trim());

        if (user is null)
        {
            return SignInResult.Failed;
        }

        return await _signInManager.PasswordSignInAsync(
            user,
            dto.Password,
            isPersistent: false,
            lockoutOnFailure: true);
    }
}