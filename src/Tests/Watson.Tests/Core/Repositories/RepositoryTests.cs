using System.Reflection;
using Dapper;
using NSubstitute;
using Shouldly;
using Watson.Core;
using Watson.Core.Abstractions;
using Watson.Core.Helpers;
using Watson.Tests.Core.Repositories.Mocks;

namespace Watson.Tests.Core.Repositories;

public class RepositoryTests : IDisposable
{
    #region Members

    private readonly IAppDbContext _dbContext;
    private readonly TestRepository _sut;

    private readonly string _dbFilePath = Path.GetTempFileName();

    #endregion

    #region Constructors

    public RepositoryTests()
    {
        _dbContext = new AppDbContext($"Data Source={_dbFilePath};Cache=Shared;Pooling=False");
        _sut = new TestRepository(_dbContext, new IdHelper());
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
    public void Ctor_ShouldCallInitializeTable()
    {
        // Arrange
        // Act
        var result = _dbContext.Connection
            .QueryFirstOrDefault("SELECT * FROM sqlite_master WHERE type = 'table' AND name = 'Tests'");

        // Assert
        result!.ShouldNotBeNull();
    }

    [Fact]
    public async Task GetByAsync_ShouldReturnItem()
    {
        // Arrange
        const string id = "id";
        await _dbContext.Connection.ExecuteAsync(
            "INSERT INTO Tests (Id,Name) VALUES (@Id,@Name)",
            new
            {
                Id = id,
                Name = "name"
            }
        );

        // Act
        var result = await _sut.GetByIdAsync(id);

        // Assert
        result.ShouldNotBeNull();
    }

    [Fact]
    public async Task GetByAsync_ShouldReturnNull()
    {
        // Arrange
        const string id = "id";

        // Act
        var result = await _sut.GetByIdAsync(id);

        // Assert
        result.ShouldBeNull();
    }

    [Fact]
    public async Task GetAsync_ShouldReturnAllItems()
    {
        // Arrange
        await _dbContext.Connection.ExecuteAsync(
            """
                                        INSERT INTO Tests (Id,Name) VALUES
                                            ('id1','name1'),
                                            ('id2','name2');
            """);

        // Act
        var result = await _sut.GetAsync();

        // Assert
        result.Count().ShouldBe(2);
    }

    [Fact]
    public async Task InsertAsync_ShouldAssignId()
    {
        // Arrange
        var model = new TestModel { Name = "name" };

        // Act
        var result = await _sut.InsertAsync(model);

        // Assert
        result.ShouldBeTrue();
        model.Id.ShouldNotBeNullOrEmpty();
    }

    [Fact]
    public async Task InsertAsync_ShouldUseAsIs_WhenIdAlreadySet()
    {
        // Arrange
        var model = new TestModel { Id = "id", Name = "name" };

        // Act
        var result = await _sut.InsertAsync(model);

        // Assert
        result.ShouldBeTrue();
        model.Id.ShouldBe("id");
    }

    [Fact]
    public async Task UpdateAsync_ShouldReturnTrue()
    {
        // Arrange
        const string id = "id";
        await _dbContext.Connection.ExecuteAsync(
            "INSERT INTO Tests (Id,Name) VALUES (@Id,@Name)",
            new
            {
                Id = id,
                Name = "name"
            }
        );

        // Act
        var result = await _sut.UpdateAsync(new TestModel { Id = id, Name = "name2" });

        // Assert
        result.ShouldBeTrue();
        (await _dbContext.Connection.QueryFirstAsync<TestModel>("SELECT * FROM Tests WHERE Id = @Id", new { Id = id }))
            .Name
            .ShouldBe("name2");
    }

    [Fact]
    public async Task UpdateAsync_ShouldReturnFalse_WhenIdNotSet()
    {
        // Arrange
        var model = new TestModel { Name = "name" };

        // Act
        var result = await _sut.UpdateAsync(model);

        // Assert
        result.ShouldBeFalse();
    }

    [Fact]
    public async Task DeleteAsync_ShouldDeleteItem()
    {
        // Arrange
        const string id = "id";
        await _dbContext.Connection.ExecuteAsync(
            "INSERT INTO Tests (Id,Name) VALUES (@Id,@Name)",
            new
            {
                Id = id,
                Name = "name"
            }
        );

        // Act
        var result = await _sut.DeleteAsync(id);

        // Assert
        result.ShouldBeTrue();
        (await _dbContext.Connection.QueryFirstOrDefaultAsync<TestModel>("SELECT * FROM Tests WHERE Id = @Id",
                new { Id = id }))
            .ShouldBeNull();
    }

    [Fact]
    public async Task DeleteManyAsync_ShouldDeleteItems()
    {
        // Arrange
        await _dbContext.Connection.ExecuteAsync(
            "INSERT INTO Tests (Id,Name) VALUES ('id1','name1'),('id2','name2')");

        // Act
        var result = await _sut.DeleteManyAsync(["id1", "id2"]);

        // Assert
        result.ShouldBeTrue();
        (await _dbContext.Connection.QueryFirstOrDefaultAsync<TestModel>("SELECT * FROM Tests WHERE Id = 'id1'"))
            .ShouldBeNull();
        (await _dbContext.Connection.QueryFirstOrDefaultAsync<TestModel>("SELECT * FROM Tests WHERE Id = 'id2'"))
            .ShouldBeNull();
    }

    [Fact]
    public void CreateIndexIfNotExists_ShouldCreateIndex()
    {
        // Arrange
        var sut = _sut.GetType()
            .GetMethod("CreateIndexIfNotExists", BindingFlags.NonPublic | BindingFlags.Instance);
        sut.ShouldNotBeNull();

        // Act
        sut.Invoke(_sut, ["TestIndex", "CREATE INDEX TestIndex ON Tests (Id)"]);

        // Assert
        var result = _dbContext.Connection.QueryFirstOrDefault<string>(
            "SELECT name FROM sqlite_master WHERE type='index' AND name='TestIndex'"
        );

        result.ShouldNotBeNull();
    }

    [Fact]
    public void CreateIndexIfNotExists_ShouldNotCreateIndex_WhenIndexAlreadyExists()
    {
        // Arrange
        var sut = _sut.GetType()
            .GetMethod("CreateIndexIfNotExists", BindingFlags.NonPublic | BindingFlags.Instance);
        sut.ShouldNotBeNull();

        _dbContext.Connection.Execute("CREATE INDEX TestIndex ON Tests (Id)");

        // Act
        sut.Invoke(_sut, ["TestIndex", "CREATE INDEX TestIndex ON Tests (Id)"]);

        // Assert
        var result = _dbContext.Connection.QueryFirstOrDefault<string>(
            "SELECT name FROM sqlite_master WHERE type='index' AND name='TestIndex'"
        );

        result.ShouldNotBeNull();
    }

    #endregion
}