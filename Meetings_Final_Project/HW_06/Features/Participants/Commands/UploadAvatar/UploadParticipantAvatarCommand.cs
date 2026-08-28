
using HW_06.DTOs.ParticipantsDTO;
using MediatR;
using Microsoft.AspNetCore.Http;

namespace HW_06.Features.Participants.Commands.UploadAvatar;

/// <summary>
/// Команда для додавання
/// або заміни аватара учасника.
/// </summary>
/// <param name="ParticipantId">
/// Ідентифікатор учасника.
/// </param>
/// <param name="File">
/// Файл нового аватара.
/// </param>
public record UploadParticipantAvatarCommand(
    int ParticipantId,
    IFormFile File)
    : IRequest<ParticipantAvatarDTO?>;