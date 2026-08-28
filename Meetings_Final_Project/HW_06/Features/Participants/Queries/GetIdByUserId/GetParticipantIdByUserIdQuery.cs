using MediatR;

namespace HW_06.Features.Participants.Queries.GetIdByUserId;

/// <summary>
/// Запит для отримання ідентифікатора учасника
/// за ідентифікатором користувача ASP.NET Identity.
/// </summary>
/// <param name="ApplicationUserId">
/// Ідентифікатор користувача ASP.NET Identity.
/// </param>
public record GetParticipantIdByUserIdQuery(
    string ApplicationUserId)
    : IRequest<int?>;