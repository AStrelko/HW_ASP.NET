using HW_06.Models.Files;
using MediatR;

namespace HW_06.Features.Participants.Queries.GetAvatar;

/// <summary>
/// Запит для отримання
/// аватара учасника.
/// </summary>
/// <param name="ParticipantId">
/// Ідентифікатор учасника.
/// </param>
public record GetParticipantAvatarQuery(
    int ParticipantId)
    : IRequest<FileDownloadResult?>;