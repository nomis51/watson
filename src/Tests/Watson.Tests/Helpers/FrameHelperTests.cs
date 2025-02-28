using System.Reflection;
using NSubstitute;
using Shouldly;
using Watson.Core.Models;
using Watson.Core.Repositories.Abstractions;
using Watson.Helpers;

namespace Watson.Tests.Helpers;

public class FrameHelperTests
{
    #region Members

    private readonly IFrameRepository _frameRepository = Substitute.For<IFrameRepository>();
    private readonly FrameHelper _sut;

    #endregion

    #region Constructors

    public FrameHelperTests()
    {
        _frameRepository.InsertAsync(Arg.Any<Frame>())
            .Returns(true);

        _frameRepository.UpdateAsync(Arg.Any<Frame>())
            .Returns(true);

        _frameRepository.DeleteManyAsync(Arg.Any<IEnumerable<string>>())
            .Returns(true);

        _sut = new FrameHelper(_frameRepository);
    }

    #endregion

    #region Tests

    [Fact]
    public async Task CreateFrameAtTheBeginningOfTheDay_ShouldReturnTrue()
    {
        // Arrange
        var frame = new Frame { Id = "id" };
        var toTimeNextFrame = new Frame
        {
            Id = "id2",
            Timestamp = 0,
            ProjectId = "to time next frame"
        };
        var toTime = DateTimeOffset.Now.AddSeconds(-1);
        var sut = _sut.GetType()
            .GetMethod("CreateFrameAtTheBeginningOfTheDay", BindingFlags.NonPublic | BindingFlags.Instance);
        sut.ShouldNotBeNull();

        // Act
        var result = await (Task<bool>)sut.Invoke(_sut, [frame, toTime, toTimeNextFrame])!;

        // Assert
        result.ShouldBeTrue();
        await _frameRepository.Received()
            .InsertAsync(frame);
        await _frameRepository.Received()
            .UpdateAsync(Arg.Is<Frame>(f =>
                f.Id == toTimeNextFrame.Id &&
                f.Timestamp == toTime.ToUnixTimeSeconds() &&
                f.ProjectId == "to time next frame"
            ));
    }

    [Fact]
    public async Task CreateFrameAtTheBeginningOfTheDay_ShouldReturnFalse_WhenInsertFails()
    {
        // Arrange
        var frame = new Frame { Id = "id" };
        var toTimeNextFrame = new Frame
        {
            Id = "id2",
            Timestamp = 0,
        };
        var toTime = DateTimeOffset.Now.AddSeconds(-1);
        var sut = _sut.GetType()
            .GetMethod("CreateFrameAtTheBeginningOfTheDay", BindingFlags.NonPublic | BindingFlags.Instance);
        sut.ShouldNotBeNull();

        _frameRepository.InsertAsync(frame)
            .Returns(false);

        // Act
        var result = await (Task<bool>)sut.Invoke(_sut, [frame, toTime, toTimeNextFrame])!;

        // Assert
        result.ShouldBeFalse();
    }

    [Fact]
    public async Task CreateFrameAtTheBeginningOfTheDay_ShouldReturnFalse_WhenUpdateFails()
    {
        // Arrange
        var frame = new Frame { Id = "id" };
        var toTimeNextFrame = new Frame
        {
            Id = "id2",
            Timestamp = 0,
        };
        var toTime = DateTimeOffset.Now.AddSeconds(-1);
        var sut = _sut.GetType()
            .GetMethod("CreateFrameAtTheBeginningOfTheDay", BindingFlags.NonPublic | BindingFlags.Instance);
        sut.ShouldNotBeNull();

        _frameRepository.UpdateAsync(toTimeNextFrame)
            .Returns(false);

        // Act
        var result = await (Task<bool>)sut.Invoke(_sut, [frame, toTime, toTimeNextFrame])!;

        // Assert
        result.ShouldBeFalse();
    }

