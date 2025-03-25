using System.Globalization;
using Shouldly;
using Watson.Core.Models.Database;
using Watson.Helpers;

namespace Watson.Tests.Tests.Helpers;

public class TimeHelperTests
{
    #region Members

    private readonly TimeHelper _sut = new();

    #endregion

    #region Tests

    [Arguments("13", "$year-$month-13")]
    [Arguments("12-13", "$year-12-13")]
    [Arguments("2022-12-13", "2022-12-13")]
    [Arguments("0001-01-01", "0001-01-01")]
    [Arguments("nope", null!)]
    [Test]
    public void ParseDate_ShouldParseDateToExpected(string input, string? expected)
    {
        // Arrange
        var now = DateTime.Now;
        expected = expected?.Replace("$year", now.Year.ToString())
            .Replace("$month", now.Month.ToString().PadLeft(2, '0'))
            .Replace("$day", now.Day.ToString().PadLeft(2, '0')) ?? expected;

        // Act
        var result = _sut.ParseDate(input);

        // Assert
        if (expected is null)
        {
            result.ShouldBeNull();
        }
        else
        {
            result.ShouldNotBeNull();
            result.Value.ToString("yyyy-MM-dd").ShouldBe(expected);
        }
    }

    [Arguments("13", "13:00")]
    [Arguments("9", "09:00")]
    [Arguments("98", "09:08")]
    [Arguments("1234", "12:34")]
    [Arguments("945", "09:45")]
    [Arguments("123", "12:03")]
    [Arguments("12:34", "12:34")]
    [Arguments("5:45", "05:45")]
    [Arguments("5:5", "05:05")]
    [Arguments("708", "07:08")]
    [Arguments("078", "07:08")]
    [Test]
    public void ParseTime_ShouldParseTimeToExpected(string input, string expected)
    {
        // Arrange

        // Act
        var result = _sut.ParseTime(input);

        // Assert
        result.ShouldNotBeNull();
        result.Value.ToString(@"hh\:mm").ShouldBe(expected);
    }

    [Arguments("7:8:9")]
    [Arguments("12345")]
    [Arguments("")]
    [Arguments("nope")]
    [Arguments("1:a")]
    [Arguments("a:1")]
    [Arguments("a")]
    [Arguments("aa")]
    [Arguments("aaa")]
    [Arguments("98a")]
    [Arguments("12aa")]
    [Arguments("aaaaa")]
    [Test]
    public void ParseTime_ShouldReturnNull_WhenInputIsInvalid(string input)
    {
        // Arrange

        // Act
        var result = _sut.ParseTime(input);

        // Assert
        result.ShouldBeNull();
    }

    [Arguments("13", "$year-$month-$day 13:00")]
    [Arguments("12-13", "$year-12-13 00:00")]
    [Arguments("2022-12-13", "2022-12-13 00:00")]
    [Arguments("13 13", "$year-$month-13 13:00")]
    [Arguments("12-13 14", "$year-12-13 14:00")]
    [Arguments("2022-12-13 14", "2022-12-13 14:00")]
    [Arguments("13 1245", "$year-$month-13 12:45")]
    [Arguments("13 13 13", null)]
    [Arguments("nope", null)]
    [Arguments("", null)]
    [Arguments("2024-02-01 aa", "2024-02-01 00:00")]
    [Test]
    public void ParseDateTime_ShouldParseDateTimeToExpected(string? input, string? expected)
    {
        // Arrange
        var now = DateTime.Now;
        expected = expected?.Replace("$year", now.Year.ToString())
            .Replace("$month", now.Month.ToString().PadLeft(2, '0'))
            .Replace("$day", now.Day.ToString().PadLeft(2, '0'))
            .Replace("$second", now.Second.ToString().PadLeft(2, '0')) ?? expected;

        // Act
        var result = _sut.ParseDateTime(input, out var dateTime);

        // Assert
        if (expected is null)
        {
            result.ShouldBeFalse();
            dateTime.ShouldBeNull();
        }
        else
        {
            result.ShouldBeTrue();
            dateTime.ShouldNotBeNull();
            dateTime.Value.ToString("yyyy-MM-dd HH:mm").ShouldBe(expected);
        }
    }

