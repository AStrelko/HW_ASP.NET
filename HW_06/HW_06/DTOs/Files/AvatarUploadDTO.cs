namespace HW_06.DTOs.Files;

/// <summary>
/// DTO для завантаження аватара учасника.
/// </summary>
public class AvatarUploadDTO
{
    /// <summary>
    /// Файл зображення,
    /// який буде використано
    /// як аватар учасника.
    /// </summary>
    public required IFormFile File { get; set; }
}