using Dapper;
using NSubstitute;
using Shouldly;
using Watson.Commands;
using Watson.Core.Helpers;
using Watson.Core.Repositories;
using Watson.Core.Repositories.Abstractions;
using Watson.Helpers;
using Watson.Models;
using Watson.Models.CommandLine;
using Watson.Tests.Abstractions;

namespace Watson.Tests.Tests.Commands;

public class ListCommandTests : ConsoleTest
{
    #region Members

    private readonly ISettingsRepository _settingsRepository = Substitute.For<ISettingsRepository>();
    private readonly ListCommand _sut;

    #endregion

    #region Constructors

    public ListCommandTests()
    {
        var idHelper = new IdHelper();

        var frameRepository = new FrameRepository(DbContext, idHelper);
        _sut = new ListCommand(
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
    public async Task Run_ShouldListProjects()
    {
        // Arrange
        await DbContext.Connection.ExecuteAsync("INSERT INTO Projects (Id,Name) VALUES ('id1','project1')");
        await DbContext.Connection.ExecuteAsync("INSERT INTO Projects (Id,Name) VALUES ('id2','project2')");
        var options = new ListOptions
        {
            Resource = "project"
        };

        // Act
        var result = await _sut.Run(options);

        // Assert
        result.ShouldBe(0);
        var lines = ConsoleHelper.GetMockOutputAsLines();
        lines.Length.ShouldBe(2);
        lines[0].ShouldBe("id1: project1");
        lines[1].ShouldBe("id2: project2");
    }

    [Fact]
    public async Task Run_ShouldListTags()
    {
        // Arrange
        await DbContext.Connection.ExecuteAsync("INSERT INTO Tags (Id,Name) VALUES ('id1','tag1')");
        await DbContext.Connection.ExecuteAsync("INSERT INTO Tags (Id,Name) VALUES ('id2','tag2')");
        var options = new ListOptions
        {
            Resource = "tag"
        };

        // Act
        var result = await _sut.Run(options);

        // Assert
        result.ShouldBe(0);
        var lines = ConsoleHelper.GetMockOutputAsLines();
        lines.Length.ShouldBe(2);
        lines[0].ShouldBe("id1: tag1");
        lines[1].ShouldBe("id2: tag2");
    }

    [Fact]
    public async Task Run_ShouldFail_WhenResourceDoesNotExist()
    {
        // Arrange
        var options = new ListOptions
        {
            Resource = "resource"
        };

        // Act
        var result = await _sut.Run(options);

        // Assert
        result.ShouldBe(1);
        ConsoleHelper.GetMockOutput().ShouldBeEmpty();
    }

    #endregion
}