using HW_06.Models.Files;

namespace HW_06.DTOs.Files;

public class FileReadDTO
{
    public int Id { get; set; }

    public string FileName { get; set; } = null!;

    public string ContentType { get; set; } = null!;

    public long Size { get; set; }

    public FileCategory Category { get; set; }

    public FileAccessLevel AccessLevel { get; set; }

    public int? OwnerParticipantId { get; set; }

    public DateTime UploadedAt { get; set; }

    public string? DownloadUrl { get; set; }
}