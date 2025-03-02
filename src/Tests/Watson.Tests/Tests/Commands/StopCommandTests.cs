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

namespace Watson.Tests.Tests.Commands;

public class StopCommandTests : IDisposable
{
    #region Members

    private readonly AppDbContext _dbContext;
    private readonly string _dbFilePath = Path.GetTempFileName();
    private readonly ISettingsRepository _settingsRepository = Substitute.For<ISettingsRepository>();
    private readonly StopCommand _sut;

    #endregion

    #region Constructors

    public StopCommandTests()
    {
        var idHelper = new IdHelper();
        _dbContext = new AppDbContext($"Data Source={_dbFilePath};Cache=Shared;Pooling=False");

        var frameRepository = new FrameRepository(_dbContext, idHelper);
        _sut = new StopCommand(
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
    public async Task Run_ShouldStopCurrentFrame()
    {
        // Arrange
        var options = new StopOptions();

        // Act
        var result = await _sut.Run(options);

        // Assert
        result.ShouldBe(0);
        var frame = await _dbContext.Connection.QueryFirstOrDefaultAsync<Frame>("SELECT * FROM Frames");
        frame.ShouldNotBeNull();
        frame.ProjectId.ShouldBeEmpty();
        (DateTime.Now - frame.TimeAsDateTime).TotalMinutes.ShouldBeLessThan(1);
    }

    [Fact]
    public async Task Run_ShouldStopFrameAtSpecifiedTime_WhenFromTimeSpecified()
    {
        // Arrange
        var atTime = DateTime.Now.AddMinutes(-1);
        var options = new StopOptions
        {
            AtTime = atTime.ToString("yyyy-MM-dd HH:mm")
        };

        // Act
        var result = await _sut.Run(options);

        // Assert
        result.ShouldBe(0);
        var frame = await _dbContext.Connection.QueryFirstOrDefaultAsync<Frame>("SELECT * FROM Frames");
        frame.ShouldNotBeNull();
        frame.ProjectId.ShouldBeEmpty();
        (atTime - frame.TimeAsDateTime).TotalMinutes.ShouldBeLessThan(1);
    }

    #endregion
}