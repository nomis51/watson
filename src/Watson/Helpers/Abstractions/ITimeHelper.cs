using Watson.Core.Models;

namespace Watson.Helpers.Abstractions;

public interface ITimeHelper
{
    TimeSpan? ParseTime(string input);
    DateTimeOffset? ParseDate(string input);
    bool ParseDateTime(string? timeStr, out DateTimeOffset? dateTime);
    TimeSpan GetDuration(List<Frame> frames, TimeSpan dayEndHour);
    string FormatDate(DateTime date, string format = "dddd dd MMMM yyyy");
    string FormatTime(TimeSpan time);
    string FormatDuration(TimeSpan duration);
}