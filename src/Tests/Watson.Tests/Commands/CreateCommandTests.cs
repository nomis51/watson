using NSubstitute;
using Shouldly;
using Watson.Commands;
using Watson.Core.Models;
using Watson.Core.Repositories.Abstractions;
using Watson.Models.Abstractions;
using Watson.Models.CommandLine;

namespace Watson.Tests.Commands;

public class CreateCommandTests
{
    #region Members

    private readonly IProjectRepository _projectRepository = Substitute.For<IProjectRepository>();
    private readonly ITagRepository _tagRepository = Substitute.For<ITagRepository>();
    private readonly CreateCommand _sut;

    #endregion

    #region Constructors

    public CreateCommandTests()
    {
        var dependencyResolver = Substitute.For<IDependencyResolver>();
        dependencyResolver.ProjectRepository.Returns(_projectRepository);
        dependencyResolver.TagRepository.Returns(_tagRepository);

        _projectRepository.DoesNameExistAsync(Arg.Any<string>())
            .Returns(false);
        _projectRepository.InsertAsync(Arg.Any<Project>())
            .Returns(true);

        _tagRepository.DoesNameExistAsync(Arg.Any<string>())
            .Returns(false);
        _tagRepository.InsertAsync(Arg.Any<Tag>())
            .Returns(true);

        _sut = new CreateCommand(dependencyResolver);
    }

    #endregion

    #region Tests

    [InlineData("")]
    [InlineData(null)]
    [Theory]
    public async Task Run_ShouldFail_WhenResourceMissing(string? resource)
    {
        // Arrange
        var options = new CreateOptions
        {
            Resource = resource!,
            Name = "name"
        };

        // Act
        var result = await _sut.Run(options);

        // Assert
        result.ShouldNotBe(0);
    }

    [InlineData("")]
    [InlineData(null)]
    [Theory]
    public async Task Run_ShouldFail_WhenNameMissing(string name)
    {
        // Arrange
        var options = new CreateOptions
        {
            Resource = "project",
            Name = name
        };

        // Act
        var result = await _sut.Run(options);

        // Assert
        result.ShouldNotBe(0);
    }

    [Fact]
    public async Task Run_ShouldFail_WhenResourceIsInvalid()
    {
        // Arrange
        var options = new CreateOptions
        {
            Resource = "invalid",
            Name = "name"
        };

        // Act
        var result = await _sut.Run(options);

        // Assert
        result.ShouldNotBe(0);
    }

    [InlineData("project")]
    [InlineData("tag")]
    [Theory]
    public async Task Run_ShouldSucceed(string resource)
    {
        // Arrange
        var options = new CreateOptions
        {
            Resource = resource,
            Name = "name"
        };

        // Act
        var result = await _sut.Run(options);

        // Assert
        result.ShouldBe(0);
    }

    [InlineData("project")]
    [InlineData("tag")]
    [Theory]
    public async Task Run_ShouldFail_WhenResourceAlreadyExists(string resource)
    {
        // Arrange
        var options = new CreateOptions
        {
            Resource = resource,
            Name = "name"
        };

        _projectRepository.DoesNameExistAsync(Arg.Any<string>())
            .Returns(true);
        _tagRepository.DoesNameExistAsync(Arg.Any<string>())
            .Returns(true);

        // Act
        var result = await _sut.Run(options);

        // Assert
        result.ShouldNotBe(0);
    }

    #endregion
}