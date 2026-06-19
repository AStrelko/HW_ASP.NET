namespace HW_06.Services;

public class MeetingFilter
{
    public int Page { get; set; } = 1;

    public int PageSize { get; set; } = 10;

    public string? Search { get; set; }

    public string? SortBy { get; set; }

    public DateTime? StartTime { get; set; }

    public DateTime? EndTime { get; set; }
}