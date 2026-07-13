using HW_06.Models;
using Microsoft.EntityFrameworkCore;

namespace HW_06.Services;

public class MeetingServices
{
    private readonly DataContext _context;

    public MeetingServices(DataContext context)
    {
        _context = context;
    }
    // Отримання списку зустрічей з підтримкою:
    // пошуку, сортування, фільтрації та пагінації
    public async Task<List<Meeting>> GetMeetings(MeetingFilter filter)
    {
        var query =_context.Set<Meeting>().AsNoTracking();

        // Пошук зустрічей за ключовим словом у назві
        if (!string.IsNullOrWhiteSpace(filter.Search))
        {
            query = query.Where(x =>
                x.Title.Contains(filter.Search));
        }
        // Фільтр за початковою датою
        if (filter.StartTime.HasValue)
        {
            query = query.Where(x =>
                x.DateTime >= filter.StartTime.Value);
        }
        // Фільтр за кінцевою датою
        if (filter.EndTime.HasValue)
        {
            query = query.Where(x =>
                x.DateTime <= filter.EndTime.Value);
        }
        // сортування за
        switch (filter.SortBy?.ToLower())
        {
            case "title"://назвою
                query = query.OrderBy(x => x.Title);
                break;

            case "date"://дотой
                query = query.OrderBy(x => x.DateTime);
                break;
        }
        // Пагінація результатів
        query = query
            .Skip((filter.Page - 1) * filter.PageSize)//починаючи
            .Take(filter.PageSize);//кілкість

        return await query.ToListAsync();//результат в вигляді рядка
    }
    // отримую зустріч по id 
    public async Task<Meeting?> GetById(int id)
    {
        return await _context.Meetings.FindAsync(id);
    }
    // додаю зустріч
    public async Task Add(Meeting meeting)
    {
        _context.Meetings.Add(meeting);
        await _context.SaveChangesAsync();
    }
    // оновлюю дані зустрічи
    public async Task Update(Meeting meeting)
    {
        _context.Meetings.Update(meeting);
        await _context.SaveChangesAsync();
    }
    // видаляю зустріч по id
    public async Task Delete(int id)
    {
        var meeting = await _context.Meetings.FindAsync(id);

        if (meeting == null)
            return;

        _context.Meetings.Remove(meeting);

        await _context.SaveChangesAsync();
    }
    //видоляю зустрічи по масиву id
    public async Task DeleteMany(List<int> ids)
    {
        var meetings = await _context.Meetings
            .Where(x => ids.Contains(x.MeetingId))
            .ToListAsync();

        _context.Meetings.RemoveRange(meetings);

        await _context.SaveChangesAsync();
    }
}