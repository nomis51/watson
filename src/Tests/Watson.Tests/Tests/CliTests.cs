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
    public async Task Run_Start_ShouldCompelte_WithManyTags()
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

    #region Private methods

    private string[] GetCompletionArgs(params string[] args)
    {
        return [Cli.CompletionCommandName, $"{nameof(Watson).ToLower()} {string.Join(' ', args)}"];
    }

    #endregion
}