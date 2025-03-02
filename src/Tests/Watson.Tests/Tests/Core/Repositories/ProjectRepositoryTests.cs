using Dapper;
using Shouldly;
using Watson.Core;
using Watson.Core.Abstractions;
using Watson.Core.Helpers;
using Watson.Core.Repositories;

namespace Watson.Tests.Tests.Core.Repositories;

public class ProjectRepositoryTests : IDisposable
{
    #region Members

    private readonly IAppDbContext _dbContext;
    private readonly ProjectRepository _sut;
    private readonly string _dbFilePath = Path.GetTempFileName();

    #endregion

    #region Constructors

    public ProjectRepositoryTests()
    {
        _dbContext = new AppDbContext($"Data Source={_dbFilePath};Cache=Shared;Pooling=False");
        _sut = new ProjectRepository(_dbContext, new IdHelper());
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
    public async Task DoesNameExistsAsync_ShouldReturnTrue()
    {
        // Arrange
        const string name = "name";
        await _dbContext.Connection.ExecuteAsync("INSERT INTO Projects (Id,Name) VALUES (@Id,@Name)", new
        {
            Id = "id",
            Name = name
        });

        // Act
        var result = await _sut.DoesNameExistAsync(name);

        // Assert
        result.ShouldBeTrue();
    }

    [Fact]
    public async Task DoesNameExistsAsync_ShouldReturnFalse_WhenNameDoesNotExist()
    {
        // Arrange
        const string name = "name";

        // Act
        var result = await _sut.DoesNameExistAsync(name);

        // Assert
        result.ShouldBeFalse();
    }

    [Fact]
    public async Task GetByNameAsync_ShouldReturnProject()
    {
        // Arrange
        const string name = "name";
        await _dbContext.Connection.ExecuteAsync("INSERT INTO Projects (Id,Name) VALUES (@Id,@Name)", new
        {
            Id = "id",
            Name = name
        });

        // Act
        var result = await _sut.GetByNameAsync(name);

        // Assert
        result.ShouldNotBeNull();
    }

    [Fact]
    public async Task GetByNameAsync_ShouldReturnNull_WhenNameDoesNotExist()
    {
        // Arrange
        const string name = "name";

        // Act
        var result = await _sut.GetByNameAsync(name);

        // Assert
        result.ShouldBeNull();
    }

    [Fact]
    public async Task EnsureNameExistsAsync_ShouldReturnExistingProject_WhenNameExists()
    {
        // Arrange
        const string name = "name";
        await _dbContext.Connection.ExecuteAsync("INSERT INTO Projects (Id,Name) VALUES (@Id,@Name)", new
        {
            Id = "id",
            Name = name
        });

        // Act
        var result = await _sut.EnsureNameExistsAsync(name);

        // Assert
        result.ShouldNotBeNull();
        (await _dbContext.Connection.QueryFirstAsync<int>("SELECT COUNT(*) FROM Projects"))
            .ShouldBe(1);
    }

    [Fact]
    public async Task EnsureNameExistsAsync_ShouldReturnNewProject_WhenNameDoesNotExist()
    {
        // Arrange
        const string name = "name";

        // Act
        var result = await _sut.EnsureNameExistsAsync(name);

        // Assert
        result.ShouldNotBeNull();
        (await _dbContext.Connection.QueryFirstAsync<int>("SELECT COUNT(*) FROM Projects"))
            .ShouldBe(1);
    }

    [Fact]
    public async Task RenameAsync_ShouldRename()
    {
        // Arrange
        const string name = "name";
        const string newName = "newName";
        await _dbContext.Connection.ExecuteAsync("INSERT INTO Projects (Id,Name) VALUES (@Id,@Name)", new
        {
            Id = "id",
            Name = name
        });

        // Act
        var result = await _sut.RenameAsync("id", newName);

        // Assert
        result.ShouldBeTrue();
        (await _dbContext.Connection.QueryFirstAsync<int>("SELECT COUNT(*) FROM Projects"))
            .ShouldBe(1);
        (await _dbContext.Connection.QueryFirstAsync<string>("SELECT Name FROM Projects"))
            .ShouldBe(newName);
    }

    [Fact]
    public async Task RenameAsync_ShouldReturnFalse_WhenNameDoesNotExist()
    {
        // Arrange
        const string name = "name";

        // Act
        var result = await _sut.RenameAsync("id", name);

        // Assert
        result.ShouldBeFalse();
    }

    #endregion
}