    [Test]
    public void GetDuration_ShouldReturnDurationOfFrames()
    {
        // Arrange
        List<Frame> frames =
        [
            new()
            {
                Time = new DateTime(2025, 1, 1, 7, 0, 0).Ticks,
                ProjectId = "id"
            },
            new()
            {
                Time = new DateTime(2025, 1, 1, 7, 23, 0).Ticks,
                ProjectId = "id"
            },
            new()
            {
                Time = new DateTime(2025, 1, 1, 7, 45, 0).Ticks,
                ProjectId = "id"
            },
        ];
        var dayEndHour = new TimeSpan(8, 0, 0);

        // Act
        var result = _sut.GetDuration(frames, dayEndHour);

        // Assert
        result.Minutes.ShouldBe(0);
        result.Hours.ShouldBe(1);
    }

    [Test]
    public void GetDuration_ShouldReturnZero_WhenFramesAreEmpty()
    {
        // Arrange
        List<Frame> frames = new();
        var dayEndHour = new TimeSpan(8, 0, 0);

        // Act
        var result = _sut.GetDuration(frames, dayEndHour);

        // Assert
        result.Minutes.ShouldBe(0);
        result.Hours.ShouldBe(0);
    }

    [Test]
    public void GetDuration_ShouldReturnZero_WhenFramesAreAllEmpty()
    {
        // Arrange
        List<Frame> frames =
        [
            new()
            {
                Time = new DateTime(2025, 1, 1, 7, 0, 0).Ticks,
                ProjectId = ""
            },
            new()
            {
                Time = new DateTime(2025, 1, 1, 7, 23, 0).Ticks,
                ProjectId = ""
            },
            new()
            {
                Time = new DateTime(2025, 1, 1, 7, 45, 0).Ticks,
                ProjectId = ""
            },
        ];
        var dayEndHour = new TimeSpan(8, 0, 0);

        // Act
        var result = _sut.GetDuration(frames, dayEndHour);

        // Assert
        result.Minutes.ShouldBe(0);
        result.Hours.ShouldBe(0);
    }

    [Arguments(1, 2, "01h 02m")]
    [Arguments(1, 0, "01h 00m")]
    [Arguments(0, 2, "00h 02m")]
    [Arguments(0, 0, "00h 00m")]
    [Arguments(15, 45, "15h 45m")]
    [Test]
    public void FormatDuration_ShouldReturnFormattedDuration(int hours, int minutes, string expected)
    {
        // Arrange
        var duration = new TimeSpan(hours, minutes, 0);

        // Act
        var result = _sut.FormatDuration(duration);

        // Assert
        result.ShouldBe(expected);
    }

    [Arguments("dddd dd MMMM yyyy", "2022-12-13", "Tuesday 13 December 2022")]
    [Arguments("yyyy-MM-dd", "2022-12-13", "2022-12-13")]
    [Test]
    public void FormatDate_ShouldReturnFormattedDate(string format, string input, string expected)
    {
        // Arrange
        var date = DateTime.Parse(input);

        // Act
        var result = _sut.FormatDate(date, format, CultureInfo.InvariantCulture);

        // Assert
        result.ShouldBe(expected);
    }

    [Arguments(12, 45, "12:45")]
    [Arguments(0, 45, "00:45")]
    [Arguments(1, 45, "01:45")]
    [Arguments(1, 0, "01:00")]
    [Arguments(0, 0, "00:00")]
    [Arguments(1, 1, "01:01")]
    [Arguments(13, 13, "13:13")]
    [Test]
    public void FormatTime_ShouldReturnFormattedTime(int hours, int minutes, string expected)
    {
        // Arrange
        var time = new TimeSpan(hours, minutes, 0);

        // Act
        var result = _sut.FormatTime(time);

        // Assert
        result.ShouldBe(expected);
    }

    #endregion
}