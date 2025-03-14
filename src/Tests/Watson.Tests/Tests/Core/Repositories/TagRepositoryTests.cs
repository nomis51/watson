using Dapper;
using Shouldly;
using Watson.Core;
using Watson.Core.Abstractions;
using Watson.Core.Helpers;
using Watson.Core.Models.Database;
using Watson.Core.Repositories;

namespace Watson.Tests.Tests.Core.Repositories;

public class TagRepositoryTests : IDisposable
{
    #region Members

    private readonly IAppDbContext _dbContext;
    private readonly TagRepository _sut;
    private readonly string _dbFilePath = Path.GetTempFileName();

    #endregion

    #region Constructors

    public TagRepositoryTests()
    {
        _dbContext = new AppDbContext($"Data Source={_dbFilePath};Cache=Shared;Pooling=False");
        _sut = new TagRepository(_dbContext, new IdHelper());
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

    [Test]
    public async Task DoesNameExistsAsync_ShouldReturnTrue()
    {
        // Arrange
        const string name = "name";
        await _dbContext.Connection.ExecuteAsync("INSERT INTO Tags (Id,Name) VALUES (@Id,@Name)", new
        {
            Id = "id",
            Name = name
        });

        // Act
        var result = await _sut.DoesNameExistAsync(name);

        // Assert
        result.ShouldBeTrue();
    }

    [Test]
    public async Task DoesNameExistsAsync_ShouldReturnFalse_WhenNameDoesNotExist()
    {
        // Arrange
        const string name = "name";

        // Act
        var result = await _sut.DoesNameExistAsync(name);

        // Assert
        result.ShouldBeFalse();
    }

    [Test]
    public async Task GetByNameAsync_ShouldReturnProject()
    {
        // Arrange
        const string name = "name";
        await _dbContext.Connection.ExecuteAsync("INSERT INTO Tags (Id,Name) VALUES (@Id,@Name)", new
        {
            Id = "id",
            Name = name
        });

        // Act
        var result = await _sut.GetByNameAsync(name);

        // Assert
        result.ShouldNotBeNull();
    }

    [Test]
    public async Task GetByNameAsync_ShouldReturnNull_WhenNameDoesNotExist()
    {
        // Arrange
        const string name = "name";

        // Act
        var result = await _sut.GetByNameAsync(name);

        // Assert
        result.ShouldBeNull();
    }

    [Test]
    public async Task RenameAsync_ShouldRename()
    {
        // Arrange
        const string name = "name";
        const string newName = "newName";
        await _dbContext.Connection.ExecuteAsync("INSERT INTO Tags (Id,Name) VALUES (@Id,@Name)", new
        {
            Id = "id",
            Name = name
        });

        // Act
        var result = await _sut.RenameAsync("id", newName);

        // Assert
        result.ShouldBeTrue();
        (await _dbContext.Connection.QueryFirstAsync<int>("SELECT COUNT(*) FROM Tags"))
            .ShouldBe(1);
        (await _dbContext.Connection.QueryFirstAsync<string>("SELECT Name FROM Tags"))
            .ShouldBe(newName);
    }

    [Test]
    public async Task RenameAsync_ShouldReturnFalse_WhenNameDoesNotExist()
    {
        // Arrange
        const string name = "name";

        // Act
        var result = await _sut.RenameAsync("id", name);

        // Assert
        result.ShouldBeFalse();
    }

    [Test]
    public async Task EnsureTagsExists_ShouldInsertTags_WhenTagsDoNotExist()
    {
        // Arrange
        await _dbContext.Connection.ExecuteAsync("INSERT INTO Tags (Id,Name) VALUES (@Id,@Name)", new
        {
            Id = "id",
            Name = "a"
        });

        // Act
        var result = await _sut.EnsureTagsExistsAsync(["a", "b"]);

        // Assert
        result.ShouldBeTrue();
        (await _dbContext.Connection.QueryAsync<Tag>("SELECT Name FROM Tags"))
            .Select(e => e.Name)
            .ToArray()
            .ShouldBeEquivalentTo(new[] { "a", "b" });
    }

    #endregion
}