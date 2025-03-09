using Dapper;
using NSubstitute;
using Shouldly;
using Watson.Commands;
using Watson.Core;
using Watson.Core.Helpers;
using Watson.Core.Models.Database;
using Watson.Core.Repositories;
using Watson.Core.Repositories.Abstractions;
using Watson.Helpers;
using Watson.Models;
using Watson.Models.CommandLine;
using Watson.Tests.Abstractions;

namespace Watson.Tests.Tests.Commands;

public class RenameCommandTests : CommandTest
{
    #region Members

    private readonly ISettingsRepository _settingsRepository = Substitute.For<ISettingsRepository>();
    private readonly RenameCommand _sut;

    #endregion

    #region Constructors

    public RenameCommandTests()
    {
        var idHelper = new IdHelper();

        var frameRepository = new FrameRepository(DbContext, idHelper);
        _sut = new RenameCommand(
            new DependencyResolver(
                new ProjectRepository(DbContext, idHelper),
                frameRepository,
                new TagRepository(DbContext, idHelper),
                new TimeHelper(),
                new FrameHelper(frameRepository),
                _settingsRepository,
                new TodoRepository(DbContext, idHelper)
            )
        );
    }

    #endregion

    #region Tests

    [Fact]
    public async Task Run_ShouldRenameProject()
    {
        // Arrange
        var options = new RenameOptions
        {
            Resource = "project",
            ResourceId = "id",
            Name = "newName"
        };
        await DbContext.Connection.ExecuteAsync("INSERT INTO Projects (Id,Name) VALUES ('id','project')");

        // Act
        var result = await _sut.Run(options);

        // Assert
        result.ShouldBe(0);
        var project =
            await DbContext.Connection.QueryFirstOrDefaultAsync<Project>(
                "SELECT * FROM Projects WHERE Name = 'newName'");
        project.ShouldNotBeNull();
    }

    [Fact]
    public async Task Run_ShouldRenameTag()
    {
        // Arrange
        var options = new RenameOptions
        {
            Resource = "tag",
            ResourceId = "id",
            Name = "newName"
        };
        await DbContext.Connection.ExecuteAsync("INSERT INTO Tags (Id,Name) VALUES ('id','tag')");

        // Act
        var result = await _sut.Run(options);

        // Assert
        result.ShouldBe(0);
        var tag = await DbContext.Connection.QueryFirstOrDefaultAsync<Tag>(
            "SELECT * FROM Tags WHERE Name = 'newName'");
        tag.ShouldNotBeNull();
    }

    [Fact]
    public async Task Run_ShouldFail_WhenResourceNotSpecified()
    {
        // Arrange
        var options = new RenameOptions
        {
            Resource = "",
            ResourceId = "id",
            Name = "newName"
        };

        // Act
        var result = await _sut.Run(options);

        // Assert
        result.ShouldBe(1);
    }

    [Fact]
    public async Task Run_ShouldFail_WhenResourceIsInvalid()
    {
        // Arrange
        var options = new RenameOptions
        {
            Resource = "invalid",
            ResourceId = "id",
            Name = "newName"
        };

        // Act
        var result = await _sut.Run(options);

        // Assert
        result.ShouldBe(1);
    }

    [Fact]
    public async Task Run_ShouldFail_WhenResourceIdIsNotSpecified()
    {
        // Arrange
        var options = new RenameOptions
        {
            Resource = "project",
            ResourceId = "",
            Name = "newName"
        };

        // Act
        var result = await _sut.Run(options);

        // Assert
        result.ShouldBe(1);
    }

    [Fact]
    public async Task Run_ShouldFail_WhenNameIsNotSpecified()
    {
        // Arrange
        var options = new RenameOptions
        {
            Resource = "project",
            ResourceId = "id",
            Name = ""
        };

        // Act
        var result = await _sut.Run(options);

        // Assert
        result.ShouldBe(1);
    }

    #endregion
}