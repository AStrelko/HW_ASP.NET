using HW_06.DTOs.ParticipantDTO;
using MediatR;

namespace HW_06.Features.Participants.Commands.PartialUpdate;

/// <summary>
/// Команда для часткового оновлення
/// даних учасника.
/// </summary>
/// <param name="Id">
/// Ідентифікатор учасника.
/// </param>
/// <param name="Dto">
/// Поля учасника, які необхідно оновити.
/// </param>
public record PartialUpdateParticipantCommand(
    int Id,
    ParticipantPartialUpdateDTO Dto)
    : IRequest<bool>;