using HW_06.Models.Files;

namespace HW_06.DTOs.Files;

public class FileUploadDTO
{
    public IFormFile File { get; set; } = null!;

    public FileAccessLevel AccessLevel { get; set; }

    public int? ParticipantId { get; set; }
}