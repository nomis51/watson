using Dapper;
using NSubstitute;
using Shouldly;
using Watson.Commands;
using Watson.Core.Helpers;
using Watson.Core.Helpers.Abstractions;
using Watson.Core.Models.Database;
using Watson.Core.Repositories;
using Watson.Core.Repositories.Abstractions;
using Watson.Helpers;
using Watson.Models;
using Watson.Models.CommandLine;
using Watson.Tests.Abstractions;

namespace Watson.Tests.Tests.Commands;

public class EditCommandTests : CommandWithConsoleTest
{
    #region Members

    private readonly ISettingsRepository _settingsRepository = Substitute.For<ISettingsRepository>();
    private readonly EditCommand _sut;

    #endregion

    #region Constructors

    public EditCommandTests()
    {
        var idHelper = new IdHelper();

        var frameRepository = new FrameRepository(DbContext, idHelper);
        _sut = new EditCommand(
            new DependencyResolver(
                new ProjectRepository(DbContext, idHelper),
                frameRepository,
                new TagRepository(DbContext, idHelper),
                new TimeHelper(),
                new FrameHelper(frameRepository),
                _settingsRepository,
                new TodoRepository(DbContext, idHelper),
                ConsoleAdapter,
                Substitute.For<IAliasRepository>(),
                Substitute.For<IProcessHelper>()
            )
        );
    }

    #endregion

    #region Tests

    [Test]
    public async Task Run_ShouldEditProject()
    {
        // Arrange
        var time = new DateTime(2022, 1, 1, 15, 45, 0);
        var options = new EditOptions
        {
            FrameId = "id",
            Project = "newName",
            Tags = ["newTag"],
            FromTime = time.ToString("yyyy-MM-dd HH:mm")
        };
        await DbContext.Connection.ExecuteAsync("INSERT INTO Frames (Id,ProjectId,Time) VALUES ('id','id',1)");

        // Act
        var result = await _sut.Run(options);

        // Assert
        result.ShouldBe(0);
        var project =
            await DbContext.Connection.QueryFirstOrDefaultAsync<Project>(
                "SELECT * FROM Projects WHERE Name = 'newName'");
        project.ShouldNotBeNull();
        var frame = await DbContext.Connection.QueryFirstAsync<Frame>("SELECT * FROM Frames");
        frame.ProjectId.ShouldBe(project.Id);
        frame.TimeAsDateTime.ShouldBe(time);

        var tags = await DbContext.Connection.QueryAsync<Tag>("SELECT * FROM Tags");
        var tagsLst = tags.ToList();
        var count = await DbContext.Connection.QueryFirstAsync<int>(
            $"SELECT COUNT(*) FROM Frames_Tags WHERE FrameId = '{frame.Id}' AND TagId IN ('{string.Join("', '", tagsLst.Select(e => e.Id))}')");
        count.ShouldBe(1);
    }

    [Test]
    public async Task Run_ShouldFail_WhenFrameDoesNotExist()
    {
        // Arrange
        var options = new EditOptions
        {
            FrameId = "id",
            Project = "newName"
        };

        // Act
        var result = await _sut.Run(options);

        // Assert
        result.ShouldBe(1);
    }

    [Test]
    public async Task Run_ShouldFail_WhenNowFrameDoesNotExist()
    {
        // Arrange
        var options = new EditOptions
        {
            Project = "newName"
        };

        // Act
        var result = await _sut.Run(options);

        // Assert
        result.ShouldBe(1);
    }

    [Test]
    public async Task Run_ShouldUpdateTime_WhenFromTimeIsSpecified()
    {
        // Arrange
        var fromTime = DateTime.Now.AddMinutes(-5);
        var options = new EditOptions
        {
            FrameId = "id",
            Project = "newName",
            FromTime = fromTime.ToString("yyyy-MM-dd HH:mm")
        };
        await DbContext.Connection.ExecuteAsync("INSERT INTO Frames (Id,ProjectId,Time) VALUES ('id','id',1)");

        // Act
        var result = await _sut.Run(options);

        // Assert
        result.ShouldBe(0);
        var frame = await DbContext.Connection.QueryFirstAsync<Frame>("SELECT * FROM Frames");
        (frame.TimeAsDateTime - fromTime).TotalMinutes.ShouldBeLessThan(1);
    }

    #endregion
}