using MediatR;
using Microsoft.EntityFrameworkCore;

namespace HW_06.Features.Meetings.Commands.PartialUpdate;

/// <summary>
/// Обробник команди часткового оновлення зустрічі.
/// </summary>
public class PartialUpdateMeetingCommandHandler
    : IRequestHandler<
        PartialUpdateMeetingCommand,
        bool>
{
    private readonly DataContext _context;
    private readonly ILogger<PartialUpdateMeetingCommandHandler> _logger;

    /// <summary>
    /// Ініціалізує обробник команди
    /// часткового оновлення зустрічі.
    /// </summary>
    /// <param name="context">
    /// Контекст бази даних.
    /// </param>
    public PartialUpdateMeetingCommandHandler(
        DataContext context,
        ILogger<PartialUpdateMeetingCommandHandler> logger)
    {
        ArgumentNullException.ThrowIfNull(context);
        ArgumentNullException.ThrowIfNull(logger);

        _context = context;
        _logger = logger;
    }

    /// <summary>
    /// Частково оновлює існуючу зустріч.
    /// Змінюються лише передані поля.
    /// </summary>
    public async Task<bool> Handle(
        PartialUpdateMeetingCommand request,
        CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(request);

        var dto =
            request.Dto;

        var meeting =
            await _context.Meetings
                .FirstOrDefaultAsync(
                    item =>
                        item.MeetingId ==
                        request.Id,
                    cancellationToken);

        if (meeting is null)
        {
            _logger.LogWarning(
                "Не вдалося частково оновити зустріч. MeetingId: {MeetingId} не знайдено.",
                request.Id);

            return false;
        }

        if (dto.Title is not null)
        {
            meeting.Title =
                dto.Title;
        }

        if (dto.Description is not null)
        {
            meeting.Description =
                dto.Description;
        }

        if (dto.DateTime.HasValue)
        {
            meeting.DateTime =
                dto.DateTime.Value;
        }

        if (dto.RoomNumber.HasValue)
        {
            var room =
                await _context.Rooms
                    .AsNoTracking()
                    .FirstOrDefaultAsync(
                        room =>
                            room.NumberRoom ==
                            dto.RoomNumber.Value,
                        cancellationToken);

            if (room is null)
            {
                throw new FluentValidation.ValidationException(
                    $"Кімнату з номером " +
                    $"{dto.RoomNumber.Value} не знайдено.");
            }

            meeting.RoomId =
                room.RoomId;
        }

        await _context.SaveChangesAsync(
            cancellationToken);
        
        _logger.LogInformation(
            "Зустріч успішно частково оновлено. MeetingId: {MeetingId}, Title: {Title}",
            meeting.MeetingId,
            meeting.Title);

        return true;
    }
}