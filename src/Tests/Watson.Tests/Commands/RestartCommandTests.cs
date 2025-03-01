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

public class RestartCommandTests : IDisposable
{
    #region Members

    private readonly AppDbContext _dbContext;
    private readonly string _dbFilePath = Path.GetTempFileName();
    private readonly ISettingsRepository _settingsRepository = Substitute.For<ISettingsRepository>();
    private readonly RestartCommand _sut;

    #endregion

    #region Constructors

    public RestartCommandTests()
    {
        var idHelper = new IdHelper();
        _dbContext = new AppDbContext($"Data Source={_dbFilePath};Cache=Shared;Pooling=False");

        var frameRepository = new FrameRepository(_dbContext, idHelper);
        _sut = new RestartCommand(
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
    public async Task Run_ShouldRestartCurrentFrame()
    {
        // Arrange
        var options = new RestartOptions
        {
        };
        await _dbContext.Connection.ExecuteAsync("INSERT INTO Frames (Id,ProjectId,Time) VALUES ('id','id',1)");

        // Act
        var result = await _sut.Run(options);

        // Assert
        result.ShouldBe(0);
        var frame = await _dbContext.Connection.QueryFirstAsync<Frame>("SELECT * FROM Frames");
        (DateTime.Now - frame.TimeAsDateTime).TotalMinutes.ShouldBeLessThan(1);
    }

    [Fact]
    public async Task Run_ShouldRestartSpecifiedFrame()
    {
        // Arrange
        var options = new RestartOptions
        {
            FrameId = "id"
        };
        await _dbContext.Connection.ExecuteAsync("INSERT INTO Frames (Id,ProjectId,Time) VALUES ('id','id',1)");
        await _dbContext.Connection.ExecuteAsync("INSERT INTO Tags (Id,Name) VALUES ('id','tag')");
        await _dbContext.Connection.ExecuteAsync("INSERT INTO Frames_Tags (Id,FrameId,TagId) VALUES ('id','id','id')");

        // Act
        var result = await _sut.Run(options);

        // Assert
        result.ShouldBe(0);
        var frame = await _dbContext.Connection.QueryFirstAsync<Frame>(
            "SELECT * FROM Frames WHERE Id <> 'id'"
        );
        (DateTime.Now - frame.TimeAsDateTime).TotalMinutes.ShouldBeLessThan(1);

        var frameTag = await _dbContext.Connection.QueryFirstAsync<int>(
            $"SELECT COUNT(*) FROM Frames_Tags WHERE FrameId = '{frame.Id}'"
        );
        frameTag.ShouldBe(1);
    }

    #endregion
}