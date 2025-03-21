﻿using System.Reflection;
using NSubstitute;
using Shouldly;
using Watson.Core.Models.Database;
using Watson.Core.Repositories.Abstractions;
using Watson.Helpers;

namespace Watson.Tests.Tests.Helpers;

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
            .Returns(e => e.Arg<Frame>());

        _frameRepository.UpdateAsync(Arg.Any<Frame>())
            .Returns(true);

        _frameRepository.DeleteManyAsync(Arg.Any<IEnumerable<string>>())
            .Returns(true);

        _sut = new FrameHelper(_frameRepository);
    }

    #endregion

    #region Tests

    [Test]
    public async Task CreateFrameAtTheBeginningOfTheDay_ShouldReturnTrue()
    {
        // Arrange
        var frame = new Frame { Id = "id" };
        var toTimeNextFrame = new Frame
        {
            Id = "id2",
            Time = 0,
            ProjectId = "to time next frame"
        };
        var toTime = DateTime.Now.AddSeconds(-1);
        var sut = _sut.GetType()
            .GetMethod("CreateFrameAtTheBeginningOfTheDay", BindingFlags.NonPublic | BindingFlags.Instance);
        sut.ShouldNotBeNull();

        // Act
        var result = await (Task<Frame?>)sut.Invoke(_sut, [frame, toTime, toTimeNextFrame])!;

        // Assert
        result.ShouldNotBeNull();
        await _frameRepository.Received()
            .InsertAsync(frame);
        await _frameRepository.Received()
            .UpdateAsync(Arg.Is<Frame>(f =>
                f.Id == toTimeNextFrame.Id &&
                f.Time == toTime.Ticks &&
                f.ProjectId == "to time next frame"
            ));
    }

    [Test]
    public async Task CreateFrameAtTheBeginningOfTheDay_ShouldReturnFalse_WhenInsertFails()
    {
        // Arrange
        var frame = new Frame { Id = "id" };
        var toTimeNextFrame = new Frame
        {
            Id = "id2",
            Time = 0,
        };
        var toTime = DateTime.Now.AddSeconds(-1);
        var sut = _sut.GetType()
            .GetMethod("CreateFrameAtTheBeginningOfTheDay", BindingFlags.NonPublic | BindingFlags.Instance);
        sut.ShouldNotBeNull();

        _frameRepository.InsertAsync(frame)
            .Returns(default(Frame));

        // Act
        var result = await (Task<Frame?>)sut.Invoke(_sut, [frame, toTime, toTimeNextFrame])!;

        // Assert
        result.ShouldBeNull();
    }

    [Test]
    public async Task CreateFrameAtTheBeginningOfTheDay_ShouldReturnFalse_WhenUpdateFails()
    {
        // Arrange
        var frame = new Frame { Id = "id" };
        var toTimeNextFrame = new Frame
        {
            Id = "id2",
            Time = 0,
        };
        var toTime = DateTime.Now.AddSeconds(-1);
        var sut = _sut.GetType()
            .GetMethod("CreateFrameAtTheBeginningOfTheDay", BindingFlags.NonPublic | BindingFlags.Instance);
        sut.ShouldNotBeNull();

        _frameRepository.UpdateAsync(toTimeNextFrame)
            .Returns(false);

        // Act
        var result = await (Task<Frame?>)sut.Invoke(_sut, [frame, toTime, toTimeNextFrame])!;

        // Assert
        result.ShouldBeNull();
    }

    [Test]
    public async Task CreateFrameContainedInAFrame_ShouldReturnTrue()
    {
        // Arrange
        var frame = new Frame { Id = "id" };
        var toTime = DateTime.Now.AddSeconds(-1);
        var fromTimePreviousFrame = new Frame
        {
            Id = "id2",
            Time = 0,
            ProjectId = "from time previous frame"
        };
        var sut = _sut.GetType()
            .GetMethod("CreateFrameContainedInAFrame", BindingFlags.NonPublic | BindingFlags.Instance);
        sut.ShouldNotBeNull();

        // Act
        var result = await (Task<Frame?>)sut.Invoke(_sut, [frame, toTime, fromTimePreviousFrame])!;

        // Assert
        result.ShouldNotBeNull();
        await _frameRepository.Received()
            .InsertAsync(frame);
        await _frameRepository.Received()
            .InsertAsync(Arg.Is<Frame>(f =>
                f.Id == string.Empty &&
                f.Time == toTime.Ticks &&
                f.ProjectId == "from time previous frame"
            ));
    }

    [Test]
    public async Task CreateFrameContainedInAFrame_ShouldReturnFalse_WhenInsertFails()
    {
        // Arrange
        var frame = new Frame { Id = "id" };
        var toTime = DateTime.Now.AddSeconds(-1);
        var fromTimePreviousFrame = new Frame
        {
            Id = "id2",
            Time = 0,
        };
        var sut = _sut.GetType()
            .GetMethod("CreateFrameContainedInAFrame", BindingFlags.NonPublic | BindingFlags.Instance);
        sut.ShouldNotBeNull();

        _frameRepository.InsertAsync(frame)
            .Returns(default(Frame));

        // Act
        var result = await (Task<Frame?>)sut.Invoke(_sut, [frame, toTime, fromTimePreviousFrame])!;

        // Assert
        result.ShouldBeNull();
    }

    [Test]
    public async Task CreateFrameContainedInAFrame_ShouldReturnFalse_WhenPreviousInsertFails()
    {
        // Arrange
        var frame = new Frame { Id = "id" };
        var toTime = DateTime.Now.AddSeconds(-1);
        var fromTimePreviousFrame = new Frame
        {
            Id = "id2",
            Time = 0,
            ProjectId = "from time previous frame"
        };
        var sut = _sut.GetType()
            .GetMethod("CreateFrameContainedInAFrame", BindingFlags.NonPublic | BindingFlags.Instance);
        sut.ShouldNotBeNull();

        _frameRepository.InsertAsync(Arg.Is<Frame>(f => f.ProjectId == "from time previous frame"))
            .Returns(default(Frame));

        // Act
        var result = await (Task<Frame?>)sut.Invoke(_sut, [frame, toTime, fromTimePreviousFrame])!;

        // Assert
        result.ShouldBeNull();
    }

    [Test]
    public async Task CreateFrameOverTwoFrames_ShouldReturnTrue()
    {
        // Arrange
        var frame = new Frame { Id = "id" };
        var toTime = DateTime.Now.AddSeconds(-1);
        var toTimePreviousFrame = new Frame
        {
            Id = "id2",
            Time = 0,
            ProjectId = "to time previous frame"
        };
        var sut = _sut.GetType()
            .GetMethod("CreateFrameOverTwoFrames", BindingFlags.NonPublic | BindingFlags.Instance);
        sut.ShouldNotBeNull();

        // Act
        var result = await (Task<Frame?>)sut.Invoke(_sut, [frame, toTime, toTimePreviousFrame])!;

        // Assert
        result.ShouldNotBeNull();
        await _frameRepository.Received()
            .InsertAsync(frame);
        await _frameRepository.Received()
            .UpdateAsync(Arg.Is<Frame>(f =>
                f.Id == "id2" &&
                f.Time == toTime.Ticks &&
                f.ProjectId == "to time previous frame"
            ));
    }

    [Test]
    public async Task CreateFrameOverTwoFrames_ShouldReturnFalse_WhenInsertFails()
    {
        // Arrange
        var frame = new Frame { Id = "id" };
        var toTime = DateTime.Now.AddSeconds(-1);
        var toTimePreviousFrame = new Frame
        {
            Id = "id2",
            Time = 0,
            ProjectId = "to time previous frame"
        };
        var sut = _sut.GetType()
            .GetMethod("CreateFrameOverTwoFrames", BindingFlags.NonPublic | BindingFlags.Instance);
        sut.ShouldNotBeNull();

        _frameRepository.InsertAsync(frame)
            .Returns(default(Frame));

        // Act
        var result = await (Task<Frame?>)sut.Invoke(_sut, [frame, toTime, toTimePreviousFrame])!;

        // Assert
        result.ShouldBeNull();
    }

    [Test]
    public async Task CreateFrameOverTwoFrames_ShouldReturnFalse_WhenUpdateFails()
    {
        // Arrange
        var frame = new Frame { Id = "id" };
        var toTime = DateTime.Now.AddSeconds(-1);
        var toTimePreviousFrame = new Frame
        {
            Id = "id2",
            Time = 0,
            ProjectId = "to time previous frame"
        };
        var sut = _sut.GetType()
            .GetMethod("CreateFrameOverTwoFrames", BindingFlags.NonPublic | BindingFlags.Instance);
        sut.ShouldNotBeNull();

        _frameRepository.UpdateAsync(Arg.Is<Frame>(f => f.ProjectId == "to time previous frame"))
            .Returns(false);

        // Act
        var result = await (Task<Frame?>)sut.Invoke(_sut, [frame, toTime, toTimePreviousFrame])!;

        // Assert
        result.ShouldBeNull();
    }

    [Test]
    public async Task CreateFrameOverMultipleFrames_ShouldReturnTrue()
    {
        // Arrange
        var frame = new Frame { Id = "id" };
        var toTime = DateTime.Now.AddSeconds(-1);
        var toTimePreviousFrame = new Frame
        {
            Id = "id2",
            Time = 2,
            ProjectId = "to time previous frame"
        };
        var toTimeNextFrame = new Frame
        {
            Id = "id3",
            Time = 3,
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

        _frameRepository.GetAsync(Arg.Any<DateTime>(), Arg.Any<DateTime>())
            .Returns(framesToDelete);

        // Act
        var result = await (Task<Frame?>)sut.Invoke(_sut, [frame, toTime, toTimeNextFrame, toTimePreviousFrame])!;

        // Assert
        result.ShouldNotBeNull();
        await _frameRepository.Received()
            .GetAsync(
                Arg.Is<DateTime>(d =>
                    d.Ticks == 3
                ),
                Arg.Is<DateTime>(d =>
                    d.Ticks == 2
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
                f.Time == toTime.Ticks &&
                f.ProjectId == "to time next frame"
            ));
    }

    [Test]
    public async Task CreateFrameOverMultipleFrames_ShouldReturnFalse_WhenDeleteFails()
    {
        // Arrange
        var frame = new Frame { Id = "id" };
        var toTime = DateTime.Now.AddSeconds(-1);
        var toTimePreviousFrame = new Frame
        {
            Id = "id2",
            Time = 2,
            ProjectId = "to time previous frame"
        };
        var toTimeNextFrame = new Frame
        {
            Id = "id3",
            Time = 3,
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

        _frameRepository.GetAsync(Arg.Any<DateTime>(), Arg.Any<DateTime>())
            .Returns(framesToDelete);

        _frameRepository.DeleteManyAsync(Arg.Any<IEnumerable<string>>())
            .Returns(false);

        // Act
        var result = await (Task<Frame?>)sut.Invoke(_sut, [frame, toTime, toTimeNextFrame, toTimePreviousFrame])!;

        // Assert
        result.ShouldBeNull();
    }

    [Test]
    public async Task CreateFrameOverMultipleFrames_ShouldReturnFalse_WhenInsertFails()
    {
        // Arrange
        var frame = new Frame { Id = "id" };
        var toTime = DateTime.Now.AddSeconds(-1);
        var toTimePreviousFrame = new Frame
        {
            Id = "id2",
            Time = 2,
            ProjectId = "to time previous frame"
        };
        var toTimeNextFrame = new Frame
        {
            Id = "id3",
            Time = 3,
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

        _frameRepository.GetAsync(Arg.Any<DateTime>(), Arg.Any<DateTime>())
            .Returns(framesToDelete);

        _frameRepository.InsertAsync(Arg.Any<Frame>())
            .Returns(default(Frame));

        // Act
        var result = await (Task<Frame?>)sut.Invoke(_sut, [frame, toTime, toTimeNextFrame, toTimePreviousFrame])!;

        // Assert
        result.ShouldBeNull();
    }

    [Test]
    public async Task CreateFrameOverMultipleFrames_ShouldReturnFalse_WhenUpdateFails()
    {
        // Arrange
        var frame = new Frame { Id = "id" };
        var toTime = DateTime.Now.AddSeconds(-1);
        var toTimePreviousFrame = new Frame
        {
            Id = "id2",
            Time = 2,
            ProjectId = "to time previous frame"
        };
        var toTimeNextFrame = new Frame
        {
            Id = "id3",
            Time = 3,
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

        _frameRepository.GetAsync(Arg.Any<DateTime>(), Arg.Any<DateTime>())
            .Returns(framesToDelete);

        _frameRepository.UpdateAsync(Arg.Any<Frame>())
            .Returns(false);

        // Act
        var result = await (Task<Frame?>)sut.Invoke(_sut, [frame, toTime, toTimeNextFrame, toTimePreviousFrame])!;

        // Assert
        result.ShouldBeNull();
    }

    [Test]
    public async Task CreateFrameOverMultipleFrames_ShouldReturnTrue_WithNoFrameDeleted_WhenNothingOverlap()
    {
        // Arrange
        var frame = new Frame { Id = "id" };
        var toTime = DateTime.Now.AddSeconds(-1);
        var toTimePreviousFrame = new Frame
        {
            Id = "id2",
            Time = 2,
            ProjectId = "to time previous frame"
        };
        var toTimeNextFrame = new Frame
        {
            Id = "id3",
            Time = 3,
            ProjectId = "to time next frame"
        };
        var sut = _sut.GetType()
            .GetMethod("CreateFrameOverMultipleFrames", BindingFlags.NonPublic | BindingFlags.Instance);
        sut.ShouldNotBeNull();

        _frameRepository.GetAsync(Arg.Any<DateTime>(), Arg.Any<DateTime>())
            .Returns([]);

        // Act
        var result = await (Task<Frame?>)sut.Invoke(_sut, [frame, toTime, toTimeNextFrame, toTimePreviousFrame])!;

        // Assert
        result.ShouldNotBeNull();
        await _frameRepository.Received()
            .GetAsync(
                Arg.Is<DateTime>(d =>
                    d.Ticks == 3
                ),
                Arg.Is<DateTime>(d =>
                    d.Ticks == 2
                )
            );
        await _frameRepository.Received(0)
            .DeleteManyAsync(Arg.Any<IEnumerable<string>>());
        await _frameRepository.Received()
            .InsertAsync(frame);
        await _frameRepository.Received()
            .UpdateAsync(Arg.Is<Frame>(f =>
                f.Id == "id3" &&
                f.Time == toTime.Ticks &&
                f.ProjectId == "to time next frame"
            ));
    }

    [Test]
    public async Task CreateFrame_ShouldReturnTrue_WhenToTimeIsNull()
    {
        // Arrange
        var frame = new Frame { Id = "id" };

        // Act
        var result = await _sut.CreateFrame(frame);

        // Assert
        result.ShouldNotBeNull();
        await _frameRepository.Received()
            .InsertAsync(frame);
        await _frameRepository.Received(0)
            .GetNextFrameAsync(Arg.Any<DateTime>());
    }

    [Test]
    public async Task CreateFrame_ShouldReturnTrue_WhenNoNextFrame()
    {
        // Arrange
        var toTime = DateTime.Now.AddSeconds(-1);
        var frame = new Frame { Id = "id" };

        // Act
        var result = await _sut.CreateFrame(frame, toTime);

        // Assert
        result.ShouldNotBeNull();
        await _frameRepository.Received()
            .InsertAsync(frame);
        await _frameRepository.Received()
            .GetNextFrameAsync(toTime);
        await _frameRepository.Received(0)
            .GetPreviousFrameAsync(Arg.Any<DateTime>());
    }

    [Test]
    public async Task CreateFrame_ShouldReturnTrue_WhenNoPreviousFrame()
    {
        // Arrange
        var toTime = DateTime.Now.AddSeconds(-1);
        var frame = new Frame
        {
            Id = "id",
            Time = 123
        };

        var toTimeNextFrame = new Frame
        {
            Id = "id2",
            Time = 45,
            ProjectId = "to time next frame"
        };

        _frameRepository.GetNextFrameAsync(toTime)
            .Returns(toTimeNextFrame);

        // Act
        var result = await _sut.CreateFrame(frame, toTime);

        // Assert
        result.ShouldNotBeNull();
        await _frameRepository.Received()
            .GetNextFrameAsync(toTime);
        await _frameRepository.Received()
            .GetPreviousFrameAsync(frame.TimeAsDateTime);
        await _frameRepository.Received()
            .InsertAsync(frame);
        await _frameRepository.Received()
            .UpdateAsync(Arg.Is<Frame>(f =>
                f.Id == toTimeNextFrame.Id &&
                f.Time == toTime.Ticks
            ));
    }

    [Test]
    public async Task CreateFrame_ShouldReturnFalse_WhenFromTimeHasPrevious_ButToTimeHasnt()
    {
        // technically impossible case, but anyway

        // Arrange
        var toTime = DateTime.Now.AddSeconds(-1);
        var frame = new Frame
        {
            Id = "id",
            Time = 123
        };


        var toTimeNextFrame = new Frame
        {
            Id = "id2",
            Time = 45,
            ProjectId = "to time next frame"
        };

        var fromTimePreviousFrame = new Frame
        {
            Id = "id4",
            Time = 67,
            ProjectId = "from time previous frame"
        };

        _frameRepository.GetNextFrameAsync(toTime)
            .Returns(toTimeNextFrame);
        _frameRepository.GetPreviousFrameAsync(frame.TimeAsDateTime)
            .Returns(fromTimePreviousFrame);
        _frameRepository.GetPreviousFrameAsync(toTime)
            .Returns(default(Frame));

        // Act
        var result = await _sut.CreateFrame(frame, toTime);

        // Assert
        result.ShouldBeNull();
    }

    [Test]
    public async Task CreateFrame_ShouldReturnTrue_WhenFromTimePreviousFrameIsToTimePreviousFrame()
    {
        // Arrange
        var toTime = DateTime.Now.AddSeconds(-1);
        var frame = new Frame
        {
            Id = "id",
            Time = 123
        };

        var previousFrame = new Frame
        {
            Id = "id4",
            Time = 67,
            ProjectId = "from time previous frame"
        };

        _frameRepository.GetNextFrameAsync(Arg.Any<DateTime>())
            .Returns(new Frame());
        _frameRepository.GetPreviousFrameAsync(frame.TimeAsDateTime)
            .Returns(previousFrame);
        _frameRepository.GetPreviousFrameAsync(toTime)
            .Returns(previousFrame);

        // Act
        var result = await _sut.CreateFrame(frame, toTime);

        // Assert
        result.ShouldNotBeNull();
        await _frameRepository.Received()
            .InsertAsync(frame);
        await _frameRepository.UpdateAsync(Arg.Is<Frame>(f =>
            f.Id == previousFrame.Id
        ));
    }

    [Test]
    public async Task CreateFrame_ShouldReturnTrue_WhenToTimePreviousFrameIsFromTimeNextFrame()
    {
        // Arrange
        var toTime = DateTime.Now.AddSeconds(-1);
        var frame = new Frame
        {
            Id = "id",
            Time = 123
        };

        var previousFrame = new Frame
        {
            Id = "id4",
            Time = 67,
            ProjectId = "from time previous frame"
        };

        _frameRepository.GetNextFrameAsync(toTime)
            .Returns(new Frame { Id = "id2" });
        _frameRepository.GetPreviousFrameAsync(frame.TimeAsDateTime)
            .Returns(new Frame { Id = "id1" });
        _frameRepository.GetPreviousFrameAsync(toTime)
            .Returns(previousFrame);
        _frameRepository.GetNextFrameAsync(frame.TimeAsDateTime)
            .Returns(previousFrame);

        // Act
        var result = await _sut.CreateFrame(frame, toTime);

        // Assert
        result.ShouldNotBeNull();
        await _frameRepository.Received()
            .InsertAsync(frame);
        await _frameRepository.UpdateAsync(Arg.Is<Frame>(f =>
            f.Id == previousFrame.Id
        ));
    }

    [Test]
    public async Task CreateFrame_ShouldReturnTrue_WhenToTimeFramesAndFromTimeFramesAreSparse()
    {
        // Arrange
        var toTime = DateTime.Now.AddSeconds(-1);
        var frame = new Frame
        {
            Id = "id",
            Time = 123
        };

        var previousFrame = new Frame
        {
            Id = "id4",
            Time = 67,
            ProjectId = "from time previous frame"
        };

        _frameRepository.GetNextFrameAsync(toTime)
            .Returns(new Frame { Id = "id2" });
        _frameRepository.GetPreviousFrameAsync(frame.TimeAsDateTime)
            .Returns(new Frame { Id = "id1" });
        _frameRepository.GetPreviousFrameAsync(toTime)
            .Returns(new Frame { Id = "id3" });
        _frameRepository.GetNextFrameAsync(frame.TimeAsDateTime)
            .Returns(previousFrame);

        _frameRepository.GetAsync(Arg.Any<DateTime>(), Arg.Any<DateTime>())
            .Returns([
                new Frame
                {
                    Id = "id5"
                }
            ]);

        // Act
        var result = await _sut.CreateFrame(frame, toTime);

        // Assert
        result.ShouldNotBeNull();
        await _frameRepository.Received()
            .DeleteManyAsync(Arg.Any<IEnumerable<string>>());
    }

    #endregion
}