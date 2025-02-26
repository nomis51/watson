using Dapper;
using Shouldly;
using Watson.Commands;
using Watson.Core;
using Watson.Core.Helpers;
using Watson.Core.Models;
using Watson.Core.Repositories;
using Watson.Helpers;
using Watson.Models;
using Watson.Models.CommandLine;

namespace Watson.Tests.Commands;

public class CreateCommandTests : IDisposable
{
    #region Members

    private readonly AppDbContext _dbContext;
    private readonly string _dbFilePath = Path.GetTempFileName();
    private readonly CreateCommand _sut;

    #endregion

    #region Constructors

    public CreateCommandTests()
    {
        var idHelper = new IdHelper();
        _dbContext = new AppDbContext($"Data Source={_dbFilePath};Cache=Shared;Pooling=False");

        var frameRepository = new FrameRepository(_dbContext, idHelper);
        _sut = new CreateCommand(
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
            await _dbContext.Connection.QueryFirstOrDefaultAsync<Project>(
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
        await _dbContext.Connection.ExecuteAsync("INSERT INTO Projects (Id,Name) VALUES ('id','project')");


        // Act
        var result = await _sut.Run(options);

        // Assert
        result.ShouldBe(1);
        var count = _dbContext.Connection.QueryFirst<int>("SELECT COUNT(*) FROM Projects WHERE Name = 'project'");
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
            await _dbContext.Connection.QueryFirstOrDefaultAsync<Tag>(
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
        await _dbContext.Connection.ExecuteAsync("INSERT INTO Tags (Id,Name) VALUES ('id','tag')");


        // Act
        var result = await _sut.Run(options);

        // Assert
        result.ShouldBe(1);
        var count = _dbContext.Connection.QueryFirst<int>("SELECT COUNT(*) FROM Tags WHERE Name = 'tag'");
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