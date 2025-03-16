using Dapper;
using NSubstitute;
using Shouldly;
using Watson.Commands;
using Watson.Core.Helpers;
using Watson.Core.Helpers.Abstractions;
using Watson.Core.Models.Database;
using Watson.Core.Repositories;
using Watson.Core.Repositories.Abstractions;
using Watson.Helpers;
using Watson.Models;
using Watson.Models.CommandLine;
using Watson.Tests.Abstractions;

namespace Watson.Tests.Tests.Commands;

public class ProjectCommandTests : CommandWithConsoleTest
{
    #region Members

    private readonly ISettingsRepository _settingsRepository = Substitute.For<ISettingsRepository>();
    private readonly ProjectCommand _sut;

    #endregion

    #region Constructors

    public ProjectCommandTests()
    {
        var idHelper = new IdHelper();

        var frameRepository = new FrameRepository(DbContext, idHelper);
        _sut = new ProjectCommand(
            new DependencyResolver(
                new ProjectRepository(DbContext, idHelper),
                frameRepository,
                new TagRepository(DbContext, idHelper),
                new TimeHelper(),
                new FrameHelper(frameRepository),
                _settingsRepository,
                new TodoRepository(DbContext, idHelper),
                ConsoleAdapter,
                Substitute.For<IAliasRepository>(),
                Substitute.For<IProcessHelper>()
            )
        );
    }

    #endregion

    #region Tests

    [Test]
    public async Task Run_ShouldListProjects()
    {
        // Arrange
        await DbContext.Connection.ExecuteAsync("INSERT INTO Projects (Id,Name) VALUES ('id1','project1')");
        await DbContext.Connection.ExecuteAsync("INSERT INTO Projects (Id,Name) VALUES ('id2','project2')");
        var options = new ProjectOptions
        {
            Action = "list",
        };

        // Act
        var result = await _sut.Run(options);

        // Assert
        result.ShouldBe(0);
        var lines = GetConsoleOutputLines();
        lines.Count.ShouldBe(2);
        lines[0].ShouldBe("id1: project1");
        lines[1].ShouldBe("id2: project2");
    }

    [Test]
    public async Task Run_ShouldDeleteProject()
    {
        // Arrange
        var options = new ProjectOptions
        {
            Action = "remove",
            Arguments = ["id"],
        };
        await DbContext.Connection.ExecuteAsync("INSERT INTO Projects (Id,Name) VALUES ('id','project')");

        // Act
        var result = await _sut.Run(options);

        // Assert
        result.ShouldBe(0);
        var count = DbContext.Connection.QueryFirst<int>("SELECT COUNT(*) FROM Projects WHERE Id = 'id'");
        count.ShouldBe(0);
    }

    [Test]
    public async Task Run_ShouldRenameProject()
    {
        // Arrange
        var options = new ProjectOptions()
        {
            Action = "rename",
            Arguments = ["id", "newName"],
        };
        await DbContext.Connection.ExecuteAsync("INSERT INTO Projects (Id,Name) VALUES ('id','project')");

        // Act
        var result = await _sut.Run(options);

        // Assert
        result.ShouldBe(0);
        var project =
            await DbContext.Connection.QueryFirstOrDefaultAsync<Project>(
                "SELECT * FROM Projects WHERE Name = 'newName'");
        project.ShouldNotBeNull();
    }

    [Test]
    public async Task Run_ShouldCreateProject_WhenDoesntExists()
    {
        // Arrange
        var options = new ProjectOptions()
        {
            Action = "create",
            Arguments = ["name"],
        };

        // Act
        var result = await _sut.Run(options);

        // Assert
        result.ShouldBe(0);
        var project =
            await DbContext.Connection.QueryFirstOrDefaultAsync<Project>(
                "SELECT * FROM Projects WHERE Name = 'name'");
        project.ShouldNotBeNull();
    }

    #endregion
}