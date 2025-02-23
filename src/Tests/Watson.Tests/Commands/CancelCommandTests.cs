using NSubstitute;
using Shouldly;
using Watson.Commands;
using Watson.Core.Models;
using Watson.Core.Repositories.Abstractions;
using Watson.Models.Abstractions;
using Watson.Models.CommandLine;

namespace Watson.Tests.Commands;

public class CancelCommandTests
{
    #region Members

    private readonly IFrameRepository _frameRepository = Substitute.For<IFrameRepository>();
    private readonly CancelCommand _sut;

    #endregion

    #region Constructors

    public CancelCommandTests()
    {
        var dependencyResolver = Substitute.For<IDependencyResolver>();
        dependencyResolver.FrameRepository
            .Returns(_frameRepository);
        _sut = new CancelCommand(dependencyResolver);
    }

    #endregion

    #region Tests

    [Fact]
    public async Task Run_ShouldSucceed()
    {
        // Arrange
        var options = new CancelOptions();
        _frameRepository.GetPreviousFrameAsync(Arg.Any<DateTimeOffset>())
            .Returns(new Frame
            {
                Id = "id",
                Timestamp = 1
            });
        _frameRepository.UpdateAsync(Arg.Any<Frame>())
            .Returns(true);

        // Act
        var result = await _sut.Run(options);

        // Assert
        result.ShouldBe(0);
        await _frameRepository.Received()
            .UpdateAsync(Arg.Is<Frame>(f => f.Id == "id" &&
                                            f.ProjectId == string.Empty &&
                                            f.Timestamp == 1));
    }

    [Fact]
    public async Task Run_ShouldFail_WhenNoPreviousFrameExists()
    {
        // Arrange
        var options = new CancelOptions();
        _frameRepository.GetPreviousFrameAsync(Arg.Any<DateTimeOffset>())
            .Returns(default(Frame));

        // Act
        var result = await _sut.Run(options);

        // Assert
        result.ShouldBe(1);
    }

    #endregion
}