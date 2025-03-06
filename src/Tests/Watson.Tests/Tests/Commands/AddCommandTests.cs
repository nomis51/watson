using Dapper;
using NSubstitute;
using Shouldly;
using Watson.Commands;
using Watson.Core.Helpers;
using Watson.Core.Models.Database;
using Watson.Core.Models.Settings;
using Watson.Core.Repositories;
using Watson.Core.Repositories.Abstractions;
using Watson.Helpers;
using Watson.Models;
using Watson.Models.CommandLine;
using Watson.Tests.Abstractions;

namespace Watson.Tests.Tests.Commands;

public class AddCommandTests : ConsoleTest
{
    #region Members

    private readonly ISettingsRepository _settingsRepository = Substitute.For<ISettingsRepository>();
    private readonly AddCommand _sut;

    #endregion

    #region Constructors

    public AddCommandTests()
    {
        var idHelper = new IdHelper();

        _settingsRepository.GetSettings()
            .Returns(new Settings
            {
                WorkTime =
                {
                    StartTime = new TimeSpan(1, 0, 0),
                    EndTime = new TimeSpan(23, 59, 0)
                }
            });

        var frameRepository = new FrameRepository(DbContext, idHelper);
        _sut = new AddCommand(
            new DependencyResolver(
                new ProjectRepository(DbContext, idHelper),
                frameRepository,
                new TagRepository(DbContext, idHelper),
                new TimeHelper(),
                new FrameHelper(frameRepository),
                _settingsRepository
            )
        );
    }

    #endregion

    #region Tests

    [Fact]
    public async Task Run_ShouldAddFrameAtNow_WithNoTag()
    {
        // Arrange
        AddOptions options = new()
        {
            Project = "project"
        };

        // Act
        await _sut.Run(options);

        // Assert
        var project = await DbContext.Connection.QueryFirstAsync<Project>("SELECT * FROM Projects");
        var frame = await DbContext.Connection.QueryFirstAsync<Frame>("SELECT * FROM Frames");
        var tag = await DbContext.Connection.QueryFirstOrDefaultAsync<Tag>("SELECT * FROM Tags");

        tag.ShouldBeNull();
        project.Name.ShouldBe("project");
        frame.ProjectId.ShouldBe(project.Id);
        (DateTime.Now - frame.TimeAsDateTime).TotalSeconds.ShouldBeLessThan(3);
    }

    [Fact]
    public async Task Run_ShouldAddFrameAtNow_WithTags()
    {
        // Arrange
        AddOptions options = new()
        {
            Project = "project",
            Tags = ["tag1", "tag2"]
        };

        // Act
        await _sut.Run(options);

        // Assert
        var project = await DbContext.Connection.QueryFirstAsync<Project>("SELECT * FROM Projects");
        var frame = await DbContext.Connection.QueryFirstAsync<Frame>("SELECT * FROM Frames");
        var tags = await DbContext.Connection.QueryAsync<Tag>("SELECT * FROM Tags");
        var tagsLst = tags.ToList();
        var count = await DbContext.Connection.QueryFirstAsync<int>(
            $"SELECT COUNT(*) FROM Frames_Tags WHERE FrameId = '{frame.Id}' AND TagId IN ('{string.Join("', '", tagsLst.Select(e => e.Id))}')");

        tagsLst.Count.ShouldBe(2);
        project.Name.ShouldBe("project");
        frame.ProjectId.ShouldBe(project.Id);
        count.ShouldBe(2);
        (DateTime.Now - frame.TimeAsDateTime).TotalSeconds.ShouldBeLessThan(3);
    }

    [Fact]
    public async Task Run_ShouldAddFrameAtSpecifiedTime_WhenFromTimeSpecified()
    {
        // Arrange
        var fromTime = DateTime.Now.AddMinutes(-1);
        AddOptions options = new()
        {
            Project = "project",
            FromTime = fromTime.ToString("yyyy-MM-dd HH:mm")
        };

        // Act
        await _sut.Run(options);

        // Assert
        var frame = await DbContext.Connection.QueryFirstAsync<Frame>("SELECT * FROM Frames");
        (fromTime - frame.TimeAsDateTime).TotalMinutes.ShouldBeLessThan(1);
    }

