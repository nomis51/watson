namespace Watson.Helpers.Abstractions;

public interface ITimeHelper
{
    TimeSpan? ParseTime(string input);
    DateTimeOffset? ParseDate(string input);
    bool ParseDateTime(string? timeStr, out DateTimeOffset? dateTime);
}