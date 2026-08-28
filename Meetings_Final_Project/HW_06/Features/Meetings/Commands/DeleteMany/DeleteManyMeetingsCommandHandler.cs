using MediatR;
using Microsoft.EntityFrameworkCore;

namespace HW_06.Features.Meetings.Commands.DeleteMany;

/// <summary>
/// Обробник команди видалення
/// декількох зустрічей.
/// </summary>
public class DeleteManyMeetingsCommandHandler
    : IRequestHandler<
        DeleteManyMeetingsCommand,
        int>
{
    private readonly DataContext _context;
    private readonly ILogger<DeleteManyMeetingsCommandHandler> _logger;

    /// <summary>
    /// Ініціалізує обробник команди
    /// масового видалення зустрічей.
    /// </summary>
    /// <param name="context">
    /// Контекст бази даних.
    /// </param>
    public DeleteManyMeetingsCommandHandler(
        DataContext context,
        ILogger<DeleteManyMeetingsCommandHandler> logger)
    {
        ArgumentNullException.ThrowIfNull(context);
        ArgumentNullException.ThrowIfNull(logger);

        _context = context;
        _logger = logger;
    }

    /// <summary>
    /// Видаляє зустрічі,
    /// ідентифікатори яких передані в команді.
    /// </summary>
    public async Task<int> Handle(
        DeleteManyMeetingsCommand request,
        CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(request);

        var validIds =
            request.Ids
                .Distinct()
                .ToList();

        var meetings =
            await _context.Meetings
                .Where(meeting =>
                    validIds.Contains(
                        meeting.MeetingId))
                .ToListAsync(
                    cancellationToken);

        if (meetings.Count == 0)
        {
            _logger.LogWarning(
                "Не вдалося видалити зустрічі. Жодної зустрічі за переданими ідентифікаторами не знайдено.");

            return 0;
        }

        _context.Meetings.RemoveRange(
            meetings);

        await _context.SaveChangesAsync(
            cancellationToken);

        _logger.LogInformation(
            "Успішно видалено зустрічі. Кількість: {Count}",
            meetings.Count);

        return meetings.Count;
    }
}