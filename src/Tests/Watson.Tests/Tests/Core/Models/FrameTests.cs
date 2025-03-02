using Shouldly;
using Watson.Core.Models.Database;

namespace Watson.Tests.Tests.Core.Models;

public class FrameTests
{
    #region Tests

    [Fact]
    public void Ctor_ShouldHaveDateTimeFieldOfTimestamp()
    {
        // Arrange
        var time = DateTime.Now.AddMinutes(-2);

        // Act
        var sut = new Frame
        {
            Time = time.Ticks
        };

        // Assert
        sut.TimeAsDateTime.ShouldBe(time);
    }

    [Fact]
    public void Ctor_ShouldHaveNowAsDefaultTimestamp()
    {
        // Arrange
        const int gracePeriod = 3;

        // Act
        var sut = new Frame();

        // Assert
        sut.Time.ShouldBeGreaterThan(0);
        sut.Time.ShouldBeGreaterThanOrEqualTo(DateTime.Now.AddSeconds(-gracePeriod).Ticks);
        sut.Time.ShouldBeLessThanOrEqualTo(DateTime.Now.AddSeconds(gracePeriod).Ticks);
    }

    [Fact]
    public void CreateEmptyFrame_ShouldCreateEmptyFrame_WithProvidedTimestamp()
    {
        // Arrange
        var time = DateTime.Now.AddMinutes(-2).Ticks;

        // Act
        var sut = Frame.CreateEmpty(time);

        // Assert
        sut.ProjectId.ShouldBeEmpty();
        sut.Id.ShouldBeNull();
        sut.Time.ShouldBe(time);
    }

    #endregion
}