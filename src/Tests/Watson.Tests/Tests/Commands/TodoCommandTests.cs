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

public class TodoCommandTests : ConsoleTest
{
    #region Members

    private readonly ISettingsRepository _settingsRepository = Substitute.For<ISettingsRepository>();
    private readonly TodoCommand _sut;

    #endregion

    #region Constructors

    public TodoCommandTests()
    {
        var idHelper = new IdHelper();

        var frameRepository = new FrameRepository(DbContext, idHelper);
        _sut = new TodoCommand(
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
    public async Task Run_ShouldAddTodo()
    {
        // Arrange
        var options = new TodoOptions
        {
            Action = "add",
            Arguments = ["description", "project"]
        };

        // Act
        var result = await _sut.Run(options);

        // Assert
        result.ShouldBe(0);

        var project = await DbContext.Connection.QueryFirstOrDefaultAsync<Project>(
            "SELECT * FROM Projects WHERE Name = 'project'");
        project.ShouldNotBeNull();

        var todo = await DbContext.Connection.QueryFirstOrDefaultAsync<Todo>(
            "SELECT * FROM Todos");
        todo.ShouldNotBeNull();
        todo.Description.ShouldBe("description");
        todo.ProjectId.ShouldBe(project.Id);
        todo.IsCompleted.ShouldBeFalse();
        todo.Priority.ShouldBeNull();
        todo.DueTime.ShouldBeNull();
        todo.DutTimeAsDateTime.ShouldBeNull();
    }

    [Fact]
    public async Task Run_ShouldAddTodo_WithTags()
    {
        // Arrange
        var options = new TodoOptions
        {
            Action = "add",
            Arguments = ["description", "project", "tag1", "tag2"]
        };

        // Act
        var result = await _sut.Run(options);

        // Assert
        result.ShouldBe(0);

        var tags = await DbContext.Connection.QueryAsync<Tag>("SELECT * FROM Tags");
        var tagsLst = tags.ToList();

        tagsLst.Count.ShouldBe(2);
        tagsLst[0].Name.ShouldBe("tag1");
        tagsLst[1].Name.ShouldBe("tag2");

        var nbTodoTags = await DbContext.Connection.QueryFirstAsync<int>("SELECT count(*) FROM Todos_Tags");
        nbTodoTags.ShouldBe(2);
    }

    [Fact]
    public async Task Run_ShouldAddTodo_WithPriority()
    {
        // Arrange
        var options = new TodoOptions
        {
            Action = "add",
            Arguments = ["description", "project"],
            Priority = 1
        };

        // Act
        var result = await _sut.Run(options);

        // Assert
        result.ShouldBe(0);

        var todo = await DbContext.Connection.QueryFirstOrDefaultAsync<Todo>(
            "SELECT * FROM Todos");
        todo.ShouldNotBeNull();
        todo.Priority.ShouldBe(1);
    }

    [Fact]
    public async Task Run_ShouldAddTodo_WithDueTime()
    {
        // Arrange
        var options = new TodoOptions
        {
            Action = "add",
            Arguments = ["description", "project"],
            DueTime = $"{DateTime.Now.Year + 1}-01-01"
        };

        // Act
        var result = await _sut.Run(options);

        // Assert
        result.ShouldBe(0);

        var todo = await DbContext.Connection.QueryFirstOrDefaultAsync<Todo>(
            "SELECT * FROM Todos");
        todo.ShouldNotBeNull();
        todo.DueTime.ShouldNotBeNull();
        todo.DutTimeAsDateTime.ShouldNotBeNull();
        todo.DutTimeAsDateTime!.Value.ToString("yyyy-MM-dd").ShouldBe(options.DueTime);
    }

    #endregion
}