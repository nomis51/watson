using Watson.Core.Models.Database;

namespace Watson.Helpers.Abstractions;

public interface ITimeHelper
{
    TimeSpan? ParseTime(string input);
    DateTime? ParseDate(string input);
    bool ParseDateTime(string? timeStr, out DateTime? dateTime);
    TimeSpan GetDuration(List<Frame> frames, TimeSpan dayEndHour);
    string FormatDate(DateTime date, string format = "dddd dd MMMM yyyy");
    string FormatTime(TimeSpan time);
    string FormatDuration(TimeSpan duration);
}