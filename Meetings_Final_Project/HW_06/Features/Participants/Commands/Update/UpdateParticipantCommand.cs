using HW_06.DTOs.ParticipantDTO;
using MediatR;

namespace HW_06.Features.Participants.Commands.Update;

/// <summary>
/// Команда для повного оновлення
/// даних учасника.
/// </summary>
/// <param name="Id">
/// Ідентифікатор учасника.
/// </param>
/// <param name="Dto">
/// Нові дані учасника.
/// </param>
public record UpdateParticipantCommand(
    int Id,
    ParticipantUpdateDTO Dto)
    : IRequest<bool>;