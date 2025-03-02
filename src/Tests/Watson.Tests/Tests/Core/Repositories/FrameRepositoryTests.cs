using Dapper;
using Shouldly;
using Watson.Core;
using Watson.Core.Abstractions;
using Watson.Core.Helpers;
using Watson.Core.Models.Database;
using Watson.Core.Repositories;

namespace Watson.Tests.Tests.Core.Repositories;

public class FrameRepositoryTests : IDisposable
{
    #region Members

    private readonly IAppDbContext _dbContext;
    private readonly FrameRepository _sut;
    private readonly string _dbFilePath = Path.GetTempFileName();

    #endregion

    #region Constructors

    public FrameRepositoryTests()
    {
        var idHelper = new IdHelper();
        _dbContext = new AppDbContext($"Data Source={_dbFilePath};Cache=Shared;Pooling=False");
        _sut = new FrameRepository(_dbContext, new IdHelper());

        _ = new ProjectRepository(_dbContext, idHelper);
        _ = new TagRepository(_dbContext, idHelper);
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
    public async Task GetByIdAsync_ShouldFrameWithProjectAndTags_WhenIdExists()
    {
        // Arrange
        const string id = "id";
        await _dbContext.Connection.ExecuteAsync(
            "INSERT INTO Frames (Id,ProjectId,Time) VALUES (@Id,@ProjectId,@Time)", new
            {
                Id = id,
                ProjectId = "id",
                Time = 1
            });
        await _dbContext.Connection.ExecuteAsync("INSERT INTO Projects (Id,Name) VALUES (@Id,@Name)", new
        {
            Id = "id",
            Name = "name"
        });
        await _dbContext.Connection.ExecuteAsync("INSERT INTO Tags (Id,Name) VALUES (@Id,@Name)", new
        {
            Id = "id",
            Name = "name"
        });
        await _dbContext.Connection.ExecuteAsync(
            "INSERT INTO Frames_Tags (Id,FrameId,TagId) VALUES (@Id,@FrameId,@TagId)", new
            {
                Id = "id",
                FrameId = id,
                TagId = "id"
            });

        // Act
        var result = await _sut.GetByIdAsync(id);

        // Assert
        result.ShouldNotBeNull();
        result.Project.ShouldNotBeNull();
        result.Project.Name.ShouldBe("name");
        result.Tags.ShouldNotBeNull();
        result.Tags.Count.ShouldBe(1);
        result.Tags.First().Name.ShouldBe("name");
    }

    [Fact]
    public async Task GetByIdAsync_ShouldReturnNull_WhenIdDoesNotExist()
    {
        // Arrange
        const string id = "id";

        // Act
        var result = await _sut.GetByIdAsync(id);

        // Assert
        result.ShouldBeNull();
    }

    [Fact]
    public async Task GetAsync_ShouldReturnAllFramesWithProjectAndTags_WhenIdExists()
    {
        // Arrange
        const string id = "id";
        await _dbContext.Connection.ExecuteAsync(
            "INSERT INTO Frames (Id,ProjectId,Time) VALUES (@Id,@ProjectId,@Time)", new
            {
                Id = id,
                ProjectId = "id",
                Time = 1
            });
        await _dbContext.Connection.ExecuteAsync("INSERT INTO Projects (Id,Name) VALUES (@Id,@Name)", new
        {
            Id = "id",
            Name = "name"
        });
        await _dbContext.Connection.ExecuteAsync("INSERT INTO Tags (Id,Name) VALUES (@Id,@Name)", new
        {
            Id = "id",
            Name = "name"
        });
        await _dbContext.Connection.ExecuteAsync(
            "INSERT INTO Frames_Tags (Id,FrameId,TagId) VALUES (@Id,@FrameId,@TagId)", new
            {
                Id = "id",
                FrameId = id,
                TagId = "id"
            });

        // Act
        var result = await _sut.GetAsync();

        // Assert
        var resultLst = result.ToList();
        resultLst.ShouldNotBeNull();
        resultLst.Count.ShouldBe(1);
        resultLst.First().Project.ShouldNotBeNull();
        resultLst.First().Project!.Name.ShouldBe("name");
        resultLst.First().Tags.ShouldNotBeNull();
        resultLst.First().Tags.Count.ShouldBe(1);
        resultLst.First().Tags.First().Name.ShouldBe("name");
    }

    [Fact]
    public async Task GetNextFrameAsync_ShouldReturnNextFrame_WhenNextFrameExists()
    {
        // Arrange
        const string id = "id";
        await _dbContext.Connection.ExecuteAsync(
            "INSERT INTO Frames (Id,ProjectId,Time) VALUES (@Id,@ProjectId,@Time)", new
            {
                Id = id,
                ProjectId = "id",
                Time = 1
            });

        // Act
        var result = await _sut.GetNextFrameAsync(new DateTime(0));

        // Assert
        result.ShouldNotBeNull();
    }

    [Fact]
    public async Task GetNextFrameAsync_ShouldReturnNull_WhenNextFrameDoesNotExist()
    {
        // Arrange
        const string id = "id";
        await _dbContext.Connection.ExecuteAsync(
            "INSERT INTO Frames (Id,ProjectId,Time) VALUES (@Id,@ProjectId,@Time)", new
            {
                Id = id,
                ProjectId = "id",
                Time = 1
            });

        // Act
        var result = await _sut.GetNextFrameAsync(new DateTime(2));

        // Assert
        result.ShouldBeNull();
    }

    [Fact]
    public async Task GetPreviousFrameAsync_ShouldReturnPreviousFrame_WhenPreviousFrameExists()
    {
        // Arrange
        const string id = "id";
        await _dbContext.Connection.ExecuteAsync(
            "INSERT INTO Frames (Id,ProjectId,Time) VALUES (@Id,@ProjectId,@Time)", new
            {
                Id = id,
                ProjectId = "id",
                Time = 1
            });

        // Act
        var result = await _sut.GetPreviousFrameAsync(new DateTime(2));

        // Assert
        result.ShouldNotBeNull();
    }

    [Fact]
    public async Task GetPreviousFrameAsync_ShouldReturnNull_WhenPreviousFrameDoesNotExist()
    {
        // Arrange
        const string id = "id";
        await _dbContext.Connection.ExecuteAsync(
            "INSERT INTO Frames (Id,ProjectId,Time) VALUES (@Id,@ProjectId,@Time)", new
            {
                Id = id,
                ProjectId = "id",
                Time = 1
            });

        // Act
        var result = await _sut.GetPreviousFrameAsync(new DateTime(0));

        // Assert
        result.ShouldBeNull();
    }

    [Fact]
    public async Task GetAsync_ShoudReturnFramesWithinDateRange_WhenFramesExists()
    {
        // Arrange
        const string id = "id";
        await _dbContext.Connection.ExecuteAsync(
            "INSERT INTO Frames (Id,ProjectId,Time) VALUES (@Id,@ProjectId,@Time)", new
            {
                Id = id,
                ProjectId = "id",
                Time = 1
            });

        // Act
        var result = await _sut.GetAsync(new DateTime(0), new DateTime(2));

        // Assert
        var resultLst = result.ToList();
        resultLst.ShouldNotBeNull();
        resultLst.Count.ShouldBe(1);
    }

    [Fact]
    public async Task GetAsync_ShoudReturnEmptyList_WhenFramesDoesNotExist()
    {
        // Arrange
        const string id = "id";
        await _dbContext.Connection.ExecuteAsync(
            "INSERT INTO Frames (Id,ProjectId,Time) VALUES (@Id,@ProjectId,@Time)", new
            {
                Id = id,
                ProjectId = "id",
                Time = 1
            });

        // Act
        var result = await _sut.GetAsync(new DateTime(2), new DateTime(4));

        // Assert
        var resultLst = result.ToList();
        resultLst.ShouldNotBeNull();
        resultLst.Count.ShouldBe(0);
    }

    [Fact]
    public async Task AssociateTagsAsync_ShouldCreateManyRelationships_WhenTagsExists()
    {
        // Arrange
        const string id = "id";
        await _dbContext.Connection.ExecuteAsync(
            "INSERT INTO Frames (Id,ProjectId,Time) VALUES (@Id,@ProjectId,@Time)", new
            {
                Id = id,
                ProjectId = "id",
                Time = 1
            });
        await _dbContext.Connection.ExecuteAsync("INSERT INTO Tags (Id,Name) VALUES (@Id,@Name)", new
        {
            Id = "id",
            Name = "name"
        });

        // Act
        await _sut.AssociateTagsAsync(id, ["name"]);

        // Assert
        var result =
            await _dbContext.Connection.QueryAsync(
                "SELECT * FROM Frames_Tags WHERE FrameId = @Id", new { Id = id });
        var resultLst = result.ToList();
        resultLst.Count.ShouldBe(1);
    }

    [Fact]
    public async Task AssociateTagsAsync_ShouldDoNothing_WhenFrameDoesNotExist()
    {
        // Arrange

        // Act
        await _sut.AssociateTagsAsync("id", ["name"]);

        // Assert    
        var result =
            await _dbContext.Connection.QueryAsync(
                "SELECT * FROM Frames_Tags WHERE FrameId = @Id", new { Id = "id" });
        var resultLst = result.ToList();
        resultLst.Count.ShouldBe(0);
    }

    [Fact]
    public async Task InsertAsync_ShouldInsertFrame()
    {
        // Arrange
        var frame = new Frame
        {
            ProjectId = "id",
            Time = 1
        };

        // Act
        await _sut.InsertAsync(frame);

        // Assert
        var result = await _dbContext.Connection.QueryFirstOrDefaultAsync<Frame>(
            "SELECT * FROM Frames WHERE Id = @Id", new { frame.Id });
        result.ShouldNotBeNull();
    }

    [Fact]
    public async Task UpdateAsync_ShouldUpdateFrame()
    {
        // Arrange
        const string id = "id";
        await _dbContext.Connection.ExecuteAsync(
            "INSERT INTO Frames (Id,ProjectId,Time) VALUES (@Id,@ProjectId,@Time)", new
            {
                Id = id,
                ProjectId = "id",
                Time = 1
            });
        var frame = new Frame
        {
            Id = id,
            ProjectId = "id",
            Time = 2
        };

        // Act
        await _sut.UpdateAsync(frame);

        // Assert
        var result = await _dbContext.Connection.QueryFirstOrDefaultAsync<Frame>(
            "SELECT * FROM Frames WHERE Id = @Id", new { frame.Id });
        result.ShouldNotBeNull();
        result.Time.ShouldBe(2);
    }

    [Fact]
    public async Task GetAsync_ShouldReturnFramesWithinRange()
    {
        // Arrange
        await _dbContext.Connection.ExecuteAsync(
            "INSERT INTO Frames (Id,ProjectId,Time) VALUES (@Id,@ProjectId,@Time)", new
            {
                Id = "id",
                ProjectId = "id",
                Time = 1
            });
        await _dbContext.Connection.ExecuteAsync(
            "INSERT INTO Frames (Id,ProjectId,Time) VALUES (@Id,@ProjectId,@Time)", new
            {
                Id = "id2",
                ProjectId = "id",
                Time = 2
            });
        await _dbContext.Connection.ExecuteAsync(
            "INSERT INTO Frames (Id,ProjectId,Time) VALUES (@Id,@ProjectId,@Time)", new
            {
                Id = "id3",
                ProjectId = "id",
                Time = 3
            });

        // Act
        var result = await _sut.GetAsync(new DateTime(1), new DateTime(2), [], [], [], []);

        // Assert
        var resultLst = result.ToList();
        resultLst.ShouldNotBeNull();
        resultLst.Count.ShouldBe(2);
    }

    [Fact]
    public async Task GetAsync_ShouldReturnFramesWithinRangeAndProject()
    {
        // Arrange
        await _dbContext.Connection.ExecuteAsync(
            "INSERT INTO Frames (Id,ProjectId,Time) VALUES (@Id,@ProjectId,@Time)", new
            {
                Id = "id",
                ProjectId = "id",
                Time = 1
            });
        await _dbContext.Connection.ExecuteAsync(
            "INSERT INTO Frames (Id,ProjectId,Time) VALUES (@Id,@ProjectId,@Time)", new
            {
                Id = "id2",
                ProjectId = "id",
                Time = 2
            });
        await _dbContext.Connection.ExecuteAsync(
            "INSERT INTO Frames (Id,ProjectId,Time) VALUES (@Id,@ProjectId,@Time)", new
            {
                Id = "id3",
                ProjectId = "id2",
                Time = 3
            });

        // Act
        var result = await _sut.GetAsync(new DateTime(1), new DateTime(2), ["id"], [], [], []);

        // Assert
        var resultLst = result.ToList();
        resultLst.ShouldNotBeNull();
        resultLst.Count.ShouldBe(2);
    }

    [Fact]
    public async Task GetAsync_ShouldReturnFramesWithinRangeAndTags()
    {
        // Arrange
        await _dbContext.Connection.ExecuteAsync(
            "INSERT INTO Frames (Id,ProjectId,Time) VALUES (@Id,@ProjectId,@Time)", new
            {
                Id = "id",
                ProjectId = "id",
                Time = 1
            });
        await _dbContext.Connection.ExecuteAsync(
            "INSERT INTO Frames (Id,ProjectId,Time) VALUES (@Id,@ProjectId,@Time)", new
            {
                Id = "id2",
                ProjectId = "id",
                Time = 2
            });
        await _dbContext.Connection.ExecuteAsync(
            "INSERT INTO Frames (Id,ProjectId,Time) VALUES (@Id,@ProjectId,@Time)", new
            {
                Id = "id3",
                ProjectId = "id2",
                Time = 3
            });
        await _dbContext.Connection.ExecuteAsync(
            "INSERT INTO Frames_Tags (Id,FrameId,TagId) VALUES (@Id,@FrameId,@TagId)", new
            {
                Id = "id",
                FrameId = "id",
                TagId = "id"
            });
        await _dbContext.Connection.ExecuteAsync(
            "INSERT INTO Tags (Id,Name) VALUES (@Id,@Name)", new
            {
                Id = "id",
                Name = "name"
            });

        // Act
        var result = await _sut.GetAsync(new DateTime(1), new DateTime(2), [], ["id"], [], []);

        // Assert
        var resultLst = result.ToList();
        resultLst.ShouldNotBeNull();
        resultLst.Count.ShouldBe(1);
    }

    [Fact]
    public async Task GetAsync_ShouldReturnFramesWithinRangeAndTagsAndProject()
    {
        // Arrange
        await _dbContext.Connection.ExecuteAsync(
            "INSERT INTO Frames (Id,ProjectId,Time) VALUES (@Id,@ProjectId,@Time)", new
            {
                Id = "id",
                ProjectId = "id",
                Time = 1
            });
        await _dbContext.Connection.ExecuteAsync(
            "INSERT INTO Frames (Id,ProjectId,Time) VALUES (@Id,@ProjectId,@Time)", new
            {
                Id = "id2",
                ProjectId = "id1",
                Time = 2
            });
        await _dbContext.Connection.ExecuteAsync(
            "INSERT INTO Frames (Id,ProjectId,Time) VALUES (@Id,@ProjectId,@Time)", new
            {
                Id = "id3",
                ProjectId = "id2",
                Time = 3
            });
        await _dbContext.Connection.ExecuteAsync(
            "INSERT INTO Frames_Tags (Id,FrameId,TagId) VALUES (@Id,@FrameId,@TagId)", new
            {
                Id = "id",
                FrameId = "id",
                TagId = "id"
            });
        await _dbContext.Connection.ExecuteAsync(
            "INSERT INTO Frames_Tags (Id,FrameId,TagId) VALUES (@Id,@FrameId,@TagId)", new
            {
                Id = "id2",
                FrameId = "id2",
                TagId = "id"
            });
        await _dbContext.Connection.ExecuteAsync(
            "INSERT INTO Tags (Id,Name) VALUES (@Id,@Name)", new
            {
                Id = "id",
                Name = "name"
            });

        // Act
        var result = await _sut.GetAsync(new DateTime(1), new DateTime(2), ["id"], ["id"], [], []);

        // Assert
        var resultLst = result.ToList();
        resultLst.ShouldNotBeNull();
        resultLst.Count.ShouldBe(1);
    }

    [Fact]
    public async Task GetAsync_ShouldReturnFramesWithinRangeAndTagsAndProjectAndIgnoreProjectsAndIgnoreTags()
    {
        // Arrange
        await _dbContext.Connection.ExecuteAsync(
            "INSERT INTO Frames (Id,ProjectId,Time) VALUES (@Id,@ProjectId,@Time)", new
            {
                Id = "id",
                ProjectId = "id",
                Time = 1
            });
        await _dbContext.Connection.ExecuteAsync(
            "INSERT INTO Frames (Id,ProjectId,Time) VALUES (@Id,@ProjectId,@Time)", new
            {
                Id = "id2",
                ProjectId = "id1",
                Time = 2
            });
        await _dbContext.Connection.ExecuteAsync(
            "INSERT INTO Frames (Id,ProjectId,Time) VALUES (@Id,@ProjectId,@Time)", new
            {
                Id = "id3",
                ProjectId = "id2",
                Time = 3
            });
        await _dbContext.Connection.ExecuteAsync(
            "INSERT INTO Frames_Tags (Id,FrameId,TagId) VALUES (@Id,@FrameId,@TagId)", new
            {
                Id = "id",
                FrameId = "id",
                TagId = "id"
            });
        await _dbContext.Connection.ExecuteAsync(
            "INSERT INTO Frames_Tags (Id,FrameId,TagId) VALUES (@Id,@FrameId,@TagId)", new
            {
                Id = "id2",
                FrameId = "id2",
                TagId = "id"
            });
        await _dbContext.Connection.ExecuteAsync(
            "INSERT INTO Tags (Id,Name) VALUES (@Id,@Name)", new
            {
                Id = "id",
                Name = "name"
            });

        // Act
        var result = await _sut.GetAsync(new DateTime(1), new DateTime(2), ["id"], ["id"], ["id"], ["id"]);

        // Assert
        var resultLst = result.ToList();
        resultLst.ShouldNotBeNull();
        resultLst.Count.ShouldBe(0);
    }

    #endregion
}