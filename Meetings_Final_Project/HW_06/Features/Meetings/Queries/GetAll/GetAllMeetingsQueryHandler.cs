using AutoMapper;
using HW_06.DTOs.MeetingDTO;
using HW_06.Helpers.Pagination;
using HW_06.Helpers.Queryable;
using HW_06.Models;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace HW_06.Features.Meetings.Queries.GetAll;

/// <summary>
/// Обробник запиту для отримання
/// списку зустрічей.
/// </summary>
public class GetAllMeetingsQueryHandler
    : IRequestHandler<
        GetAllMeetingsQuery,
        PagedResult<MeetingReadDTO>>
{
    private readonly DataContext _context;
    private readonly IMapper _mapper;

    /// <summary>
    /// Ініціалізує обробник запиту.
    /// </summary>
    /// <param name="context">
    /// Контекст бази даних.
    /// </param>
    /// <param name="mapper">
    /// Сервіс AutoMapper.
    /// </param>
    public GetAllMeetingsQueryHandler(
        DataContext context,
        IMapper mapper)
    {
        ArgumentNullException.ThrowIfNull(context);
        ArgumentNullException.ThrowIfNull(mapper);

        _context = context;
        _mapper = mapper;
    }

    /// <summary>
    /// Отримує сторінку зі списком зустрічей.
    /// </summary>
    public async Task<PagedResult<MeetingReadDTO>> Handle(
        GetAllMeetingsQuery request,
        CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(request);

        IQueryable<Meeting> query =
            _context.Meetings
                .AsNoTracking()
                .Include(meeting =>
                    meeting.Room)
                .Include(meeting =>
                    meeting.MeetingParticipants);

        query = query
            .ApplySearch(
                request.Parameters.Search)
            .ApplyFilter(
                request.Filter)
            .ApplySorting(
                request.Parameters);

        return await query
            .ToPagedResultAsync<
                Meeting,
                MeetingReadDTO>(
                    request.Parameters.Page,
                    request.Parameters.PageSize,
                    _mapper);
    }
}