    [Fact]
    public async Task CreateFrameContainedInAFrame_ShouldReturnTrue()
    {
        // Arrange
        var frame = new Frame { Id = "id" };
        var toTime = DateTimeOffset.Now.AddSeconds(-1);
        var fromTimePreviousFrame = new Frame
        {
            Id = "id2",
            Timestamp = 0,
            ProjectId = "from time previous frame"
        };
        var sut = _sut.GetType()
            .GetMethod("CreateFrameContainedInAFrame", BindingFlags.NonPublic | BindingFlags.Instance);
        sut.ShouldNotBeNull();

        // Act
        var result = await (Task<bool>)sut.Invoke(_sut, [frame, toTime, fromTimePreviousFrame])!;

        // Assert
        result.ShouldBeTrue();
        await _frameRepository.Received()
            .InsertAsync(frame);
        await _frameRepository.Received()
            .InsertAsync(Arg.Is<Frame>(f =>
                f.Id == string.Empty &&
                f.Timestamp == toTime.ToUnixTimeSeconds() &&
                f.ProjectId == "from time previous frame"
            ));
    }

    [Fact]
    public async Task CreateFrameContainedInAFrame_ShouldReturnFalse_WhenInsertFails()
    {
        // Arrange
        var frame = new Frame { Id = "id" };
        var toTime = DateTimeOffset.Now.AddSeconds(-1);
        var fromTimePreviousFrame = new Frame
        {
            Id = "id2",
            Timestamp = 0,
        };
        var sut = _sut.GetType()
            .GetMethod("CreateFrameContainedInAFrame", BindingFlags.NonPublic | BindingFlags.Instance);
        sut.ShouldNotBeNull();

        _frameRepository.InsertAsync(frame)
            .Returns(false);

        // Act
        var result = await (Task<bool>)sut.Invoke(_sut, [frame, toTime, fromTimePreviousFrame])!;

        // Assert
        result.ShouldBeFalse();
    }

    [Fact]
    public async Task CreateFrameContainedInAFrame_ShouldReturnFalse_WhenPreviousInsertFails()
    {
        // Arrange
        var frame = new Frame { Id = "id" };
        var toTime = DateTimeOffset.Now.AddSeconds(-1);
        var fromTimePreviousFrame = new Frame
        {
            Id = "id2",
            Timestamp = 0,
            ProjectId = "from time previous frame"
        };
        var sut = _sut.GetType()
            .GetMethod("CreateFrameContainedInAFrame", BindingFlags.NonPublic | BindingFlags.Instance);
        sut.ShouldNotBeNull();

        _frameRepository.InsertAsync(Arg.Is<Frame>(f => f.ProjectId == "from time previous frame"))
            .Returns(false);

        // Act
        var result = await (Task<bool>)sut.Invoke(_sut, [frame, toTime, fromTimePreviousFrame])!;

        // Assert
        result.ShouldBeFalse();
    }

    [Fact]
    public async Task CreateFrameOverTwoFrames_ShouldReturnTrue()
    {
        // Arrange
        var frame = new Frame { Id = "id" };
        var toTime = DateTimeOffset.Now.AddSeconds(-1);
        var toTimePreviousFrame = new Frame
        {
            Id = "id2",
            Timestamp = 0,
            ProjectId = "to time previous frame"
        };
        var sut = _sut.GetType()
            .GetMethod("CreateFrameOverTwoFrames", BindingFlags.NonPublic | BindingFlags.Instance);
        sut.ShouldNotBeNull();

        // Act
        var result = await (Task<bool>)sut.Invoke(_sut, [frame, toTime, toTimePreviousFrame])!;

        // Assert
        result.ShouldBeTrue();
        await _frameRepository.Received()
            .InsertAsync(frame);
        await _frameRepository.Received()
            .UpdateAsync(Arg.Is<Frame>(f =>
                f.Id == "id2" &&
                f.Timestamp == toTime.ToUnixTimeSeconds() &&
                f.ProjectId == "to time previous frame"
            ));
    }

    [Fact]
    public async Task CreateFrameOverTwoFrames_ShouldReturnFalse_WhenInsertFails()
    {
        // Arrange
        var frame = new Frame { Id = "id" };
        var toTime = DateTimeOffset.Now.AddSeconds(-1);
        var toTimePreviousFrame = new Frame
        {
            Id = "id2",
            Timestamp = 0,
            ProjectId = "to time previous frame"
        };
        var sut = _sut.GetType()
            .GetMethod("CreateFrameOverTwoFrames", BindingFlags.NonPublic | BindingFlags.Instance);
        sut.ShouldNotBeNull();

        _frameRepository.InsertAsync(frame)
            .Returns(false);

        // Act
        var result = await (Task<bool>)sut.Invoke(_sut, [frame, toTime, toTimePreviousFrame])!;

        // Assert
        result.ShouldBeFalse();
    }

