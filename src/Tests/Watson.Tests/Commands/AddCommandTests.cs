using System.Runtime.InteropServices.JavaScript;
using NSubstitute;
using Shouldly;
using Watson.Commands;
using Watson.Core.Models;
using Watson.Core.Repositories.Abstractions;
using Watson.Models.Abstractions;
using Watson.Models.CommandLine;

namespace Watson.Tests.Commands;

public class AddCommandTests
{
    #region Members

    private readonly IFrameRepository _frameRepository = Substitute.For<IFrameRepository>();
    private readonly ITagRepository _tagRepository = Substitute.For<ITagRepository>();
    private readonly IProjectRepository _projectRepository = Substitute.For<IProjectRepository>();
    private readonly AddCommand _sut;

    #endregion

    #region Constructors

    public AddCommandTests()
    {
        var dependencyResolver = Substitute.For<IDependencyResolver>();
        dependencyResolver.FrameRepository
            .Returns(_frameRepository);
        dependencyResolver.TagRepository
            .Returns(_tagRepository);
        dependencyResolver.ProjectRepository
            .Returns(_projectRepository);

        _projectRepository.GetByNameAsync(Arg.Any<string>())
            .Returns(new Project { Id = "sut" });
        _frameRepository.InsertAsync(Arg.Any<Frame>())
            .Returns(true);
        _frameRepository.UpdateAsync(Arg.Any<Frame>())
            .Returns(true);

        _sut = new AddCommand(dependencyResolver);
    }

    #endregion

    #region Tests

    [InlineData("")]
    [InlineData(null)]
    [Theory]
    public async Task Run_ShouldFail_WhenProjectMissing(string? project)
    {
        // Arrange
        var options = new AddOptions
        {
            Project = project!
        };

        // Act
        var result = await _sut.Run(options);

        // Assert
        result.ShouldNotBe(0);
    }

    [Fact]
    public async Task Run_ShouldSucceed()
    {
        // Arrange
        var options = new AddOptions
        {
            Project = "project"
        };

        // Act
        var result = await _sut.Run(options);

        // Assert
        result.ShouldBe(0);
    }

    [Fact]
    public async Task Run_ShouldInsertProject_WhenMissing()
    {
        // Arrange
        var options = new AddOptions
        {
            Project = "sut"
        };
        _projectRepository.GetByNameAsync(Arg.Any<string>())
            .Returns(default(Project));
        _projectRepository.InsertAsync(Arg.Any<Project>())
            .Returns(e =>
            {
                e.Arg<Project>().Id = "id";
                return true;
            });

        // Act
        var result = await _sut.Run(options);

        // Assert
        result.ShouldBe(0);
        await _projectRepository.Received()
            .InsertAsync(Arg.Is<Project>(p => p.Name == "sut"));
    }

    [Fact]
    public async Task Run_ShouldInsertTags_WhenMissing()
    {
        // Arrange
        var options = new AddOptions
        {
            Project = "sut",
            Tags = ["a", "b"]
        };
        _tagRepository.GetByNameAsync("a")
            .Returns(default(Tag));
        _tagRepository.GetByNameAsync("b")
            .Returns(new Tag());
        _tagRepository.InsertAsync(Arg.Any<Tag>())
            .Returns(true);
        
        // Act
        var result = await _sut.Run(options);

        // Assert
        result.ShouldBe(0);
        await _tagRepository.Received()
            .InsertAsync(Arg.Is<Tag>(t => t.Name == "a"));
    }

