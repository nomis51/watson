using Dapper;
using NSubstitute;
using Shouldly;
using Watson.Commands;
using Watson.Core.Helpers;
using Watson.Core.Models.Database;
using Watson.Core.Repositories;
using Watson.Core.Repositories.Abstractions;
using Watson.Helpers;
using Watson.Models;
using Watson.Models.CommandLine;
using Watson.Tests.Abstractions;

namespace Watson.Tests.Tests.Commands;

public class StopCommandTests : CommandWithConsoleTest
{
    #region Members

    private readonly ISettingsRepository _settingsRepository = Substitute.For<ISettingsRepository>();
    private readonly StopCommand _sut;

    #endregion

    #region Constructors

    public StopCommandTests()
    {
        var idHelper = new IdHelper();

        var frameRepository = new FrameRepository(DbContext, idHelper);
        _sut = new StopCommand(
            new DependencyResolver(
                new ProjectRepository(DbContext, idHelper),
                frameRepository,
                new TagRepository(DbContext, idHelper),
                new TimeHelper(),
                new FrameHelper(frameRepository),
                _settingsRepository,
                new TodoRepository(DbContext, idHelper),
                ConsoleAdapter
            )
        );
    }

    #endregion

    #region Tests

    [Test]
    public async Task Run_ShouldStopCurrentFrame()
    {
        // Arrange
        var options = new StopOptions();

        // Act
        var result = await _sut.Run(options);

        // Assert
        result.ShouldBe(0);
        var frame = await DbContext.Connection.QueryFirstOrDefaultAsync<Frame>("SELECT * FROM Frames");
        frame.ShouldNotBeNull();
        frame.ProjectId.ShouldBeEmpty();
        (DateTime.Now - frame.TimeAsDateTime).TotalMinutes.ShouldBeLessThan(1);
    }

    [Test]
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
        var frame = await DbContext.Connection.QueryFirstOrDefaultAsync<Frame>("SELECT * FROM Frames");
        frame.ShouldNotBeNull();
        frame.ProjectId.ShouldBeEmpty();
        (atTime - frame.TimeAsDateTime).TotalMinutes.ShouldBeLessThan(1);
    }

    #endregion
}