    [Fact]
    public async Task CreateFrameOverTwoFrames_ShouldReturnFalse_WhenUpdateFails()
    {
        // Arrange
        var frame = new Frame { Id = "id" };
        var toTime = DateTimeOffset.Now.AddSeconds(-1);
        var toTimePreviousFrame = new Frame
        {
            Id = "id2",
            Timestamp = 0,
            ProjectId = "to time previous frame"
        };
        var sut = _sut.GetType()
            .GetMethod("CreateFrameOverTwoFrames", BindingFlags.NonPublic | BindingFlags.Instance);
        sut.ShouldNotBeNull();

        _frameRepository.UpdateAsync(Arg.Is<Frame>(f => f.ProjectId == "to time previous frame"))
            .Returns(false);

        // Act
        var result = await (Task<bool>)sut.Invoke(_sut, [frame, toTime, toTimePreviousFrame])!;

        // Assert
        result.ShouldBeFalse();
    }

    [Fact]
    public async Task CreateFrameOverMultipleFrames_ShouldReturnTrue()
    {
        // Arrange
        var frame = new Frame { Id = "id" };
        var toTime = DateTimeOffset.Now.AddSeconds(-1);
        var toTimePreviousFrame = new Frame
        {
            Id = "id2",
            Timestamp = 2,
            ProjectId = "to time previous frame"
        };
        var toTimeNextFrame = new Frame
        {
            Id = "id3",
            Timestamp = 3,
            ProjectId = "to time next frame"
        };
        var framesToDelete = new[]
        {
            new Frame
            {
                Id = "id4"
            },
            new Frame
            {
                Id = "id5"
            }
        };
        var sut = _sut.GetType()
            .GetMethod("CreateFrameOverMultipleFrames", BindingFlags.NonPublic | BindingFlags.Instance);
        sut.ShouldNotBeNull();

        _frameRepository.GetAsync(Arg.Any<DateTimeOffset>(), Arg.Any<DateTimeOffset>())
            .Returns(framesToDelete);

        // Act
        var result = await (Task<bool>)sut.Invoke(_sut, [frame, toTime, toTimeNextFrame, toTimePreviousFrame])!;

        // Assert
        result.ShouldBeTrue();
        await _frameRepository.Received()
            .GetAsync(
                Arg.Is<DateTimeOffset>(d =>
                    d.ToUnixTimeSeconds() == 3
                ),
                Arg.Is<DateTimeOffset>(d =>
                    d.ToUnixTimeSeconds() == 2
                )
            );
        await _frameRepository.Received()
            .DeleteManyAsync(Arg.Is<IEnumerable<string>>(i =>
                i.All(t => t == "id4" || t == "id5")
            ));
        await _frameRepository.Received()
            .InsertAsync(frame);
        await _frameRepository.Received()
            .UpdateAsync(Arg.Is<Frame>(f =>
                f.Id == "id3" &&
                f.Timestamp == toTime.ToUnixTimeSeconds() &&
                f.ProjectId == "to time next frame"
            ));
    }

    [Fact]
    public async Task CreateFrameOverMultipleFrames_ShouldReturnFalse_WhenDeleteFails()
    {
        // Arrange
        var frame = new Frame { Id = "id" };
        var toTime = DateTimeOffset.Now.AddSeconds(-1);
        var toTimePreviousFrame = new Frame
        {
            Id = "id2",
            Timestamp = 2,
            ProjectId = "to time previous frame"
        };
        var toTimeNextFrame = new Frame
        {
            Id = "id3",
            Timestamp = 3,
            ProjectId = "to time next frame"
        };
        var framesToDelete = new[]
        {
            new Frame
            {
                Id = "id4"
            },
            new Frame
            {
                Id = "id5"
            }
        };
        var sut = _sut.GetType()
            .GetMethod("CreateFrameOverMultipleFrames", BindingFlags.NonPublic | BindingFlags.Instance);
        sut.ShouldNotBeNull();

        _frameRepository.GetAsync(Arg.Any<DateTimeOffset>(), Arg.Any<DateTimeOffset>())
            .Returns(framesToDelete);

        _frameRepository.DeleteManyAsync(Arg.Any<IEnumerable<string>>())
            .Returns(false);

        // Act
        var result = await (Task<bool>)sut.Invoke(_sut, [frame, toTime, toTimeNextFrame, toTimePreviousFrame])!;

        // Assert
        result.ShouldBeFalse();
    }

