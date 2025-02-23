﻿using Watson.Helpers.Abstractions;

namespace Watson.Helpers;

public class TimeHelper : ITimeHelper
{
    #region Public methods

    public bool ParseDateTime(string? timeStr, out DateTimeOffset? dateTime)
    {
        if (string.IsNullOrEmpty(timeStr))
        {
            dateTime = null;
            return true;
        }

        var parts = timeStr.Split(' ', StringSplitOptions.TrimEntries | StringSplitOptions.RemoveEmptyEntries);
        if (parts.Length == 0)
        {
            dateTime = null;
            return false;
        }

        var datePartIndex = parts.ToList().FindIndex(e => e.Contains('-'));
        var datePart = datePartIndex == -1 ? parts.Length == 2 ? parts[0] : null : parts[datePartIndex];
        var timePart = datePartIndex != -1 && parts.Length == 1 ? null : parts[datePartIndex == 0 ? 1 : 0];

        var date = datePart is not null ? ParseDate(datePart) : null;
        var time = timePart is not null ? ParseTime(timePart) : null;

        if (date is not null && time is not null)
        {
            dateTime = date.Value.Add(time.Value);
            return true;
        }

        if (date is not null)
        {
            dateTime = date.Value;
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
        int hour = 0, minute = 0;

        // colon-separated time
        if (input.Contains(':'))
        {
            var timeParts = input.Split(':');

            if (timeParts.Length != 2) return null;

            hour = int.Parse(timeParts[0]);
            minute = int.Parse(timeParts[1]);

            hour = Math.Clamp(hour, 0, 23);
            minute = Math.Clamp(minute, 0, 59);
        }
        else
        {
            switch (input.Length)
            {
                // Case 1: Single digit "9" -> 09:00
                case 1:
                    hour = int.Parse(input);
                    minute = 0;
                    break;
                // Case 2: Two digits "13" -> 13:00, "89" -> 08:09
                case 2:
                {
                    hour = int.Parse(input);
                    if (hour < 24)
                    {
                        minute = 0; // Valid hour, set minutes to 0
                    }
                    else
                    {
                        hour = int.Parse(input[0].ToString());
                        minute = int.Parse(input[1].ToString()) * 10;
                    }

                    break;
                }
                // Case 3: Three digits "078" -> 07:08, "123" -> 12:03, "945" -> 09:45
                case 3:
                {
                    hour = int.Parse(input[..2]);
                    minute = int.Parse(input[2..]);

                    if (hour >= 24)
                    {
                        hour = int.Parse(input[0].ToString());
                        minute = int.Parse(input[1..]);
                    }

                    break;
                }
                // Case 4: Four digits "1234" -> 12:34, "0435" -> 04:35
                case 4:
                    hour = int.Parse(input[..2]);
                    minute = int.Parse(input[2..]);
                    break;
            }

            hour = Math.Clamp(hour, 0, 23);
            minute = Math.Clamp(minute, 0, 59);
        }

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