    [InlineData("3-27 1345", "$year-03-27 13:45")]
    [InlineData("8-28 13", "$year-08-28 13:00")]
    [InlineData("13", "$year-$month-$day 13:00")]
    [InlineData("1345", "$year-$month-$day 13:45")]
    [InlineData("0945", "$year-$month-$day 09:45")]
    [InlineData("945", "$year-$month-$day 09:45")]
    [InlineData("124", "$year-$month-$day 12:04")]
    [InlineData("078", "$year-$month-$day 07:08")]
    [InlineData("708", "$year-$month-$day 07:08")]
    [InlineData("9-9", "$year-09-09 00:00")]
    [InlineData("13 13", "$year-$month-13 13:00")]
    [InlineData("2025-12-09", "2025-12-09 00:00")]
    [InlineData("13:45", "$year-$month-$day 13:45")]
    [InlineData("09:45", "$year-$month-$day 09:45")]
    [InlineData("9:45", "$year-$month-$day 09:45")]
    [InlineData("13:8", "$year-$month-$day 13:08")]
    [InlineData("8:8", "$year-$month-$day 08:08")]
    [Theory]
    public async Task Run_ShouldSucceed_WithTheRightFromTime(string fromTime, string expectedTime)
    {
        // Arrange
        expectedTime = expectedTime.Replace("$year", DateTime.UtcNow.Year.ToString())
            .Replace("$month", DateTime.UtcNow.Month.ToString().PadLeft(2, '0'))
            .Replace("$day", DateTime.UtcNow.Day.ToString().PadLeft(2, '0'));

        var options = new AddOptions
        {
            Project = "project",
            FromTime = fromTime
        };
        string? actualFromTime = null;
        _frameRepository.WhenForAnyArgs(e => e.InsertAsync(Arg.Any<Frame>()))
            .Do(e => actualFromTime = DateTimeOffset.FromUnixTimeSeconds(e.Arg<Frame>().Timestamp)
                .LocalDateTime
                .ToString("yyyy-MM-dd HH:mm"));

        // Act
        var result = await _sut.Run(options);

        // Assert
        result.ShouldBe(0);
        actualFromTime.ShouldNotBeNull();
        actualFromTime.ShouldBe(expectedTime);
    }

    [Fact]
    public async Task Run_ShouldInsertNormally_WhenNoToTime()
    {
        // Arrange
        var options = new AddOptions
        {
            Project = "sut"
        };

        // Act
        var result = await _sut.Run(options);

        // Assert
        result.ShouldBe(0);
        await _frameRepository.Received()
            .InsertAsync(Arg.Is<Frame>(f => f.ProjectId == "sut"));
    }

    [Fact]
    public async Task Run_ShouldInsertNormally_WhenFromTimeDoesntMeetAnyFrames()
    {
        // Arrange
        var options = new AddOptions
        {
            Project = "sut",
            ToTime = DateTimeOffset.UtcNow.ToString("yyyy-MM-dd HH:mm")
        };
        _frameRepository.GetNextFrameAsync(Arg.Any<DateTimeOffset>())
            .Returns(default(Frame));

        // Act
        var result = await _sut.Run(options);

        // Assert
        result.ShouldBe(0);
        await _frameRepository.Received()
            .InsertAsync(Arg.Is<Frame>(e => e.ProjectId == "sut"));
    }

    [Fact]
    public async Task Run_ShouldInsertNormally_WhenFromTimeDoesntMeetAnyPreviousFrame_BeginningOfTheDay()
    {
        // Arrange
        var toTime = DateTimeOffset.UtcNow.Date.AddMinutes(2);
        var options = new AddOptions
        {
            Project = "sut",
            FromTime = DateTimeOffset.UtcNow.Date.ToString("yyyy-MM-dd HH:mm"),
            ToTime = toTime.ToString("yyyy-MM-dd HH:mm")
        };
        _frameRepository.GetNextFrameAsync(Arg.Any<DateTimeOffset>())
            .Returns(new Frame
            {
                ProjectId = "next frame",
                Timestamp = new DateTimeOffset(DateTimeOffset.UtcNow.Date.AddMinutes(1)).ToUnixTimeSeconds()
            });

        // Act
        var result = await _sut.Run(options);

        // Assert
        result.ShouldBe(0);
        await _frameRepository.Received()
            .InsertAsync(Arg.Is<Frame>(e => e.ProjectId == "sut"));
        await _frameRepository.Received()
            .UpdateAsync(Arg.Is<Frame>(e => e.ProjectId == "next frame" &&
                                            e.Timestamp == new DateTimeOffset(toTime).ToUnixTimeSeconds()
            ));
    }