    [Fact]
    public async Task CreateFrameOverMultipleFrames_ShouldReturnFalse_WhenInsertFails()
    {
        // Arrange
        var frame = new Frame { Id = "id" };
        var toTime = DateTimeOffset.Now.AddSeconds(-1);
        var toTimePreviousFrame = new Frame
        {
            Id = "id2",
            Timestamp = 2,
            ProjectId = "to time previous frame"
        };
        var toTimeNextFrame = new Frame
        {
            Id = "id3",
            Timestamp = 3,
            ProjectId = "to time next frame"
        };
        var framesToDelete = new[]
        {
            new Frame
            {
                Id = "id4"
            },
            new Frame
            {
                Id = "id5"
            }
        };
        var sut = _sut.GetType()
            .GetMethod("CreateFrameOverMultipleFrames", BindingFlags.NonPublic | BindingFlags.Instance);
        sut.ShouldNotBeNull();

        _frameRepository.GetAsync(Arg.Any<DateTimeOffset>(), Arg.Any<DateTimeOffset>())
            .Returns(framesToDelete);

        _frameRepository.InsertAsync(Arg.Any<Frame>())
            .Returns(false);

        // Act
        var result = await (Task<bool>)sut.Invoke(_sut, [frame, toTime, toTimeNextFrame, toTimePreviousFrame])!;

        // Assert
        result.ShouldBeFalse();
    }

    [Fact]
    public async Task CreateFrameOverMultipleFrames_ShouldReturnFalse_WhenUpdateFails()
    {
        // Arrange
        var frame = new Frame { Id = "id" };
        var toTime = DateTimeOffset.Now.AddSeconds(-1);
        var toTimePreviousFrame = new Frame
        {
            Id = "id2",
            Timestamp = 2,
            ProjectId = "to time previous frame"
        };
        var toTimeNextFrame = new Frame
        {
            Id = "id3",
            Timestamp = 3,
            ProjectId = "to time next frame"
        };
        var framesToDelete = new[]
        {
            new Frame
            {
                Id = "id4"
            },
            new Frame
            {
                Id = "id5"
            }
        };
        var sut = _sut.GetType()
            .GetMethod("CreateFrameOverMultipleFrames", BindingFlags.NonPublic | BindingFlags.Instance);
        sut.ShouldNotBeNull();

        _frameRepository.GetAsync(Arg.Any<DateTimeOffset>(), Arg.Any<DateTimeOffset>())
            .Returns(framesToDelete);

        _frameRepository.UpdateAsync(Arg.Any<Frame>())
            .Returns(false);

        // Act
        var result = await (Task<bool>)sut.Invoke(_sut, [frame, toTime, toTimeNextFrame, toTimePreviousFrame])!;

        // Assert
        result.ShouldBeFalse();
    }

