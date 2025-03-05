using Dapper;
using NSubstitute;
using Shouldly;
using Watson.Commands;
using Watson.Core;
using Watson.Core.Helpers;
using Watson.Core.Models.Database;
using Watson.Core.Models.Settings;
using Watson.Core.Repositories;
using Watson.Core.Repositories.Abstractions;
using Watson.Helpers;
using Watson.Models;
using Watson.Models.CommandLine;
using Watson.Tests.Abstractions;

namespace Watson.Tests.Tests.Commands;

public class StartCommandTests : CommandTest
{
    #region Members

    private readonly ISettingsRepository _settingsRepository = Substitute.For<ISettingsRepository>();
    private readonly StartCommand _sut;

    #endregion

    #region Constructors

    public StartCommandTests()
    {
        var idHelper = new IdHelper();

        _settingsRepository.GetSettings()
            .Returns(new Settings());

        var frameRepository = new FrameRepository(DbContext, idHelper);
        _sut = new StartCommand(
            new DependencyResolver(
                new ProjectRepository(DbContext, idHelper),
                frameRepository,
                new TagRepository(DbContext, idHelper),
                new TimeHelper(),
                new FrameHelper(frameRepository),
                _settingsRepository
            )
        );
    }

    #endregion

    #region Tests

    [Fact]
    public async Task Run_ShouldAddFrameAtNow()
    {
        // Arrange
        var options = new StartOptions
        {
            Project = "project",
            Tags = ["tag1", "tag2"]
        };

        // Act
        var result = await _sut.Run(options);

        // Assert
        result.ShouldBe(0);
        var frame = await DbContext.Connection.QueryFirstAsync<Frame>("SELECT * FROM Frames");
        var project = await DbContext.Connection.QueryFirstAsync<Project>("SELECT * FROM Projects");
        var tags = await DbContext.Connection.QueryAsync<Tag>("SELECT * FROM Tags");
        tags.Count().ShouldBe(2);
        project.Name.ShouldBe("project");
        frame.ProjectId.ShouldBe(project.Id);
        (DateTime.Now - frame.TimeAsDateTime).TotalMinutes.ShouldBeLessThan(1);
    }

    [Fact]
    public async Task Run_ShouldAddFrameAtSpecifiedTime_WhenFromTimeSpecified()
    {
        // Arrange
        var fromTime = DateTime.Now.AddMinutes(-1);
        var options = new StartOptions
        {
            Project = "project",
            FromTime = fromTime.ToString("yyyy-MM-dd HH:mm")
        };

        // Act
        await _sut.Run(options);

        // Assert
        var frame = await DbContext.Connection.QueryFirstAsync<Frame>("SELECT * FROM Frames");
        (fromTime - frame.TimeAsDateTime).TotalMinutes.ShouldBeLessThan(1);
    }

    [Fact]
    public async Task Run_ShouldAddFrameAtSpecifiedTime_WhenAtTimeSpecified()
    {
        // Arrange
        var fromTime = DateTime.Now.AddMinutes(-1);
        var options = new StartOptions
        {
            Project = "project",
            AtTime = fromTime.ToString("yyyy-MM-dd HH:mm")
        };

        // Act
        await _sut.Run(options);

        // Assert
        var frame = await DbContext.Connection.QueryFirstAsync<Frame>("SELECT * FROM Frames");
        (fromTime - frame.TimeAsDateTime).TotalMinutes.ShouldBeLessThan(1);
    }

    #endregion
}