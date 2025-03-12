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

    [InlineData("13", "$year-$month-13")]
    [InlineData("12-13", "$year-12-13")]
    [InlineData("2022-12-13", "2022-12-13")]
    [InlineData("0001-01-01", "0001-01-01")]
    [InlineData("nope", null!)]
    [Theory]
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

    [InlineData("13", "13:00")]
    [InlineData("9", "09:00")]
    [InlineData("98", "09:08")]
    [InlineData("1234", "12:34")]
    [InlineData("945", "09:45")]
    [InlineData("123", "12:03")]
    [InlineData("12:34", "12:34")]
    [InlineData("5:45", "05:45")]
    [InlineData("5:5", "05:05")]
    [InlineData("708", "07:08")]
    [InlineData("078", "07:08")]
    [Theory]
    public void ParseTime_ShouldParseTimeToExpected(string input, string expected)
    {
        // Arrange

        // Act
        var result = _sut.ParseTime(input);

        // Assert
        result.ShouldNotBeNull();
        result.Value.ToString(@"hh\:mm").ShouldBe(expected);
    }

    [InlineData("7:8:9")]
    [InlineData("12345")]
    [InlineData("")]
    [InlineData("nope")]
    [InlineData("1:a")]
    [InlineData("a:1")]
    [InlineData("a")]
    [InlineData("aa")]
    [InlineData("aaa")]
    [InlineData("98a")]
    [InlineData("12aa")]
    [InlineData("aaaaa")]
    [Theory]
    public void ParseTime_ShouldReturnNull_WhenInputIsInvalid(string input)
    {
        // Arrange

        // Act
        var result = _sut.ParseTime(input);

        // Assert
        result.ShouldBeNull();
    }

    [InlineData("13", "$year-$month-$day 13:00")]
    [InlineData("12-13", "$year-12-13 00:00")]
    [InlineData("2022-12-13", "2022-12-13 00:00")]
    [InlineData("13 13", "$year-$month-13 13:00")]
    [InlineData("12-13 14", "$year-12-13 14:00")]
    [InlineData("2022-12-13 14", "2022-12-13 14:00")]
    [InlineData("13 1245", "$year-$month-13 12:45")]
    [InlineData("13 13 13", null)]
    [InlineData("nope", null)]
    [InlineData("", null)]
    [InlineData("2024-02-01 aa", "2024-02-01 00:00")]
    [Theory]
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

    [Fact]
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

    [Fact]
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

    [Fact]
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

    [InlineData(1, 2, "01h 02m")]
    [InlineData(1, 0, "01h 00m")]
    [InlineData(0, 2, "00h 02m")]
    [InlineData(0, 0, "00h 00m")]
    [InlineData(15, 45, "15h 45m")]
    [Theory]
    public void FormatDuration_ShouldReturnFormattedDuration(int hours, int minutes, string expected)
    {
        // Arrange
        var duration = new TimeSpan(hours, minutes, 0);

        // Act
        var result = _sut.FormatDuration(duration);

        // Assert
        result.ShouldBe(expected);
    }

    [InlineData("dddd dd MMMM yyyy", "2022-12-13", "Tuesday 13 December 2022")]
    [InlineData("yyyy-MM-dd", "2022-12-13", "2022-12-13")]
    [Theory]
    public void FormatDate_ShouldReturnFormattedDate(string format, string input, string expected)
    {
        // Arrange
        var date = DateTime.Parse(input);

        // Act
        var result = _sut.FormatDate(date, format);

        // Assert
        result.ShouldBe(expected);
    }

    [InlineData(12, 45, "12:45")]
    [InlineData(0, 45, "00:45")]
    [InlineData(1, 45, "01:45")]
    [InlineData(1, 0, "01:00")]
    [InlineData(0, 0, "00:00")]
    [InlineData(1, 1, "01:01")]
    [InlineData(13, 13, "13:13")]
    [Theory]
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