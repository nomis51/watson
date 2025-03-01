using Dapper;
using NSubstitute;
using Shouldly;
using Watson.Commands;
using Watson.Core;
using Watson.Core.Helpers;
using Watson.Core.Models.Database;
using Watson.Core.Repositories;
using Watson.Core.Repositories.Abstractions;
using Watson.Helpers;
using Watson.Models;
using Watson.Models.CommandLine;

namespace Watson.Tests.Commands;

public class EditCommandTests : IDisposable
{
    #region Members

    private readonly AppDbContext _dbContext;
    private readonly string _dbFilePath = Path.GetTempFileName();
    private readonly ISettingsRepository _settingsRepository = Substitute.For<ISettingsRepository>();
    private readonly EditCommand _sut;

    #endregion

    #region Constructors

    public EditCommandTests()
    {
        var idHelper = new IdHelper();
        _dbContext = new AppDbContext($"Data Source={_dbFilePath};Cache=Shared;Pooling=False");

        var frameRepository = new FrameRepository(_dbContext, idHelper);
        _sut = new EditCommand(
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
    public async Task Run_ShouldEditProject()
    {
        // Arrange
        var options = new EditOptions
        {
            FrameId = "id",
            Project = "newName"
        };
        await _dbContext.Connection.ExecuteAsync("INSERT INTO Frames (Id,ProjectId,Time) VALUES ('id','id',1)");

        // Act
        var result = await _sut.Run(options);

        // Assert
        result.ShouldBe(0);
        var project =
            await _dbContext.Connection.QueryFirstOrDefaultAsync<Project>(
                "SELECT * FROM Projects WHERE Name = 'newName'");
        project.ShouldNotBeNull();
        var frame = await _dbContext.Connection.QueryFirstAsync<Frame>("SELECT * FROM Frames");
        frame.ProjectId.ShouldBe(project.Id);
    }

    [Fact]
    public async Task Run_ShouldFail_WhenFrameDoesNotExist()
    {
        // Arrange
        var options = new EditOptions
        {
            FrameId = "id",
            Project = "newName"
        };

        // Act
        var result = await _sut.Run(options);

        // Assert
        result.ShouldBe(1);
    }

    [Fact]
    public async Task Run_ShouldFail_WhenNowFrameDoesNotExist()
    {
        // Arrange
        var options = new EditOptions
        {
            Project = "newName"
        };

        // Act
        var result = await _sut.Run(options);

        // Assert
        result.ShouldBe(1);
    }

    [Fact]
    public async Task Run_ShouldUpdateTime_WhenFromTimeIsSpecified()
    {
        // Arrange
        var fromTime = DateTime.Now.AddMinutes(-5);
        var options = new EditOptions
        {
            FrameId = "id",
            Project = "newName",
            FromTime = fromTime.ToString("yyyy-MM-dd HH:mm")
        };
        await _dbContext.Connection.ExecuteAsync("INSERT INTO Frames (Id,ProjectId,Time) VALUES ('id','id',1)");

        // Act
        var result = await _sut.Run(options);

        // Assert
        result.ShouldBe(0);
        var frame = await _dbContext.Connection.QueryFirstAsync<Frame>("SELECT * FROM Frames");
        (frame.TimeAsDateTime - fromTime).TotalMinutes.ShouldBeLessThan(1);
    }

    #endregion
}