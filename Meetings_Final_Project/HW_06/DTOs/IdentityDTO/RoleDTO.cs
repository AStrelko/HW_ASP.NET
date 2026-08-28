namespace HW_06.DTOs.IdentityDTO;

/// <summary>
/// DTO для роботи з роллю користувача.
/// </summary>
public class RoleDTO
{
    /// <summary>
    /// Назва ролі.
    /// </summary>
    public string RoleName { get; set; } = string.Empty;
}

/// <summary>
/// DTO для зміни ролі учасника.
/// </summary>
public class UserRoleDTO
{
    /// <summary>
    /// Ідентифікатор учасника.
    /// </summary>
    public int ParticipantId { get; set; }

    /// <summary>
    /// Назва нової ролі.
    /// </summary>
    public string RoleName { get; set; } = string.Empty;
}