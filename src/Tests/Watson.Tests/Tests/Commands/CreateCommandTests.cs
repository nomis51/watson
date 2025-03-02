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

public class CreateCommandTests : CommandTest, IDisposable
{
    #region Members

    private readonly ISettingsRepository _settingsRepository = Substitute.For<ISettingsRepository>();
    private readonly CreateCommand _sut;

    #endregion

    #region Constructors

    public CreateCommandTests()
    {
        var idHelper = new IdHelper();

        var frameRepository = new FrameRepository(DbContext, idHelper);
        _sut = new CreateCommand(
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

    public new void Dispose()
    {
        base.Dispose();
        GC.SuppressFinalize(this);
    }

    #endregion

    #region Tests

    [Fact]
    public async Task Run_ShouldCreateProject_WhenDoesntExists()
    {
        // Arrange
        var options = new CreateOptions
        {
            Resource = "project",
            Name = "project"
        };

        // Act
        var result = await _sut.Run(options);

        // Assert
        result.ShouldBe(0);
        var project =
            await DbContext.Connection.QueryFirstOrDefaultAsync<Project>(
                "SELECT * FROM Projects WHERE Name = 'project'");
        project.ShouldNotBeNull();
    }

    [Fact]
    public async Task Run_ShouldFail_WhenProjectAlreadyExists()
    {
        // Arrange
        var options = new CreateOptions
        {
            Resource = "project",
            Name = "project"
        };
        await DbContext.Connection.ExecuteAsync("INSERT INTO Projects (Id,Name) VALUES ('id','project')");


        // Act
        var result = await _sut.Run(options);

        // Assert
        result.ShouldBe(1);
        var count = DbContext.Connection.QueryFirst<int>("SELECT COUNT(*) FROM Projects WHERE Name = 'project'");
        count.ShouldBe(1);
    }

    [Fact]
    public async Task Run_ShouldCreateTag_WhenDoesntExists()
    {
        // Arrange
        var options = new CreateOptions
        {
            Resource = "tag",
            Name = "tag"
        };

        // Act
        var result = await _sut.Run(options);

        // Assert
        result.ShouldBe(0);
        var tag =
            await DbContext.Connection.QueryFirstOrDefaultAsync<Tag>(
                "SELECT * FROM Tags WHERE Name = 'tag'");
        tag.ShouldNotBeNull();
    }

    [Fact]
    public async Task Run_ShouldFail_WhenTagAlreadyExists()
    {
        // Arrange
        var options = new CreateOptions
        {
            Resource = "tag",
            Name = "tag"
        };
        await DbContext.Connection.ExecuteAsync("INSERT INTO Tags (Id,Name) VALUES ('id','tag')");


        // Act
        var result = await _sut.Run(options);

        // Assert
        result.ShouldBe(1);
        var count = DbContext.Connection.QueryFirst<int>("SELECT COUNT(*) FROM Tags WHERE Name = 'tag'");
        count.ShouldBe(1);
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
        result.ShouldBe(1);
    }

    [Fact]
    public async Task Run_ShouldFail_WhenResourceIsMissing()
    {
        // Arrange
        var options = new CreateOptions
        {
            Resource = "",
            Name = "name"
        };

        // Act
        var result = await _sut.Run(options);

        // Assert
        result.ShouldBe(1);
    }

    [Fact]
    public async Task Run_ShouldFail_WhenNameIsMissing()
    {
        // Arrange
        var options = new CreateOptions
        {
            Resource = "project",
            Name = ""
        };

        // Act
        var result = await _sut.Run(options);

        // Assert
        result.ShouldBe(1);
    }

    #endregion
}