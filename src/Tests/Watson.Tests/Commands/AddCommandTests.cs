using Dapper;
using Microsoft.AspNetCore.Components.Forms;
using NSubstitute;
using Shouldly;
using Watson.Commands;
using Watson.Core;
using Watson.Core.Helpers;
using Watson.Core.Models;
using Watson.Core.Repositories;
using Watson.Core.Repositories.Abstractions;
using Watson.Helpers;
using Watson.Models;
using Watson.Models.Abstractions;
using Watson.Models.CommandLine;

namespace Watson.Tests.Commands;

public class AddCommandTests : IDisposable
{
    #region Members

    private readonly AppDbContext _dbContext;
    private readonly string _dbFilePath = Path.GetTempFileName();
    private readonly AddCommand _sut;

    #endregion

    #region Constructors

    public AddCommandTests()
    {
        var idHelper = new IdHelper();
        _dbContext = new AppDbContext($"Data Source={_dbFilePath};Cache=Shared;Pooling=False");

        var frameRepository = new FrameRepository(_dbContext, idHelper);
        _sut = new AddCommand(
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
    public async Task Run_ShouldAddFrameAtNow_WithNoTag()
    {
        // Arrange
        AddOptions options = new()
        {
            Project = "project"
        };

        // Act
        await _sut.Run(options);

        // Assert
        var project = await _dbContext.Connection.QueryFirstAsync<Project>("SELECT * FROM Projects");
        var frame = await _dbContext.Connection.QueryFirstAsync<Frame>("SELECT * FROM Frames");
        var tag = await _dbContext.Connection.QueryFirstOrDefaultAsync<Tag>("SELECT * FROM Tags");

        tag.ShouldBeNull();
        project.Name.ShouldBe("project");
        frame.ProjectId.ShouldBe(project.Id);
        (DateTimeOffset.UtcNow - frame.TimestampAsDateTime).TotalSeconds.ShouldBeLessThan(3);
    }

    #endregion
}