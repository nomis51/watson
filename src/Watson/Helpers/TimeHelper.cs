using Watson.Helpers.Abstractions;

namespace Watson.Helpers;

public class TimeHelper : ITimeHelper
{
    #region Public methods

    public bool ParseDateTime(string? timeStr, out DateTimeOffset? dateTime)
    {
        if (timeStr is null)
        {
            dateTime = null;
            return true;
        }

        if (string.IsNullOrEmpty(timeStr))
        {
            dateTime = null;
            return false;
        }

        var parts = timeStr.Split(' ', StringSplitOptions.TrimEntries | StringSplitOptions.RemoveEmptyEntries);
        if (parts.Length != 1 && parts.Length != 2)
        {
            dateTime = null;
            return false;
        }

        var datePart = parts.Length == 2 ? parts[0] : null;
        var timePart = parts.Length == 2 ? parts[1] : parts[0];

        var date = datePart is not null ? ParseDate(datePart) : null;
        var time = ParseTime(timePart);

        if (date is not null && time is not null)
        {
            dateTime = date.Value.Add(time.Value);
            return true;
        }

        if (time is not null)
        {
            dateTime = DateTimeOffset.UtcNow.Date.Add(time.Value);
            return true;
        }

        dateTime = null;
        return false;
    }

    public TimeSpan? ParseTime(string input)
    {
        int hour;
        int minute;

        // colon-separated time
        if (input.Contains(':'))
        {
            var timeParts = input.Split(':');

            if (timeParts.Length != 2) return null;

            if (!int.TryParse(timeParts[0], out hour) || !int.TryParse(timeParts[1], out minute)) return null;

            hour = Math.Clamp(hour, 0, 23);
            minute = Math.Clamp(minute, 0, 59);
        }
        else
        {
            switch (input.Length)
            {
                // Case 1: Single digit "9" -> 09:00
                case 1:
                    if (!int.TryParse(input, out hour)) return null;
                    minute = 0;
                    break;
                // Case 2: Two digits "13" -> 13:00, "89" -> 08:09
                case 2:
                {
                    if (!int.TryParse(input, out hour)) return null;
                    if (hour < 24)
                    {
                        minute = 0;
                    }
                    else
                    {
                        if (!int.TryParse(input[0].ToString(), out hour)) return null;
                        if (!int.TryParse(input[1].ToString(), out minute)) return null;
                    }

                    break;
                }
                // Case 3: Three digits "078" -> 07:08, "123" -> 12:03, "945" -> 09:45
                case 3:
                {
                    if (!int.TryParse(input[..2], out hour)) return null;
                    if (!int.TryParse(input[2..], out minute)) return null;

                    if (hour >= 24)
                    {
                        if (!int.TryParse(input[0].ToString(), out hour)) return null;
                        if (!int.TryParse(input[1..], out minute)) return null;
                    }

                    break;
                }
                // Case 4: Four digits "1234" -> 12:34, "0435" -> 04:35
                case 4:
                    if (!int.TryParse(input[..2], out hour)) return null;
                    if (!int.TryParse(input[2..], out minute)) return null;
                    break;

                default:
                    return null;
            }

            hour = Math.Clamp(hour, 0, 23);
            minute = Math.Clamp(minute, 0, 59);
        }

        if (hour == 0 && minute == 0) return null;

        return new TimeSpan(hour, minute, 0);
    }


    public DateTimeOffset? ParseDate(string input)
    {
        // only day
        if (int.TryParse(input, out var day))
        {
            return new DateTimeOffset(new DateTime(DateTimeOffset.UtcNow.Year, DateTimeOffset.UtcNow.Month, day));
        }

        // month and day
        var parts = input.Split('-');
        if (parts.Length == 2 && int.TryParse(parts[0], out var month) && int.TryParse(parts[1], out day))
        {
            return new DateTimeOffset(new DateTime(DateTimeOffset.UtcNow.Year, month, day));
        }

        // year, month and day
        if (parts.Length == 3 && int.TryParse(parts[0], out var year) &&
            int.TryParse(parts[1], out month) && int.TryParse(parts[2], out day))
        {
            return new DateTimeOffset(new DateTime(year, month, day));
        }

        return null;
    }

    #endregion
}