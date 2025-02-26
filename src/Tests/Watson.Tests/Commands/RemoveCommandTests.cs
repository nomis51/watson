using Dapper;
using Shouldly;
using Watson.Commands;
using Watson.Core;
using Watson.Core.Helpers;
using Watson.Core.Repositories;
using Watson.Helpers;
using Watson.Models;
using Watson.Models.CommandLine;

namespace Watson.Tests.Commands;

public class RemoveCommandTests : IDisposable
{
    #region Members

    private readonly AppDbContext _dbContext;
    private readonly string _dbFilePath = Path.GetTempFileName();
    private readonly RemoveCommand _sut;

    #endregion

    #region Constructors

    public RemoveCommandTests()
    {
        var idHelper = new IdHelper();
        _dbContext = new AppDbContext($"Data Source={_dbFilePath};Cache=Shared;Pooling=False");

        var frameRepository = new FrameRepository(_dbContext, idHelper);
        _sut = new RemoveCommand(
            new DependencyResolver(
                new ProjectRepository(_dbContext, idHelper),
                frameRepository,
                new TagRepository(_dbContext, idHelper),
                new TimeHelper(),
                new FrameHelper(frameRepository)
            )
        );
    }

    public void Dispose()
    {
        GC.SuppressFinalize(this);
        _dbContext.Connection.Close();
        _dbContext.Connection.Dispose();

        if (File.Exists(_dbFilePath))
        {
            File.Delete(_dbFilePath);
        }
    }

    #endregion

    #region Tests

    [Fact]
    public async Task Run_ShouldDeleteProject()
    {
        // Arrange
        var options = new RemoveOptions
        {
            Resource = "project",
            ResourceId = "id"
        };
        await _dbContext.Connection.ExecuteAsync("INSERT INTO Projects (Id,Name) VALUES ('id','project')");

        // Act
        var result = await _sut.Run(options);

        // Assert
        result.ShouldBe(0);
        var count = _dbContext.Connection.QueryFirst<int>("SELECT COUNT(*) FROM Projects WHERE Id = 'id'");
        count.ShouldBe(0);
    }

    [Fact]
    public async Task Run_ShouldDeleteTag()
    {
        // Arrange
        var options = new RemoveOptions
        {
            Resource = "tag",
            ResourceId = "id"
        };
        await _dbContext.Connection.ExecuteAsync("INSERT INTO Tags (Id,Name) VALUES ('id','tag')");

        // Act
        var result = await _sut.Run(options);

        // Assert
        result.ShouldBe(0);
        var count = _dbContext.Connection.QueryFirst<int>("SELECT COUNT(*) FROM Tags WHERE Id = 'id'");
        count.ShouldBe(0);
    }

    [Fact]
    public async Task Run_ShouldDeleteFrame()
    {
        // Arrange
        var options = new RemoveOptions
        {
            Resource = "frame",
            ResourceId = "id"
        };
        await _dbContext.Connection.ExecuteAsync("INSERT INTO Frames (Id,ProjectId,Timestamp) VALUES ('id','id',1)");

        // Act
        var result = await _sut.Run(options);

        // Assert
        result.ShouldBe(0);
        var count = _dbContext.Connection.QueryFirst<int>("SELECT COUNT(*) FROM Frames WHERE Id = 'id'");
        count.ShouldBe(0);
    }

    [Fact]
    public async Task Run_ShouldFail_WhenResourceIsInvalid()
    {
        // Arrange
        var options = new RemoveOptions
        {
            Resource = "invalid",
            ResourceId = "id"
        };

        // Act
        var result = await _sut.Run(options);

        // Assert
        result.ShouldBe(1);
    }

    [Fact]
    public async Task Run_ShouldFail_WhenResourceIdIsInvalid()
    {
        // Arrange
        var options = new RemoveOptions
        {
            Resource = "project",
            ResourceId = "invalid"
        };

        // Act
        var result = await _sut.Run(options);

        // Assert
        result.ShouldBe(1);
    }

    [Fact]
    public async Task Run_ShouldFail_WhenResourceNotProvided()
    {
        // Arrange
        var options = new RemoveOptions
        {
            Resource = "",
            ResourceId = "id"
        };

        // Act
        var result = await _sut.Run(options);

        // Assert
        result.ShouldBe(1);
    }

    [Fact]
    public async Task Run_ShouldFail_WhenResourceIdNotProvided()
    {
        // Arrange
        var options = new RemoveOptions
        {
            Resource = "project",
            ResourceId = ""
        };

        // Act
        var result = await _sut.Run(options);

        // Assert
        result.ShouldBe(1);
    }

    #endregion
}