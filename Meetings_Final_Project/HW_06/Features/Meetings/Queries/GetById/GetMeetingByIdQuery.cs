using HW_06.DTOs.MeetingDTO;
using MediatR;

namespace HW_06.Features.Meetings.Queries.GetById;

/// <summary>
/// Запит для отримання детальної інформації
/// про зустріч за її ідентифікатором.
/// </summary>
/// <param name="Id">
/// Ідентифікатор зустрічі.
/// </param>
public record GetMeetingByIdQuery(
    int Id)
    : IRequest<MeetingDetailDTO?>;