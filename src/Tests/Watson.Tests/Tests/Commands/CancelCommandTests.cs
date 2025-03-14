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

public class CancelCommandTests : CommandTest
{
    #region Members

    private readonly ISettingsRepository _settingsRepository = Substitute.For<ISettingsRepository>();
    private readonly CancelCommand _sut;

    #endregion

    #region Constructors

    public CancelCommandTests()
    {
        var idHelper = new IdHelper();

        var frameRepository = new FrameRepository(DbContext, idHelper);
        _sut = new CancelCommand(
            new DependencyResolver(
                new ProjectRepository(DbContext, idHelper),
                frameRepository,
                new TagRepository(DbContext, idHelper),
                new TimeHelper(),
                new FrameHelper(frameRepository),
                _settingsRepository,
                new TodoRepository(DbContext, idHelper)
            )
        );
    }

    #endregion

    #region Tests

    [Fact]
    public async Task Run_ShouldCancelLastFrame_WhenItExists()
    {
        // Arrange
        await DbContext.Connection.ExecuteAsync("INSERT INTO Frames (Id,ProjectId,Time) VALUES ('id','id',1)");
        var options = new CancelOptions();

        // Act
        var result = await _sut.Run(options);

        // Assert
        result.ShouldBe(0);
        var frame = await DbContext.Connection.QueryFirstOrDefaultAsync<Frame>("SELECT * FROM Frames");
        frame.ShouldBeNull();
    }

    [Fact]
    public async Task Run_ShouldFail_WhenLastFrameDoesNotExist()
    {
        // Arrange
        var options = new CancelOptions();

        // Act
        var result = await _sut.Run(options);

        // Assert
        result.ShouldBe(1);
    }

    [Fact]
    public async Task Run_ShouldFail_WhenLastFrameIsAnEmptyFrame()
    {
        // Arrange
        await DbContext.Connection.ExecuteAsync("INSERT INTO Frames (Id,ProjectId,Time) VALUES ('id','',0)");
        var options = new CancelOptions();

        // Act
        var result = await _sut.Run(options);

        // Assert
        result.ShouldBe(1);
    }

    #endregion
}