    [Fact]
    public async Task Run_ShouldFail_WhenFromTimeMeetsPrevious_AndToTimeDoesntMeetAnyPreviousFrame()
    {
        // Arrange
        var fromTime = DateTimeOffset.UtcNow.Date;
        var toTime = DateTimeOffset.UtcNow.Date.AddMinutes(2);
        var options = new AddOptions
        {
            Project = "sut",
            FromTime = fromTime.ToString("yyyy-MM-dd HH:mm"),
            ToTime = toTime.ToString("yyyy-MM-dd HH:mm")
        };
        _frameRepository.GetPreviousFrameAsync(fromTime)
            .Returns(new Frame());
        _frameRepository.GetPreviousFrameAsync(toTime)
            .Returns(default(Frame));

        // Act
        var result = await _sut.Run(options);

        // Assert
        result.ShouldNotBe(1);
    }

    [Fact]
    public async Task
        Run_ShouldInsertAndCopyFromTimePreviousFrameToCreateNewNextFrame_WhenFromTimeAndToTimeHaveTheSamePreviousFrame()
    {
        // Arrange
        var fromTime = DateTimeOffset.UtcNow.Date;
        var toTime = DateTimeOffset.UtcNow.Date.AddMinutes(2);
        var options = new AddOptions
        {
            Project = "sut",
            FromTime = fromTime.ToString("yyyy-MM-dd HH:mm"),
            ToTime = toTime.ToString("yyyy-MM-dd HH:mm")
        };
        _frameRepository.GetNextFrameAsync(Arg.Any<DateTimeOffset>())
            .Returns(new Frame());
        _frameRepository.GetPreviousFrameAsync(fromTime)
            .Returns(new Frame
            {
                Id = "id",
                ProjectId = "fromTime previous frame"
            });
        _frameRepository.GetPreviousFrameAsync(toTime)
            .Returns(new Frame
            {
                Id = "id",
                ProjectId = "toTime previous frame"
            });

        // Act
        var result = await _sut.Run(options);

        // Assert
        result.ShouldBe(0);
        await _frameRepository.Received()
            .InsertAsync(Arg.Is<Frame>(e => e.ProjectId == "sut"));
        await _frameRepository.Received()
            .InsertAsync(Arg.Is<Frame>(e => e.ProjectId == "fromTime previous frame" &&
                                            e.Timestamp == new DateTimeOffset(toTime).ToUnixTimeSeconds()
            ));
    }

    [Fact]
    public async Task Run_ShouldInsertAndUpdateFromTimeNextFrameToToTime_WhenFromTimeAndToTimeAreOverTwoFrames()
    {
        // Arrange
        var fromTime = DateTimeOffset.UtcNow.Date;
        var toTime = DateTimeOffset.UtcNow.Date.AddMinutes(2);
        var options = new AddOptions
        {
            Project = "sut",
            FromTime = fromTime.ToString("yyyy-MM-dd HH:mm"),
            ToTime = toTime.ToString("yyyy-MM-dd HH:mm")
        };
        _frameRepository.GetNextFrameAsync(toTime)
            .Returns(new Frame
            {
                Id = "id1",
                ProjectId = "toTime next frame"
            });
        _frameRepository.GetPreviousFrameAsync(fromTime)
            .Returns(new Frame
            {
                Id = "id",
                ProjectId = "fromTime previous frame"
            });
        _frameRepository.GetPreviousFrameAsync(toTime)
            .Returns(new Frame
            {
                Id = "id1",
                ProjectId = "toTime previous frame"
            });

        // Act
        var result = await _sut.Run(options);

        // Assert
        result.ShouldBe(0);
        await _frameRepository.Received()
            .InsertAsync(Arg.Is<Frame>(e => e.ProjectId == "sut"));
        await _frameRepository.Received()
            .UpdateAsync(Arg.Is<Frame>(e => e.ProjectId == "toTime previous frame" &&
                                            e.Timestamp == new DateTimeOffset(toTime).ToUnixTimeSeconds()
            ));
    }

