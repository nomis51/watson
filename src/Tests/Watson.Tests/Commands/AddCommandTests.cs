using NSubstitute;
using Shouldly;
using Watson.Commands;
using Watson.Core.Models;
using Watson.Core.Repositories.Abstractions;
using Watson.Helpers;
using Watson.Models.Abstractions;
using Watson.Models.CommandLine;

namespace Watson.Tests.Commands;

public class AddCommandTests
{
    #region Members

    private readonly IFrameRepository _frameRepository = Substitute.For<IFrameRepository>();
    private readonly IProjectRepository _projectRepository = Substitute.For<IProjectRepository>();
    private readonly ITagRepository _tagRepository = Substitute.For<ITagRepository>();
    private readonly AddCommand _sut;
    private readonly List<Frame> _frames = [];

    #endregion

    #region Constructors

    public AddCommandTests()
    {
        _frameRepository.InsertAsync(Arg.Any<Frame>())
            .Returns(e =>
            {
                _frames.Add(e.Arg<Frame>());
                return true;
            });

        _projectRepository.EnsureNameExistsAsync(Arg.Any<string>())
            .Returns(e => new Project
            {
                Id = "idProject",
                Name = e.Arg<string>()
            });

        _tagRepository.EnsureTagsExistsAsync(Arg.Any<IEnumerable<string>>())
            .Returns(e => true);

        var dependencyResolver = Substitute.For<IDependencyResolver>();
        dependencyResolver.FrameRepository
            .Returns(_frameRepository);
        dependencyResolver.TimeHelper
            .Returns(new TimeHelper());
        dependencyResolver.FrameHelper
            .Returns(new FrameHelper(_frameRepository));
        dependencyResolver.ProjectRepository
            .Returns(_projectRepository);
        dependencyResolver.TagRepository
            .Returns(_tagRepository);

        _sut = new AddCommand(dependencyResolver);
    }

    #endregion

    #region Tests

    [Fact]
    public async Task Run_ShouldAddFrameAtNow_WithNoTag()
    {
        // Arrange
        AddOptions options = new()
        {
            Project = "project"
        };

        // Act
        await _sut.Run(options);

        // Assert
        _frames.Count.ShouldBe(1);
        _frames[0].ProjectId.ShouldBe("project");
        (DateTimeOffset.UtcNow.ToUnixTimeSeconds() - _frames[0].Timestamp).ShouldBeLessThan(2);
    }

    #endregion
}