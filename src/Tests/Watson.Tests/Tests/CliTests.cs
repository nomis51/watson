﻿using Dapper;
using Microsoft.Extensions.Logging;
using NSubstitute;
using Shouldly;
using Watson.Core.Helpers;
using Watson.Core.Helpers.Abstractions;
using Watson.Core.Models.Database;
using Watson.Core.Repositories;
using Watson.Core.Repositories.Abstractions;
using Watson.Helpers.Abstractions;
using Watson.Models;
using Watson.Tests.Abstractions;

namespace Watson.Tests.Tests;

public class CliTests : CommandWithConsoleTest
{
    #region Members

    private readonly Cli _sut;
    private readonly IFrameRepository _frameRepository = Substitute.For<IFrameRepository>();

    #endregion

    #region Constructors

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
            _frameRepository,
            tagRepository,
            Substitute.For<ITimeHelper>(),
            Substitute.For<IFrameHelper>(),
            Substitute.For<ISettingsRepository>(),
            ConsoleAdapter,
            new AliasRepository(DbContext, new IdHelper()),
            Substitute.For<IProcessHelper>()
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

    #region Alias

    [Arguments("c", "create")]
    [Arguments("s", "set")]
    [Arguments("a", "add")]
    [Arguments("r", "remove")]
    [Arguments("d", "delete")]
    [Test]
    public async Task Run_Alias_ShouldComplete_WithAction(string input, string expected)
    {
        // Arrange
        var args = GetCompletionArgs("alias", input);

        // Act
        var result = await _sut.Run(args);

        // Assert
        result.ShouldBe(0);
        GetConsoleOutput().ShouldBe(expected);
    }

    [Test]
    public async Task Run_Alias_ShouldExecuteCommandAssociatedWithAlias()
    {
        // Arrange
        await DbContext.Connection.ExecuteAsync(
            "INSERT INTO Aliases (Id, Name, Command) VALUES ('id1', 'test', 'status')");

        // Act
        var result = await _sut.Run(["test"]);

        // Assert
        result.ShouldBe(1);
        await _frameRepository.Received(1)
            .GetPreviousFrameAsync(Arg.Any<DateTime>());
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

    #region Tag

    [Arguments("a", "add")]
    [Arguments("c", "create")]
    [Arguments("d", "delete")]
    [Arguments("r", "remove")]
    [Arguments("rem", "remove")]
    [Arguments("ren", "rename")]
    [Arguments("l", "list")]
    [Test]
    public async Task Run_Tag_ShouldComplete_WithAction(string input, string expected)
    {
        // Arrange
        var args = GetCompletionArgs("tag", input);

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
    public async Task Run_Tag_ShouldComplete_WithProject(string input)
    {
        // Arrange
        var args = GetCompletionArgs("tag", input, "proj");

        // Act
        var result = await _sut.Run(args);

        // Assert
        result.ShouldBe(0);
        GetConsoleOutput().ShouldBe("project1");
    }

    #endregion

    #region Workhours

    [Arguments("r", "reset")]
    [Arguments("s", "start")]
    [Arguments("e", "end")]
    [Test]
    public async Task Run_Workhours_ShouldComplete_WithType(string input, string expected)
    {
        // Arrange
        var args = GetCompletionArgs("workhours", input);

        // Act
        var result = await _sut.Run(args);

        // Assert
        result.ShouldBe(0);
        GetConsoleOutput().ShouldBe(expected);
    }

    #endregion

    [Arguments("help")]
    [Arguments("--help")]
    [Test]
    public async Task Run_Help_ShouldPrintHelp(string input)
    {
        // Arrange

        // Act
        var result = await _sut.Run([input]);

        // Assert
        result.ShouldBe(0);
        var output = GetConsoleOutput();
        output.ShouldStartWith(nameof(Watson));
    }

    [Test]
    public async Task Run_Help_ShouldPrintHelp_WithSubcommand()
    {
        // Arrange

        // Act
        var result = await _sut.Run(["status", "--help"]);

        // Assert
        result.ShouldBe(0);
        var output = GetConsoleOutput();
        output.ShouldStartWith(nameof(Watson));
    }

    [Test]
    public async Task Run_Version_ShouldPrintVersion()
    {
        // Arrange

        // Act
        var result = await _sut.Run(["version"]);

        // Assert
        result.ShouldBe(0);
        var output = GetConsoleOutput();
        output.ShouldStartWith(nameof(Watson));
    }
    
    #endregion

    #region Private methods

    private static string[] GetCompletionArgs(params string[] args)
    {
        return [Cli.CompletionCommandName, $"{nameof(Watson).ToLower()} {string.Join(' ', args)}"];
    }

    #endregion
}