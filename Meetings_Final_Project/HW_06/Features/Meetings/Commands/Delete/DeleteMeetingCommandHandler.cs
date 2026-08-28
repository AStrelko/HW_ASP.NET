using MediatR;
using Microsoft.EntityFrameworkCore;

namespace HW_06.Features.Meetings.Commands.Delete;

/// <summary>
/// Обробник команди видалення зустрічі.
/// </summary>
public class DeleteMeetingCommandHandler
    : IRequestHandler<
        DeleteMeetingCommand,
        bool>
{
    private readonly DataContext _context;

    private readonly ILogger<DeleteMeetingCommandHandler>
        _logger;

    /// <summary>
    /// Ініціалізує обробник команди
    /// видалення зустрічі.
    /// </summary>
    /// <param name="context">
    /// Контекст бази даних.
    /// </param>
    /// <param name="logger">
    /// Сервіс журналювання подій видалення зустрічей.
    /// </param>
    public DeleteMeetingCommandHandler(
        DataContext context,
        ILogger<DeleteMeetingCommandHandler> logger)
    {
        ArgumentNullException.ThrowIfNull(context);
        ArgumentNullException.ThrowIfNull(logger);

        _context = context;
        _logger = logger;
    }

    /// <summary>
    /// Видаляє зустріч за її ідентифікатором.
    /// </summary>
    /// <param name="request">
    /// Команда з ідентифікатором зустрічі.
    /// </param>
    /// <param name="cancellationToken">
    /// Токен скасування асинхронної операції.
    /// </param>
    /// <returns>
    /// <see langword="true"/>, якщо зустріч успішно видалено;
    /// інакше — <see langword="false"/>.
    /// </returns>
    public async Task<bool> Handle(
        DeleteMeetingCommand request,
        CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(request);

        var meeting =
            await _context.Meetings
                .FirstOrDefaultAsync(
                    meeting =>
                        meeting.MeetingId ==
                        request.Id,
                    cancellationToken);

        if (meeting is null)
        {
            _logger.LogWarning(
                "Не вдалося видалити зустріч. MeetingId: {MeetingId} не знайдено.",
                request.Id);

            return false;
        }

        _context.Meetings.Remove(
            meeting);

        await _context.SaveChangesAsync(
            cancellationToken);

        _logger.LogInformation(
            "Зустріч успішно видалено. MeetingId: {MeetingId}, Title: {Title}",
            meeting.MeetingId,
            meeting.Title);

        return true;
    }
}