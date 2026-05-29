using OnlineStoreLes02Lab.Core.Models;
using OnlineStoreLes02Lab.Storage;

namespace OnlineStoreLes02Lab.Web.Services;

public class LogService
{
    private readonly DataContext _context;

    public LogService(DataContext context)
    {
        _context = context;
    }

    public void AddLog(string actionType, string message)
    {
        var log = new ActiveLog
        {
            ActionType = actionType,
            Message = message,
            CreatedAt = DateTime.UtcNow
        };

        _context.Logs.Add(log);
        _context.SaveChanges();
    }
}