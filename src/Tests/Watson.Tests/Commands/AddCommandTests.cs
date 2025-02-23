using System.Runtime.InteropServices;
using NSubstitute;
using Shouldly;
using Watson.Commands;
using Watson.Core.Models;
using Watson.Core.Repositories.Abstractions;
using Watson.Models.Abstractions;
using Watson.Models.CommandLine;

namespace Watson.Tests.Commands;

public class AddCommandTests
{
    #region Members

    private readonly IFrameRepository _frameRepository = Substitute.For<IFrameRepository>();
    private readonly ITagRepository _tagRepository = Substitute.For<ITagRepository>();
    private readonly IProjectRepository _projectRepository = Substitute.For<IProjectRepository>();
    private readonly AddCommand _sut;

    #endregion

    #region Constructors

    public AddCommandTests()
    {
        var dependencyResolver = Substitute.For<IDependencyResolver>();
        dependencyResolver.FrameRepository
            .Returns(_frameRepository);
        dependencyResolver.TagRepository
            .Returns(_tagRepository);
        dependencyResolver.ProjectRepository
            .Returns(_projectRepository);

        _frameRepository.InsertAsync(Arg.Any<Frame>())
            .Returns(true);

        _sut = new AddCommand(dependencyResolver);
    }

    #endregion

    #region Tests

    [InlineData("")]
    [InlineData(null)]
    [Theory]
    public async Task Run_ShouldFail_WhenProjectMissing(string? project)
    {
        // Arrange
        var options = new AddOptions
        {
            Project = project!
        };

        // Act
        var result = await _sut.Run(options);

        // Assert
        result.ShouldNotBe(0);
    }

    [Fact]
    public async Task Run_ShouldSucceed()
    {
        // Arrange
        var options = new AddOptions
        {
            Project = "project"
        };

        // Act
        var result = await _sut.Run(options);

        // Assert
        result.ShouldBe(0);
    }

    [InlineData("3-27 1345", "$year-03-27 13:45")]
    [InlineData("8-28 13", "$year-08-28 13:00")]
    [InlineData("13", "$year-$month-$day 13:00")]
    [InlineData("1345", "$year-$month-$day 13:45")]
    [InlineData("0945", "$year-$month-$day 09:45")]
    [InlineData("945", "$year-$month-$day 09:45")]
    [InlineData("124", "$year-$month-$day 12:04")]
    [InlineData("078", "$year-$month-$day 07:08")]
    [InlineData("708", "$year-$month-$day 07:08")]
    [InlineData("9-9", "$year-09-09 00:00")]
    [InlineData("13 13", "$year-$month-13 13:00")]
    [InlineData("2025-12-09", "2025-12-09 00:00")]
    [InlineData("13:45", "$year-$month-$day 13:45")]
    [InlineData("09:45", "$year-$month-$day 09:45")]
    [InlineData("9:45", "$year-$month-$day 09:45")]
    [InlineData("13:8", "$year-$month-$day 13:08")]
    [InlineData("8:8", "$year-$month-$day 08:08")]
    [Theory]
    public async Task Run_ShouldSucceed_WithTheRightFromTime(string fromTime, string expectedTime)
    {
        // Arrange
        expectedTime = expectedTime.Replace("$year", DateTime.UtcNow.Year.ToString())
            .Replace("$month", DateTime.UtcNow.Month.ToString().PadLeft(2, '0'))
            .Replace("$day", DateTime.UtcNow.Day.ToString().PadLeft(2, '0'));

        var options = new AddOptions
        {
            Project = "project",
            FromTime = fromTime
        };
        string? actualFromTime = null;
        _frameRepository.WhenForAnyArgs(e => e.InsertAsync(Arg.Any<Frame>()))
            .Do(e => actualFromTime = DateTimeOffset.FromUnixTimeSeconds(e.Arg<Frame>().Timestamp)
                .LocalDateTime
                .ToString("yyyy-MM-dd HH:mm"));

        // Act
        var result = await _sut.Run(options);

        // Assert
        result.ShouldBe(0);
        actualFromTime.ShouldNotBeNull();
        actualFromTime.ShouldBe(expectedTime);
    }

    #endregion
}