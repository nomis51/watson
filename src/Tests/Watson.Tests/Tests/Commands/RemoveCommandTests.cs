using Dapper;
using NSubstitute;
using Shouldly;
using Watson.Commands;
using Watson.Core;
using Watson.Core.Helpers;
using Watson.Core.Repositories;
using Watson.Core.Repositories.Abstractions;
using Watson.Helpers;
using Watson.Models;
using Watson.Models.CommandLine;
using Watson.Tests.Abstractions;

namespace Watson.Tests.Tests.Commands;

public class RemoveCommandTests : CommandTest, IDisposable
{
    #region Members

    private readonly ISettingsRepository _settingsRepository = Substitute.For<ISettingsRepository>();
    private readonly RemoveCommand _sut;

    #endregion

    #region Constructors

    public RemoveCommandTests()
    {
        var idHelper = new IdHelper();

        var frameRepository = new FrameRepository(DbContext, idHelper);
        _sut = new RemoveCommand(
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

    public new void Dispose()
    {
        base.Dispose();
        GC.SuppressFinalize(this);
    }

    #endregion

    #region Tests

    [Fact]
    public async Task Run_ShouldDeleteProject()
    {
        // Arrange
        var options = new RemoveOptions
        {
            Resource = "project",
            ResourceId = "id"
        };
        await DbContext.Connection.ExecuteAsync("INSERT INTO Projects (Id,Name) VALUES ('id','project')");

        // Act
        var result = await _sut.Run(options);

        // Assert
        result.ShouldBe(0);
        var count = DbContext.Connection.QueryFirst<int>("SELECT COUNT(*) FROM Projects WHERE Id = 'id'");
        count.ShouldBe(0);
    }

    [Fact]
    public async Task Run_ShouldDeleteTag()
    {
        // Arrange
        var options = new RemoveOptions
        {
            Resource = "tag",
            ResourceId = "id"
        };
        await DbContext.Connection.ExecuteAsync("INSERT INTO Tags (Id,Name) VALUES ('id','tag')");

        // Act
        var result = await _sut.Run(options);

        // Assert
        result.ShouldBe(0);
        var count = DbContext.Connection.QueryFirst<int>("SELECT COUNT(*) FROM Tags WHERE Id = 'id'");
        count.ShouldBe(0);
    }

    [Fact]
    public async Task Run_ShouldDeleteFrame()
    {
        // Arrange
        var options = new RemoveOptions
        {
            Resource = "frame",
            ResourceId = "id"
        };
        await DbContext.Connection.ExecuteAsync("INSERT INTO Frames (Id,ProjectId,Time) VALUES ('id','id',1)");

        // Act
        var result = await _sut.Run(options);

        // Assert
        result.ShouldBe(0);
        var count = DbContext.Connection.QueryFirst<int>("SELECT COUNT(*) FROM Frames WHERE Id = 'id'");
        count.ShouldBe(0);
    }

    [Fact]
    public async Task Run_ShouldFail_WhenResourceIsInvalid()
    {
        // Arrange
        var options = new RemoveOptions
        {
            Resource = "invalid",
            ResourceId = "id"
        };

        // Act
        var result = await _sut.Run(options);

        // Assert
        result.ShouldBe(1);
    }

    [Fact]
    public async Task Run_ShouldFail_WhenResourceIdIsInvalid()
    {
        // Arrange
        var options = new RemoveOptions
        {
            Resource = "project",
            ResourceId = "invalid"
        };

        // Act
        var result = await _sut.Run(options);

        // Assert
        result.ShouldBe(1);
    }

    [Fact]
    public async Task Run_ShouldFail_WhenResourceNotProvided()
    {
        // Arrange
        var options = new RemoveOptions
        {
            Resource = "",
            ResourceId = "id"
        };

        // Act
        var result = await _sut.Run(options);

        // Assert
        result.ShouldBe(1);
    }

    [Fact]
    public async Task Run_ShouldFail_WhenResourceIdNotProvided()
    {
        // Arrange
        var options = new RemoveOptions
        {
            Resource = "project",
            ResourceId = ""
        };

        // Act
        var result = await _sut.Run(options);

        // Assert
        result.ShouldBe(1);
    }

    #endregion
}