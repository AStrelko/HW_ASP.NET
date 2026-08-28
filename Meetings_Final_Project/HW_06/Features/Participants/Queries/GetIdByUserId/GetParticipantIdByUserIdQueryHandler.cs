using MediatR;
using Microsoft.EntityFrameworkCore;

namespace HW_06.Features.Participants.Queries.GetIdByUserId;

/// <summary>
/// Обробник запиту для отримання
/// ідентифікатора учасника
/// за ідентифікатором користувача Identity.
/// </summary>
public class GetParticipantIdByUserIdQueryHandler
    : IRequestHandler<
        GetParticipantIdByUserIdQuery,
        int?>
{
    private readonly DataContext _context;

    /// <summary>
    /// Ініціалізує обробник запиту.
    /// </summary>
    /// <param name="context">
    /// Контекст бази даних.
    /// </param>
    public GetParticipantIdByUserIdQueryHandler(
        DataContext context)
    {
        ArgumentNullException.ThrowIfNull(context);

        _context = context;
    }

    /// <summary>
    /// Повертає ідентифікатор учасника,
    /// пов'язаного з користувачем Identity.
    /// </summary>
    public async Task<int?> Handle(
        GetParticipantIdByUserIdQuery request,
        CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(request);

        return await _context.Participants
            .AsNoTracking()
            .Where(participant =>
                participant.ApplicationUserId ==
                request.ApplicationUserId)
            .Select(participant =>
                (int?)participant.ParticipantId)
            .FirstOrDefaultAsync(
                cancellationToken);
    }
}