    [Fact]
    public async Task Run_ShouldFail_WhenToTimeProvidedWithoutFromTime()
    {
        // Arrange
        var toTime = DateTime.Now.AddMinutes(-1);
        AddOptions options = new()
        {
            Project = "project",
            ToTime = toTime.ToString("yyyy-MM-dd HH:mm")
        };

        // Act
        var result = await _sut.Run(options);

        // Assert
        result.ShouldBe(1);
    }

    [Fact]
    public async Task Run_ShouldFail_WhenToTimeIsBeforeFromTime()
    {
        // Arrange
        var fromTime = DateTime.Now.AddMinutes(-1);
        var toTime = DateTime.Now.AddMinutes(-2);
        AddOptions options = new()
        {
            Project = "project",
            FromTime = fromTime.ToString("yyyy-MM-dd HH:mm"),
            ToTime = toTime.ToString("yyyy-MM-dd HH:mm")
        };

        // Act
        var result = await _sut.Run(options);

        // Assert
        result.ShouldBe(1);
    }

    [Fact]
    public async Task Run_ShouldFail_WhenToTimeIsInTheFuture()
    {
        // Arrange
        var toTime = DateTime.Now.AddMinutes(5);
        var fromTime = DateTime.Now.AddMinutes(-1);
        AddOptions options = new()
        {
            Project = "project",
            ToTime = toTime.ToString("yyyy-MM-dd HH:mm"),
            FromTime = fromTime.ToString("yyyy-MM-dd HH:mm")
        };

        // Act
        var result = await _sut.Run(options);

        // Assert
        result.ShouldBe(1);
    }

    [Fact]
    public async Task Run_ShouldFail_WhenFromTimeIsInTheFuture()
    {
        // Arrange
        var toTime = DateTime.Now.AddMinutes(7);
        var fromTime = DateTime.Now.AddMinutes(5);
        AddOptions options = new()
        {
            Project = "project",
            ToTime = toTime.ToString("yyyy-MM-dd HH:mm"),
            FromTime = fromTime.ToString("yyyy-MM-dd HH:mm")
        };

        // Act
        var result = await _sut.Run(options);

        // Assert
        result.ShouldBe(1);
    }

    [Fact]
    public async Task Run_ShouldFail_WhenProjectNotSpecified()
    {
        // Arrange
        AddOptions options = new()
        {
            Project = ""
        };

        // Act
        var result = await _sut.Run(options);

        // Assert
        result.ShouldBe(1);
    }

    [Fact]
    public async Task Run_ShouldFail_WhenUnableToParseFromTime()
    {
        // Arrange
        AddOptions options = new()
        {
            Project = "project",
            FromTime = "invalid"
        };

        // Act
        var result = await _sut.Run(options);

        // Assert
        result.ShouldBe(1);
    }

    [Fact]
    public async Task Run_ShouldFail_WhenUnableToParseToTime()
    {
        // Arrange
        AddOptions options = new()
        {
            Project = "project",
            ToTime = "invalid"
        };

        // Act
        var result = await _sut.Run(options);

        // Assert
        result.ShouldBe(1);
    }

