using HW_06.DTOs.ParticipantDTO;
using MediatR;

namespace HW_06.Features.Participants.Queries.GetById;

/// <summary>
/// Запит для отримання детальної інформації
/// про учасника за його ідентифікатором.
/// </summary>
/// <param name="Id">
/// Ідентифікатор учасника.
/// </param>
public record GetParticipantByIdQuery(
    int Id)
    : IRequest<ParticipantDetailDTO?>;