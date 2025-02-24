using Shouldly;
using Watson.Core.Models;

namespace Watson.Tests.Core.Models;

public class FrameTests
{
    #region Tests

    [Fact]
    public void Ctor_ShouldHaveDateTimeFieldOfTimestamp()
    {
        // Arrange
        var time = DateTimeOffset.UtcNow.AddMinutes(-2);

        // Act
        var sut = new Frame
        {
            Timestamp = time.ToUnixTimeSeconds()
        };

        // Assert
        sut.TimestampAsDateTime.ToString("yyyy-MM-dd HH:mm").ShouldBe(time.ToString("yyyy-MM-dd HH:mm"));
    }

    [Fact]
    public void Ctor_ShouldHaveNowAsDefaultTimestamp()
    {
        // Arrange
        const int gracePeriod = 5;

        // Act
        var sut = new Frame();

        // Assert
        sut.Timestamp.ShouldBeGreaterThan(0);
        sut.Timestamp.ShouldSatisfyAllConditions(
            e => e.ShouldBeGreaterThanOrEqualTo(DateTimeOffset.UtcNow.ToUnixTimeSeconds()),
            e => e.ShouldBeLessThanOrEqualTo(
                new DateTimeOffset(DateTime.UtcNow.AddSeconds(gracePeriod)).ToUnixTimeSeconds())
        );
    }

    [Fact]
    public void CreateEmptyFrame_ShouldCreateEmptyFrame_WithProvidedTimestamp()
    {
        // Arrange
        var time = DateTimeOffset.UtcNow.AddMinutes(-2).ToUnixTimeSeconds();

        // Act
        var sut = Frame.CreateEmpty(time);

        // Assert
        sut.ProjectId.ShouldBeEmpty();
        sut.Id.ShouldBeNull();
        sut.Timestamp.ShouldBe(time);
    }

    #endregion
}