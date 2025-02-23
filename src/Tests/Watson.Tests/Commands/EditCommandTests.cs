using Microsoft.AspNetCore.Hosting.Server.Features;
using NSubstitute;
using Shouldly;
using Watson.Commands;
using Watson.Core.Models;
using Watson.Core.Repositories.Abstractions;
using Watson.Helpers.Abstractions;
using Watson.Models.Abstractions;
using Watson.Models.CommandLine;

namespace Watson.Tests.Commands;

public class EditCommandTests
{
    #region Members

    private readonly ITimeHelper _timeHelper = Substitute.For<ITimeHelper>();
    private readonly IProjectRepository _projectRepository = Substitute.For<IProjectRepository>();
    private readonly IFrameRepository _frameRepository = Substitute.For<IFrameRepository>();
    private readonly EditCommand _sut;

    #endregion

    #region Constructors

    public EditCommandTests()
    {
        var dependencyResolver = Substitute.For<IDependencyResolver>();
        dependencyResolver.FrameRepository
            .Returns(_frameRepository);
        dependencyResolver.ProjectRepository
            .Returns(_projectRepository);
        dependencyResolver.TimeHelper
            .Returns(_timeHelper);

        _sut = new EditCommand(dependencyResolver);
    }

    #endregion

    #region Tests

    [Fact]
    public async Task Run_ShouldUseExistingFrame_WhenFrameIdProvided()
    {
        // Arrange
        var options = new EditOptions
        {
            FrameId = "sut"
        };
        _frameRepository.GetByIdAsync(Arg.Any<string>())
            .Returns(new Frame());

        // Act
        var result = await _sut.Run(options);

        // Assert
        await _frameRepository.Received()
            .GetByIdAsync("sut");
    }

    [Fact]
    public async Task Run_ShouldUseLatestFrame_WhenNoFrameIdProvided()
    {
        // Arrange
        var options = new EditOptions();
        _frameRepository.GetPreviousFrameAsync(Arg.Any<DateTimeOffset>())
            .Returns(new Frame());

        // Act
        var result = await _sut.Run(options);

        // Assert
        await _frameRepository.Received()
            .GetPreviousFrameAsync(Arg.Any<DateTimeOffset>());
    }

    [Fact]
    public async Task Run_ShouldFail_WhenGetFrameFails()
    {
        // Arrange
        var options = new EditOptions();
        _frameRepository.GetPreviousFrameAsync(Arg.Any<DateTimeOffset>())
            .Returns(default(Frame));

        // Act
        var result = await _sut.Run(options);

        // Assert
        result.ShouldBe(1);
    }

    [Fact]
    public async Task Run_ShouldUpdateProject_WhenProjectProvided()
    {
        // Arrange
        var options = new EditOptions
        {
            Project = "sut"
        };
        _projectRepository.EnsureNameExistsAsync(Arg.Any<string>())
            .Returns(new Project
            {
                Id = "id"
            });
        _frameRepository.GetPreviousFrameAsync(Arg.Any<DateTimeOffset>())
            .Returns(new Frame());
        _frameRepository.UpdateAsync(Arg.Any<Frame>())
            .Returns(true);

        // Act
        var result = await _sut.Run(options);

        // Assert
        result.ShouldBe(0);
        await _projectRepository.Received()
            .EnsureNameExistsAsync("sut");
        await _frameRepository.Received()
            .UpdateAsync(Arg.Is<Frame>(f => f.ProjectId == "id"));
    }

    [Fact]
    public async Task Run_ShouldUpdateTimestamp_WhenFromTimeProvided()
    {
        // Arrange
        var fromTime = DateTimeOffset.UtcNow.Date;
        var options = new EditOptions
        {
            FromTime = fromTime.ToString("yyyy-MM-dd HH:mm")
        };
        _frameRepository.GetPreviousFrameAsync(Arg.Any<DateTimeOffset>())
            .Returns(new Frame());
        _frameRepository.UpdateAsync(Arg.Any<Frame>())
            .Returns(true);
        _timeHelper.ParseDateTime(Arg.Any<string>(), out Arg.Any<DateTimeOffset?>())
            .Returns(e =>
            {
                e[1] = new DateTimeOffset(fromTime);
                return true;
            });
        _projectRepository.EnsureNameExistsAsync(Arg.Any<string>())
            .Returns(new Project());

        // Act
        var result = await _sut.Run(options);

        // Assert
        result.ShouldBe(0);
        await _frameRepository.Received()
            .UpdateAsync(Arg.Is<Frame>(f => f.Timestamp == new DateTimeOffset(fromTime).ToUnixTimeSeconds()));
    }

    #endregion
}