    [Fact]
    public async Task CreateFrameOverMultipleFrames_ShouldReturnTrue_WithNoFrameDeleted_WhenNothingOverlap()
    {
        // Arrange
        var frame = new Frame { Id = "id" };
        var toTime = DateTimeOffset.Now.AddSeconds(-1);
        var toTimePreviousFrame = new Frame
        {
            Id = "id2",
            Timestamp = 2,
            ProjectId = "to time previous frame"
        };
        var toTimeNextFrame = new Frame
        {
            Id = "id3",
            Timestamp = 3,
            ProjectId = "to time next frame"
        };
        var sut = _sut.GetType()
            .GetMethod("CreateFrameOverMultipleFrames", BindingFlags.NonPublic | BindingFlags.Instance);
        sut.ShouldNotBeNull();

        _frameRepository.GetAsync(Arg.Any<DateTimeOffset>(), Arg.Any<DateTimeOffset>())
            .Returns([]);

        // Act
        var result = await (Task<bool>)sut.Invoke(_sut, [frame, toTime, toTimeNextFrame, toTimePreviousFrame])!;

        // Assert
        result.ShouldBeTrue();
        await _frameRepository.Received()
            .GetAsync(
                Arg.Is<DateTimeOffset>(d =>
                    d.ToUnixTimeSeconds() == 3
                ),
                Arg.Is<DateTimeOffset>(d =>
                    d.ToUnixTimeSeconds() == 2
                )
            );
        await _frameRepository.Received(0)
            .DeleteManyAsync(Arg.Any<IEnumerable<string>>());
        await _frameRepository.Received()
            .InsertAsync(frame);
        await _frameRepository.Received()
            .UpdateAsync(Arg.Is<Frame>(f =>
                f.Id == "id3" &&
                f.Timestamp == toTime.ToUnixTimeSeconds() &&
                f.ProjectId == "to time next frame"
            ));
    }

    [Fact]
    public async Task CreateFrame_ShouldReturnTrue_WhenToTimeIsNull()
    {
        // Arrange
        var frame = new Frame { Id = "id" };

        // Act
        var result = await _sut.CreateFrame(frame);

        // Assert
        result.ShouldBeTrue();
        await _frameRepository.Received()
            .InsertAsync(frame);
        await _frameRepository.Received(0)
            .GetNextFrameAsync(Arg.Any<DateTimeOffset>());
    }

    [Fact]
    public async Task CreateFrame_ShouldReturnTrue_WhenNoNextFrame()
    {
        // Arrange
        var toTime = DateTimeOffset.Now.AddSeconds(-1);
        var frame = new Frame { Id = "id" };

        // Act
        var result = await _sut.CreateFrame(frame, toTime);

        // Assert
        result.ShouldBeTrue();
        await _frameRepository.Received()
            .InsertAsync(frame);
        await _frameRepository.Received()
            .GetNextFrameAsync(toTime);
        await _frameRepository.Received(0)
            .GetPreviousFrameAsync(Arg.Any<DateTimeOffset>());
    }

    [Fact]
    public async Task CreateFrame_ShouldReturnTrue_WhenNoPreviousFrame()
    {
        // Arrange
        var toTime = DateTimeOffset.Now.AddSeconds(-1);
        var frame = new Frame
        {
            Id = "id",
            Timestamp = 123
        };

        var toTimeNextFrame = new Frame
        {
            Id = "id2",
            Timestamp = 45,
            ProjectId = "to time next frame"
        };

        _frameRepository.GetNextFrameAsync(toTime)
            .Returns(toTimeNextFrame);

        // Act
        var result = await _sut.CreateFrame(frame, toTime);

        // Assert
        result.ShouldBeTrue();
        await _frameRepository.Received()
            .GetNextFrameAsync(toTime);
        await _frameRepository.Received()
            .GetPreviousFrameAsync(frame.TimestampAsDateTime);
        await _frameRepository.Received()
            .InsertAsync(frame);
        await _frameRepository.Received()
            .UpdateAsync(Arg.Is<Frame>(f =>
                f.Id == toTimeNextFrame.Id &&
                f.Timestamp == toTime.ToUnixTimeSeconds()
            ));
    }

    [Fact]
    public async Task CreateFrame_ShouldReturnFalse_WhenFromTimeHasPrevious_ButToTimeHasnt()
    {
        // technically impossible case, but anyway

        // Arrange
        var toTime = DateTimeOffset.Now.AddSeconds(-1);
        var frame = new Frame
        {
            Id = "id",
            Timestamp = 123
        };


        var toTimeNextFrame = new Frame
        {
            Id = "id2",
            Timestamp = 45,
            ProjectId = "to time next frame"
        };

        var fromTimePreviousFrame = new Frame
        {
            Id = "id4",
            Timestamp = 67,
            ProjectId = "from time previous frame"
        };

        _frameRepository.GetNextFrameAsync(toTime)
            .Returns(toTimeNextFrame);
        _frameRepository.GetPreviousFrameAsync(frame.TimestampAsDateTime)
            .Returns(fromTimePreviousFrame);
        _frameRepository.GetPreviousFrameAsync(toTime)
            .Returns(default(Frame));

        // Act
        var result = await _sut.CreateFrame(frame, toTime);

        // Assert
        result.ShouldBeFalse();
    }