    [Fact]
    public async Task
        Run_ShouldInsertAndUpdateToTimeNextFrameAndDeleteMiddleFrames_WhenFromAndToTimeAreOverMultipleFrames()
    {
        // Arrange
        var fromTime = DateTimeOffset.UtcNow.Date;
        var toTime = DateTimeOffset.UtcNow.Date.AddMinutes(2);
        var options = new AddOptions
        {
            Project = "sut",
            FromTime = fromTime.ToString("yyyy-MM-dd HH:mm"),
            ToTime = toTime.ToString("yyyy-MM-dd HH:mm")
        };
        _frameRepository.GetAsync(Arg.Any<DateTimeOffset>(), Arg.Any<DateTimeOffset>())
            .Returns([
                new Frame
                {
                    Id = "id1"
                },
                new Frame
                {
                    Id = "id2"
                }
            ]);
        _frameRepository.GetNextFrameAsync(toTime)
            .Returns(new Frame
            {
                Id = "id1",
                ProjectId = "toTime next frame",
                Timestamp = 0
            });
        _frameRepository.GetPreviousFrameAsync(fromTime)
            .Returns(new Frame
            {
                Id = "id",
                ProjectId = "fromTime previous frame"
            });
        _frameRepository.GetPreviousFrameAsync(toTime)
            .Returns(new Frame
            {
                Id = "id2",
                ProjectId = "toTime previous frame",
                Timestamp = 0
            });
        _frameRepository.DeleteManyAsync(Arg.Any<IEnumerable<string>>())
            .Returns(true);

        // Act
        var result = await _sut.Run(options);

        // Assert
        result.ShouldBe(0);
        await _frameRepository.Received()
            .InsertAsync(Arg.Is<Frame>(e => e.ProjectId == "sut"));
        await _frameRepository.Received()
            .UpdateAsync(Arg.Is<Frame>(e => e.ProjectId == "toTime next frame" &&
                                            e.Timestamp == new DateTimeOffset(toTime).ToUnixTimeSeconds()
            ));
        await _frameRepository.Received()
            .DeleteManyAsync(Arg.Is<IEnumerable<string>>(e => e.Contains("id1") && e.Contains("id2")));
    }

    [Fact]
    public async Task Run_ShouldFail_WhenProjectFailToExists()
    {
        // Arrange
        var options = new AddOptions
        {
            Project = "sut"
        };
        _projectRepository.GetByNameAsync(Arg.Any<string>())
            .Returns(default(Project));
        _projectRepository.InsertAsync(Arg.Any<Project>())
            .Returns(false);

        // Act
        var result = await _sut.Run(options);

        // Assert
        result.ShouldBe(1);
    }

    [Fact]
    public async Task Run_ShouldFail_WhenFromTimeDoesntMeetAnyPreviousFrame_BeginningOfTheDay_ButFailToInsert()
    {
        // Arrange
        var toTime = DateTimeOffset.UtcNow.Date.AddMinutes(2);
        var options = new AddOptions
        {
            Project = "sut",
            FromTime = DateTimeOffset.UtcNow.Date.ToString("yyyy-MM-dd HH:mm"),
            ToTime = toTime.ToString("yyyy-MM-dd HH:mm")
        };
        _frameRepository.GetNextFrameAsync(Arg.Any<DateTimeOffset>())
            .Returns(new Frame
            {
                ProjectId = "next frame",
                Timestamp = new DateTimeOffset(DateTimeOffset.UtcNow.Date.AddMinutes(1)).ToUnixTimeSeconds()
            });
        _frameRepository.InsertAsync(Arg.Any<Frame>())
            .Returns(false);

        // Act
        var result = await _sut.Run(options);

        // Assert
        result.ShouldBe(1);
    }

    [Fact]
    public async Task Run_ShouldFail_WhenFromTimeHasPreviousFrame_ButToTimeDont()
    {
        // Arrange
        var fromTime = DateTimeOffset.UtcNow.Date;
        var toTime = DateTimeOffset.UtcNow.Date.AddMinutes(2);
        var options = new AddOptions
        {
            Project = "sut",
            FromTime = fromTime.ToString("yyyy-MM-dd HH:mm"),
            ToTime = toTime.ToString("yyyy-MM-dd HH:mm")
        };
        _frameRepository.GetPreviousFrameAsync(fromTime)
            .Returns(new Frame());
        _frameRepository.GetPreviousFrameAsync(toTime)
            .Returns(default(Frame));
        _frameRepository.GetNextFrameAsync(toTime)
            .Returns(new Frame());

        // Act
        var result = await _sut.Run(options);

        // Assert
        result.ShouldBe(1);
    }

