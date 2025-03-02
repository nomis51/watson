using Dapper;
using NSubstitute;
using Shouldly;
using Watson.Commands;
using Watson.Core;
using Watson.Core.Helpers;
using Watson.Core.Models.Database;
using Watson.Core.Models.Settings;
using Watson.Core.Repositories;
using Watson.Core.Repositories.Abstractions;
using Watson.Helpers;
using Watson.Models;
using Watson.Models.CommandLine;

namespace Watson.Tests.Tests.Commands;

public class AddCommandTests : IDisposable
{
    #region Members

    private readonly AppDbContext _dbContext;
    private readonly string _dbFilePath = Path.GetTempFileName();
    private readonly ISettingsRepository _settingsRepository = Substitute.For<ISettingsRepository>();
    private readonly AddCommand _sut;

    #endregion

    #region Constructors

    public AddCommandTests()
    {
        var idHelper = new IdHelper();
        _dbContext = new AppDbContext($"Data Source={_dbFilePath};Cache=Shared;Pooling=False");

        _settingsRepository.GetSettings()
            .Returns(new Settings());

        var frameRepository = new FrameRepository(_dbContext, idHelper);
        _sut = new AddCommand(
            new DependencyResolver(
                new ProjectRepository(_dbContext, idHelper),
                frameRepository,
                new TagRepository(_dbContext, idHelper),
                new TimeHelper(),
                new FrameHelper(frameRepository),
                _settingsRepository
            )
        );
    }

    public void Dispose()
    {
        GC.SuppressFinalize(this);
        _dbContext.Connection.Close();
        _dbContext.Connection.Dispose();

        if (File.Exists(_dbFilePath))
        {
            File.Delete(_dbFilePath);
        }
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
        var project = await _dbContext.Connection.QueryFirstAsync<Project>("SELECT * FROM Projects");
        var frame = await _dbContext.Connection.QueryFirstAsync<Frame>("SELECT * FROM Frames");
        var tag = await _dbContext.Connection.QueryFirstOrDefaultAsync<Tag>("SELECT * FROM Tags");

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
        var project = await _dbContext.Connection.QueryFirstAsync<Project>("SELECT * FROM Projects");
        var frame = await _dbContext.Connection.QueryFirstAsync<Frame>("SELECT * FROM Frames");
        var tags = await _dbContext.Connection.QueryAsync<Tag>("SELECT * FROM Tags");

        tags.Count().ShouldBe(2);
        project.Name.ShouldBe("project");
        frame.ProjectId.ShouldBe(project.Id);
        (DateTime.Now - frame.TimeAsDateTime).TotalSeconds.ShouldBeLessThan(3);
    }

    [Fact]
    public async Task Run_ShouldAddFrameAtSpecifedTime_WhenFromTimeSpecified()
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
        var frame = await _dbContext.Connection.QueryFirstAsync<Frame>("SELECT * FROM Frames");
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

    #endregion
}