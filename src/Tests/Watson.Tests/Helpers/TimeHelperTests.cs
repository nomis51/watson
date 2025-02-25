using System.Runtime.InteropServices;
using Shouldly;
using Watson.Helpers;

namespace Watson.Tests.Helpers;

public class TimeHelperTests
{
    #region Members

    private readonly TimeHelper _sut = new();

    #endregion

    #region Tests

    [InlineData("13", "$year-$month-13")]
    [InlineData("12-13", "$year-12-13")]
    [InlineData("2022-12-13", "2022-12-13")]
    [InlineData("nope", null!)]
    [Theory]
    public void ParseDate_ShouldParseDateToExpected(string input, string? expected)
    {
        // Arrange
        var now = DateTimeOffset.UtcNow;
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
    [InlineData("12-13", null)]
    [InlineData("2022-12-13", null)]
    [InlineData("13 13", "$year-$month-13 13:00")]
    [InlineData("12-13 14", "$year-12-13 14:00")]
    [InlineData("2022-12-13 14", "2022-12-13 14:00")]
    [InlineData("13 1245", "$year-$month-13 12:45")]
    [InlineData("13 13 13", null)]
    [InlineData("nope", null)]
    [InlineData("", null)]
    [InlineData(null, null)]
    [InlineData("2024-02-01 aa", null)]
    [Theory]
    public void ParseDateTime_ShouldParseDateTimeToExpected(string? input, string? expected)
    {
        // Arrange
        var now = DateTimeOffset.UtcNow;
        expected = expected?.Replace("$year", now.Year.ToString())
            .Replace("$month", now.Month.ToString().PadLeft(2, '0'))
            .Replace("$day", now.Day.ToString().PadLeft(2, '0'))
            .Replace("$second", now.Second.ToString().PadLeft(2, '0')) ?? expected;

        // Act
        var result = _sut.ParseDateTime(input, out var dateTimeOffset);

        // Assert
        if (expected is null)
        {
            result.ShouldBeFalse();
            dateTimeOffset.ShouldBeNull();
        }
        else
        {
            result.ShouldBeTrue();
            dateTimeOffset.ShouldNotBeNull();
            dateTimeOffset.Value.ToString("yyyy-MM-dd HH:mm").ShouldBe(expected);
        }
    }

    #endregion
}