    [Fact]
    public async Task CreateFrame_ShouldReturnTrue_WhenFromTimePreviousFrameIsToTimePreviousFrame()
    {
        // Arrange
        var toTime = DateTimeOffset.Now.AddSeconds(-1);
        var frame = new Frame
        {
            Id = "id",
            Timestamp = 123
        };

        var previousFrame = new Frame
        {
            Id = "id4",
            Timestamp = 67,
            ProjectId = "from time previous frame"
        };

        _frameRepository.GetNextFrameAsync(Arg.Any<DateTimeOffset>())
            .Returns(new Frame());
        _frameRepository.GetPreviousFrameAsync(frame.TimestampAsDateTime)
            .Returns(previousFrame);
        _frameRepository.GetPreviousFrameAsync(toTime)
            .Returns(previousFrame);

        // Act
        var result = await _sut.CreateFrame(frame, toTime);

        // Assert
        result.ShouldBeTrue();
        await _frameRepository.Received()
            .InsertAsync(frame);
        await _frameRepository.UpdateAsync(Arg.Is<Frame>(f =>
            f.Id == previousFrame.Id
        ));
    }

    [Fact]
    public async Task CreateFrame_ShouldReturnTrue_WhenToTimePreviousFrameIsFromTimeNextFrame()
    {
        // Arrange
        var toTime = DateTimeOffset.Now.AddSeconds(-1);
        var frame = new Frame
        {
            Id = "id",
            Timestamp = 123
        };

        var previousFrame = new Frame
        {
            Id = "id4",
            Timestamp = 67,
            ProjectId = "from time previous frame"
        };

        _frameRepository.GetNextFrameAsync(toTime)
            .Returns(new Frame { Id = "id2" });
        _frameRepository.GetPreviousFrameAsync(frame.TimestampAsDateTime)
            .Returns(new Frame { Id = "id1" });
        _frameRepository.GetPreviousFrameAsync(toTime)
            .Returns(previousFrame);
        _frameRepository.GetNextFrameAsync(frame.TimestampAsDateTime)
            .Returns(previousFrame);

        // Act
        var result = await _sut.CreateFrame(frame, toTime);

        // Assert
        result.ShouldBeTrue();
        await _frameRepository.Received()
            .InsertAsync(frame);
        await _frameRepository.UpdateAsync(Arg.Is<Frame>(f =>
            f.Id == previousFrame.Id
        ));
    }

    [Fact]
    public async Task CreateFrame_ShouldReturnTrue_WhenToTimeFramesAndFromTimeFramesAreSparse()
    {
        // Arrange
        var toTime = DateTimeOffset.Now.AddSeconds(-1);
        var frame = new Frame
        {
            Id = "id",
            Timestamp = 123
        };

        var previousFrame = new Frame
        {
            Id = "id4",
            Timestamp = 67,
            ProjectId = "from time previous frame"
        };

        _frameRepository.GetNextFrameAsync(toTime)
            .Returns(new Frame { Id = "id2" });
        _frameRepository.GetPreviousFrameAsync(frame.TimestampAsDateTime)
            .Returns(new Frame { Id = "id1" });
        _frameRepository.GetPreviousFrameAsync(toTime)
            .Returns(new Frame { Id = "id3" });
        _frameRepository.GetNextFrameAsync(frame.TimestampAsDateTime)
            .Returns(previousFrame);

        _frameRepository.GetAsync(Arg.Any<DateTimeOffset>(), Arg.Any<DateTimeOffset>())
            .Returns([
                new Frame
                {
                    Id = "id5"
                }
            ]);

        // Act
        var result = await _sut.CreateFrame(frame, toTime);

        // Assert
        result.ShouldBeTrue();
        await _frameRepository.Received()
            .DeleteManyAsync(Arg.Any<IEnumerable<string>>());
    }

    #endregion
}