    [Fact]
    public async Task Run_ShouldDisplayFrameStatusAfterCreation_WhenDateIsToday()
    {
        // Arrange
        await DbContext.Connection.ExecuteAsync(
            "INSERT INTO Frames (Id, Time, ProjectId) VALUES ('id', @Time, 'id')",
            new
            {
                Time = new DateTime(DateTime.Now.Year, DateTime.Now.Month, DateTime.Now.Day, 16, 0, 0).Ticks
            }
        );
        var options = new AddOptions
        {
            FromTime =
                new DateTime(DateTime.Now.Year, DateTime.Now.Month, DateTime.Now.Day, 15, 45, 0).ToString(
                    "yyyy-MM-dd HH:mm"),
            ToTime =
                new DateTime(DateTime.Now.Year, DateTime.Now.Month, DateTime.Now.Day, 15, 46, 0).ToString(
                    "yyyy-MM-dd HH:mm"),
            Project = "project",
            Tags = ["tag"]
        };

        // Act
        var result = await _sut.Run(options);
        var output = ConsoleHelper.GetMockOutput();

        // Assert
        var frameId = await DbContext.Connection.QueryFirstAsync<string>("SELECT Id FROM Frames");
        var expectedOutput =
            ConsoleHelper.GetSpectreMarkupOutput(
                $"{frameId}: [green]project[/] ([purple]tag[/]) added from [blue]15:45[/] to [blue]15:46[/] (00h 01m)");
        result.ShouldBe(0);
        output.ShouldStartWith(expectedOutput);
    }

    [Fact]
    public async Task Run_ShouldDisplayFrameStatusAfterCreation_WhenDateIsNotToday()
    {
        // Arrange
        await DbContext.Connection.ExecuteAsync(
            "INSERT INTO Frames (Id, Time, ProjectId) VALUES ('id', @Time, 'id')",
            new
            {
                Time = new DateTime(2025, 1, 2, 16, 0, 0).Ticks
            }
        );
        var options = new AddOptions
        {
            FromTime =
                new DateTime(2025, 1, 2, 15, 45, 0).ToString(
                    "yyyy-MM-dd HH:mm"),
            ToTime =
                new DateTime(2025, 1, 2, 15, 46, 0).ToString(
                    "yyyy-MM-dd HH:mm"),
            Project = "project",
            Tags = ["tag"]
        };

        // Act
        var result = await _sut.Run(options);
        var output = ConsoleHelper.GetMockOutput();

        // Assert
        var frameId = await DbContext.Connection.QueryFirstAsync<string>("SELECT Id FROM Frames");
        var expectedOutput =
            ConsoleHelper.GetSpectreMarkupOutput(
                $"{frameId}: [green]project[/] ([purple]tag[/]) added from [blue]2025-01-02 15:45[/] to [blue]2025-01-02 15:46[/] (00h 01m)");

        result.ShouldBe(0);
        output.ShouldStartWith(expectedOutput);
    }

    [Fact]
    public async Task Run_ShouldDisplayFrameStatusAfterCreation_WhenFrameIsRunning()
    {
        // Arrange


        var options = new AddOptions
        {
            FromTime =
                new DateTime(DateTime.Now.Year, DateTime.Now.Month, DateTime.Now.Day, 15, 45, 0).ToString(
                    "yyyy-MM-dd HH:mm"),
            Project = "project",
            Tags = ["tag"]
        };

        // Act
        var result = await _sut.Run(options);
        var output = ConsoleHelper.GetMockOutput();

        // Assert
        var frameId = await DbContext.Connection.QueryFirstAsync<string>("SELECT Id FROM Frames");
        var expectedOutput =
            ConsoleHelper.GetSpectreMarkupOutput(
                $"{frameId}: [green]project[/] ([purple]tag[/]) started at [blue]15:45[/]");
        result.ShouldBe(0);
        output.ShouldStartWith(expectedOutput);
    }

    [Fact]
    public async Task Run_ShouldFail_WhenFromTimeIsOutOfWorkHours()
    {
        // Arrange
        _settingsRepository.GetSettings()
            .Returns(new Settings
            {
                WorkTime =
                {
                    StartTime = new TimeSpan(1, 0, 0),
                    EndTime = new TimeSpan(16, 0, 0)
                }
            });

        var options = new AddOptions
        {
            FromTime =
                new DateTime(DateTime.Now.Year, DateTime.Now.Month, DateTime.Now.Day, 23, 0, 0)
                    .ToString("yyyy-MM-dd HH:mm"),
            Project = "project",
            Tags = ["tag"]
        };

        // Act
        var result = await _sut.Run(options);

        // Assert
        result.ShouldBe(1);
    }

    #endregion
}