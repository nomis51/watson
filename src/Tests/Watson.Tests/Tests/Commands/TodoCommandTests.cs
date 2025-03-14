using Dapper;
using NSubstitute;
using Shouldly;
using Spectre.Console;
using Spectre.Console.Rendering;
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

public class TodoCommandTests : CommandWithConsoleTest
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
                new TodoRepository(DbContext, idHelper),
                ConsoleAdapter
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
        todo.DueTimeAsDateTime.ShouldBeNull();
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
        todo.DueTimeAsDateTime.ShouldNotBeNull();
        todo.DueTimeAsDateTime!.Value.ToString("yyyy-MM-dd").ShouldBe(options.DueTime);
    }

    [Fact]
    public async Task Run_ShouldRemoveTodo_WhenExists()
    {
        // Arrange
        var options = new TodoOptions
        {
            Action = "remove",
            Arguments = ["id"]
        };
        await DbContext.Connection.ExecuteAsync(
            "INSERT INTO Todos (Id,Description,ProjectId,DueTime,Priority) VALUES ('id','description','id', null, null)");

        // Act
        var result = await _sut.Run(options);

        // Assert
        result.ShouldBe(0);

        var todo = await DbContext.Connection.QueryFirstOrDefaultAsync<Todo>(
            "SELECT * FROM Todos WHERE Id = 'id'");
        todo.ShouldBeNull();
    }

    [Fact]
    public async Task Run_ShouldFailToRemove_WhenTodoDoesNotExist()
    {
        // Arrange
        var options = new TodoOptions
        {
            Action = "remove",
            Arguments = ["id"]
        };

        // Act
        var result = await _sut.Run(options);

        // Assert
        result.ShouldBe(1);
    }

    [Fact]
    public async Task Run_ShouldListTodos()
    {
        // Arrange
        var time = DateTime.Now;
        await DbContext.Connection.ExecuteAsync(
            $"INSERT INTO Todos (Id,Description,ProjectId,DueTime,Priority) VALUES ('id1','description1','id1', {time.Ticks}, 3)");
        await DbContext.Connection.ExecuteAsync("INSERT INTO Projects (Id,Name) VALUES ('id1','project1')");
        await DbContext.Connection.ExecuteAsync("INSERT INTO Tags (Id,Name) VALUES ('id1','tag1')");
        await DbContext.Connection.ExecuteAsync("INSERT INTO Todos_Tags (Id,TodoId,TagId) VALUES ('id1','id1','id1')");

        var options = new TodoOptions
        {
            Action = "list"
        };

        // Act
        var result = await _sut.Run(options);

        // Assert
        result.ShouldBe(0);
        var output = GetConsoleOutput();

        var expectedTable = CreateTable([
            [
                new Text("id1"),
                new Text("description1"),
                new Markup("[green]project1[/]"),
                new Markup("[purple]tag1[/]"),
                new Markup("[gray]-[/]").Centered(),
                new Markup("3").Centered(),
                new Markup(time.ToString("yyyy-MM-dd HH:mm")).Centered()
            ]
        ]);
        var expectedOutput = GenerateSpectreRenderableOutput(expectedTable);
        output.ShouldBe(expectedOutput);
    }

    [InlineData(true)]
    [InlineData(false)]
    [Theory]
    public async Task Run_ShouldToggleCompletion_WhenTodoExists(bool isCompleted)
    {
        // Arrange
        var originalIsCompleted = !isCompleted;
        await DbContext.Connection.ExecuteAsync(
            $"INSERT INTO Todos (Id,Description,ProjectId,DueTime,Priority,IsCompleted) VALUES ('id1','description1','id1', null, null, {originalIsCompleted})");

        var options = new TodoOptions
        {
            Action = originalIsCompleted ? "uncomplete" : "complete",
            Arguments = ["id1"]
        };

        // Act
        var result = await _sut.Run(options);

        // Assert
        result.ShouldBe(0);
        var todo = await DbContext.Connection.QueryFirstOrDefaultAsync<Todo>(
            "SELECT * FROM Todos WHERE Id = 'id1'");
        todo.ShouldNotBeNull();
        todo.IsCompleted.ShouldBe(isCompleted);
    }

    [InlineData("complete", true, false)]
    [InlineData("done", true, false)]
    [InlineData("uncomplete", false, true)]
    [InlineData("undone", false, true)]
    [InlineData("undo", false, true)]
    [InlineData("reset", false, true)]
    [Theory]
    public async Task Run_ShouldToggleCompletion_WithDifferentAliases(string argument, bool expected, bool initial)
    {
        // Arrange
        await DbContext.Connection.ExecuteAsync(
            $"INSERT INTO Todos (Id,Description,ProjectId,DueTime,Priority,IsCompleted) VALUES ('id1','description1','id1', null, null, {initial})");

        var options = new TodoOptions
        {
            Action = argument,
            Arguments = ["id1"]
        };

        // Act
        var result = await _sut.Run(options);

        // Assert
        result.ShouldBe(0);
        var todo = await DbContext.Connection.QueryFirstOrDefaultAsync<Todo>(
            "SELECT * FROM Todos WHERE Id = 'id1'");
        todo.ShouldNotBeNull();
        todo.IsCompleted.ShouldBe(expected);
    }

    #endregion

    #region Private methods

    private static Table CreateTable(IEnumerable<IEnumerable<IRenderable>> rows)
    {
        var table = new Table();
        table.Border(TableBorder.Rounded);
        table.AddColumn("ID");
        table.AddColumn("Description");
        table.AddColumn("Project");
        table.AddColumn("Tags");
        table.AddColumn("Completed")
            .Centered();
        table.AddColumn("Priority")
            .Centered();
        table.AddColumn("Due Time");

        foreach (var row in rows)
        {
            table.AddRow(row);
        }

        return table;
    }

    [Fact]
    public async Task Run_ShouldEditTodo_WhenTodoExists()
    {
        // Arrange
        await DbContext.Connection.ExecuteAsync(
            $"INSERT INTO Todos (Id,Description,ProjectId,DueTime,Priority) VALUES ('id1','description1','id1', {DateTime.Today.Ticks}, 3)");

        var options = new TodoOptions
        {
            Action = "edit",
            Arguments = ["id1", "description2", "project2", "tag2"],
            DueTime = DateTime.Now.AddDays(1).ToString("yyyy-MM-dd HH:mm"),
            Priority = 2
        };

        // Act
        var result = await _sut.Run(options);

        // Assert
        result.ShouldBe(0);
        var todo = await DbContext.Connection.QueryFirstOrDefaultAsync<Todo>(
            "SELECT * FROM Todos WHERE Id = 'id1'");
        var project = await DbContext.Connection.QueryFirstOrDefaultAsync<Project>(
            "SELECT * FROM Projects WHERE Name = 'project2'");
        project.ShouldNotBeNull();
        todo.ShouldNotBeNull();
        todo.Description.ShouldBe("description2");
        todo.ProjectId.ShouldBe(project.Id);
        todo.DueTime!.Value.ShouldBeGreaterThan(DateTime.Today.AddDays(1).Ticks);
        todo.Priority.ShouldBe(2);
    }

    #endregion
}