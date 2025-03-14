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

public class TagCommandTests : CommandWithConsoleTest
{
    #region Members

    private readonly ISettingsRepository _settingsRepository = Substitute.For<ISettingsRepository>();
    private readonly TagCommand _sut;

    #endregion

    #region Constructors

    public TagCommandTests()
    {
        var idHelper = new IdHelper();

        var frameRepository = new FrameRepository(DbContext, idHelper);
        _sut = new TagCommand(
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

    [Fact]
    public async Task Run_ShouldListProjects()
    {
        // Arrange
        await DbContext.Connection.ExecuteAsync("INSERT INTO Tags (Id,Name) VALUES ('id1','tag1')");
        await DbContext.Connection.ExecuteAsync("INSERT INTO Tags (Id,Name) VALUES ('id2','tag2')");
        var options = new TagOptions
        {
            Action = "list",
        };

        // Act
        var result = await _sut.Run(options);

        // Assert
        result.ShouldBe(0);
        var lines = GetConsoleOutputLines();
        lines.Count.ShouldBe(2);
        lines[0].ShouldBe("id1: tag1");
        lines[1].ShouldBe("id2: tag2");
    }

    [Fact]
    public async Task Run_ShouldDeleteProject()
    {
        // Arrange
        var options = new TagOptions
        {
            Action = "remove",
            Arguments = ["id"],
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
    public async Task Run_ShouldRenameProject()
    {
        // Arrange
        var options = new TagOptions
        {
            Action = "rename",
            Arguments = ["id", "newName"],
        };
        await DbContext.Connection.ExecuteAsync("INSERT INTO Tags (Id,Name) VALUES ('id','tag')");

        // Act
        var result = await _sut.Run(options);

        // Assert
        result.ShouldBe(0);
        var tag =
            await DbContext.Connection.QueryFirstOrDefaultAsync<Tag>(
                "SELECT * FROM Tags WHERE Name = 'newName'");
        tag.ShouldNotBeNull();
    }

    [Fact]
    public async Task Run_ShouldCreateProject_WhenDoesntExists()
    {
        // Arrange
        var options = new TagOptions
        {
            Action = "create",
            Arguments = ["name"],
        };

        // Act
        var result = await _sut.Run(options);

        // Assert
        result.ShouldBe(0);
        var tag =
            await DbContext.Connection.QueryFirstOrDefaultAsync<Tag>(
                "SELECT * FROM Tags WHERE Name = 'name'");
        tag.ShouldNotBeNull();
    }

    #endregion
}