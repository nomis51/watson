using Microsoft.Extensions.Logging;
using NSubstitute;
using Shouldly;
using Watson.Core.Models.Database;
using Watson.Core.Repositories.Abstractions;
using Watson.Helpers.Abstractions;
using Watson.Models;
using Watson.Tests.Abstractions;

namespace Watson.Tests.Tests;

public class CliTests : CommandWithConsoleTest
{
    #region Members

    private readonly Cli _sut;

    #endregion

    #region Cosntructors

    public CliTests()
    {
        var projectRepository = Substitute.For<IProjectRepository>();
        projectRepository.GetAsync()
            .Returns([
                new Project
                {
                    Id = "id1",
                    Name = "project1"
                }
            ]);

        var tagRepository = Substitute.For<ITagRepository>();
        tagRepository.GetAsync()
            .Returns([
                new Tag
                {
                    Id = "id1",
                    Name = "tag1"
                }
            ]);

        _sut = new Cli(new DependencyResolver(
            projectRepository,
            Substitute.For<IFrameRepository>(),
            tagRepository,
            Substitute.For<ITimeHelper>(),
            Substitute.For<IFrameHelper>(),
            Substitute.For<ISettingsRepository>(),
            Substitute.For<ITodoRepository>(),
            ConsoleAdapter
        ), Substitute.For<ILogger<Cli>>());
    }

    #endregion

    #region Tests

    #region Add

    [Test]
    public async Task Run_Add_ShouldComplete_WithProject()
    {
        // Arrange
        var args = GetCompletionArgs("add", "proj");

        // Act
        var result = await _sut.Run(args);

        // Assert
        result.ShouldBe(0);
        GetConsoleOutput().ShouldBe("project1");
    }

    [Test]
    public async Task Run_Add_ShouldComplete_WithTag()
    {
        // Arrange
        var args = GetCompletionArgs("add", "project", "ta");

        // Act
        var result = await _sut.Run(args);

        // Assert
        result.ShouldBe(0);
        GetConsoleOutput().ShouldBe("tag1");
    }

    [Test]
    public async Task Run_Add_ShouldComplete_WithManyTags()
    {
        // Arrange
        var args = GetCompletionArgs("add", "project", "ta");

        // Act
        var result = await _sut.Run(args);

        // Assert
        result.ShouldBe(0);
        GetConsoleOutput().ShouldBe("tag1");
    }

    #endregion

    #region Edit

    [Test]
    public async Task Run_Edit_ShouldComplete_WithProject()
    {
        // Arrange
        var args = GetCompletionArgs("edit", "proj");

        // Act
        var result = await _sut.Run(args);

        // Assert
        result.ShouldBe(0);
        GetConsoleOutput().ShouldBe("project1");
    }

    [Test]
    public async Task Run_Edit_ShouldComplete_WithTag()
    {
        // Arrange
        var args = GetCompletionArgs("edit", "project", "ta");

        // Act
        var result = await _sut.Run(args);

        // Assert
        result.ShouldBe(0);
        GetConsoleOutput().ShouldBe("tag1");
    }

    [Test]
    public async Task Run_Edit_ShouldComplete_WithManyTags()
    {
        // Arrange
        var args = GetCompletionArgs("edit", "project", "ta");

        // Act
        var result = await _sut.Run(args);

        // Assert
        result.ShouldBe(0);
        GetConsoleOutput().ShouldBe("tag1");
    }

    #endregion

    #region Project

    [Arguments("a", "add")]
    [Arguments("c", "create")]
    [Arguments("d", "delete")]
    [Arguments("r", "remove")]
    [Arguments("rem", "remove")]
    [Arguments("ren", "rename")]
    [Arguments("l", "list")]
    [Test]
    public async Task Run_Project_ShouldComplete_WithAction(string input, string expected)
    {
        // Arrange
        var args = GetCompletionArgs("project", input);

        // Act
        var result = await _sut.Run(args);

        // Assert
        result.ShouldBe(0);
        GetConsoleOutput().ShouldBe(expected);
    }

    [Arguments("add")]
    [Arguments("create")]
    [Arguments("delete")]
    [Arguments("remove")]
    [Arguments("rename")]
    [Test]
    public async Task Run_Project_ShouldComplete_WithProject(string input)
    {
        // Arrange
        var args = GetCompletionArgs("project", input, "proj");

        // Act
        var result = await _sut.Run(args);

        // Assert
        result.ShouldBe(0);
        GetConsoleOutput().ShouldBe("project1");
    }

    #endregion

    #region Start

    [Test]
    public async Task Run_Start_ShouldComplete_WithProject()
    {
        // Arrange
        var args = GetCompletionArgs("start", "proj");

        // Act
        var result = await _sut.Run(args);

        // Assert
        result.ShouldBe(0);
        GetConsoleOutput().ShouldBe("project1");
    }

    [Test]
    public async Task Run_Start_ShouldComplete_WithTag()
    {
        // Arrange
        var args = GetCompletionArgs("start", "project", "ta");

        // Act
        var result = await _sut.Run(args);

        // Assert
        result.ShouldBe(0);
        GetConsoleOutput().ShouldBe("tag1");
    }

    [Test]
    public async Task Run_Start_ShouldComplete_WithManyTags()
    {
        // Arrange
        var args = GetCompletionArgs("start", "project", "ta");

        // Act
        var result = await _sut.Run(args);

        // Assert
        result.ShouldBe(0);
        GetConsoleOutput().ShouldBe("tag1");
    }

    #endregion

    #region Stats

    [Arguments("proj", "projects")]
    [Test]
    public async Task Run_Stats_ShouldComplete_WithStatsType(string input, string expected)
    {
        // Arrange
        var args = GetCompletionArgs("stats", input);

        // Act
        var result = await _sut.Run(args);

        // Assert
        result.ShouldBe(0);
        GetConsoleOutput().ShouldBe(expected);
    }

    #endregion

    #endregion

    #region Private methods

    private string[] GetCompletionArgs(params string[] args)
    {
        return [Cli.CompletionCommandName, $"{nameof(Watson).ToLower()} {string.Join(' ', args)}"];
    }

    #endregion
}