    [Fact]
    public async Task Run_ShouldFail_WhenInsertFailOverTwoFrames()
    {
        // Arrange
        var fromTime = DateTimeOffset.UtcNow.Date;
        var toTime = DateTimeOffset.UtcNow.Date.AddMinutes(2);
        var options = new AddOptions
        {
            Project = "sut",
            FromTime = fromTime.ToString("yyyy-MM-dd HH:mm"),
            ToTime = toTime.ToString("yyyy-MM-dd HH:mm")
        };
        _frameRepository.GetNextFrameAsync(toTime)
            .Returns(new Frame
            {
                Id = "id1",
                ProjectId = "toTime next frame"
            });
        _frameRepository.GetPreviousFrameAsync(fromTime)
            .Returns(new Frame
            {
                Id = "id",
                ProjectId = "fromTime previous frame"
            });
        _frameRepository.GetPreviousFrameAsync(toTime)
            .Returns(new Frame
            {
                Id = "id1",
                ProjectId = "toTime previous frame"
            });
        _frameRepository.InsertAsync(Arg.Any<Frame>())
            .Returns(false);

        // Act
        var result = await _sut.Run(options);

        // Assert
        result.ShouldBe(1);
    }

    [Fact]
    public async Task Run_ShouldFail_WhenInsertFailsOverMultipleFrames()
    {
        // Arrange
        var fromTime = DateTimeOffset.UtcNow.Date;
        var toTime = DateTimeOffset.UtcNow.Date.AddMinutes(2);
        var options = new AddOptions
        {
            Project = "sut",
            FromTime = fromTime.ToString("yyyy-MM-dd HH:mm"),
            ToTime = toTime.ToString("yyyy-MM-dd HH:mm")
        };
        _frameRepository.GetAsync(Arg.Any<DateTimeOffset>(), Arg.Any<DateTimeOffset>())
            .Returns([
                new Frame
                {
                    Id = "id1"
                },
                new Frame
                {
                    Id = "id2"
                }
            ]);
        _frameRepository.GetNextFrameAsync(toTime)
            .Returns(new Frame
            {
                Id = "id1",
                ProjectId = "toTime next frame",
                Timestamp = 0
            });
        _frameRepository.GetPreviousFrameAsync(fromTime)
            .Returns(new Frame
            {
                Id = "id",
                ProjectId = "fromTime previous frame"
            });
        _frameRepository.GetPreviousFrameAsync(toTime)
            .Returns(new Frame
            {
                Id = "id2",
                ProjectId = "toTime previous frame",
                Timestamp = 0
            });

        // Act
        var result = await _sut.Run(options);

        // Assert
        result.ShouldBe(1);
    }

    [Fact]
    public async Task Run_ShouldFail_WhenDeletingMiddleFramesFails()
    {
        // Arrange
        var fromTime = DateTimeOffset.UtcNow.Date;
        var toTime = DateTimeOffset.UtcNow.Date.AddMinutes(2);
        var options = new AddOptions
        {
            Project = "sut",
            FromTime = fromTime.ToString("yyyy-MM-dd HH:mm"),
            ToTime = toTime.ToString("yyyy-MM-dd HH:mm")
        };
        _frameRepository.GetAsync(Arg.Any<DateTimeOffset>(), Arg.Any<DateTimeOffset>())
            .Returns([
                new Frame
                {
                    Id = "id1"
                },
                new Frame
                {
                    Id = "id2"
                }
            ]);
        _frameRepository.GetNextFrameAsync(toTime)
            .Returns(new Frame
            {
                Id = "id1",
                ProjectId = "toTime next frame",
                Timestamp = 0
            });
        _frameRepository.GetPreviousFrameAsync(fromTime)
            .Returns(new Frame
            {
                Id = "id",
                ProjectId = "fromTime previous frame"
            });
        _frameRepository.GetPreviousFrameAsync(toTime)
            .Returns(new Frame
            {
                Id = "id2",
                ProjectId = "toTime previous frame",
                Timestamp = 0
            });
        _frameRepository.DeleteManyAsync(Arg.Any<IEnumerable<string>>())
            .Returns(false);

        // Act
        var result = await _sut.Run(options);

        // Assert
        result.ShouldBe(1);
    }
    
    #endregion
}