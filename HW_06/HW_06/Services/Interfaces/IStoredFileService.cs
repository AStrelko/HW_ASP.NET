using HW_06.DTOs.Files;
using HW_06.Models.Files;

namespace HW_06.Services.Interfaces;

public interface IStoredFileService
{
    Task<FileReadDTO> UploadPublicAsync(
        IFormFile file,
        CancellationToken cancellationToken = default);

    Task<FileReadDTO> UploadPrivateAsync(
        int participantId,
        IFormFile file,
        CancellationToken cancellationToken = default);

    Task<FileReadDTO> UploadAvatarAsync(
        int participantId,
        IFormFile file,
        CancellationToken cancellationToken = default);

    Task<FileDownloadResult?> GetDownloadAsync(
        int fileId,
        int? requestingParticipantId,
        CancellationToken cancellationToken = default);

    Task<bool> DeleteAsync(
        int fileId,
        int? requestingParticipantId,
        CancellationToken cancellationToken = default);
}