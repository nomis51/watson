using System.Text.RegularExpressions;
using Dapper;
using NSubstitute;
using Shouldly;
using Watson.Commands;
using Watson.Core;
using Watson.Core.Helpers;
using Watson.Core.Repositories;
using Watson.Core.Repositories.Abstractions;
using Watson.Helpers;
using Watson.Models;
using Watson.Models.CommandLine;
using Watson.Tests.Abstractions;
using Watson.Tests.Helpers;

namespace Watson.Tests.Tests.Commands;

public class StatusCommandTests : ConsoleTest, IDisposable
{
    #region Members

    private readonly AppDbContext _dbContext;
    private readonly string _dbFilePath = Path.GetTempFileName();
    private readonly ISettingsRepository _settingsRepository = Substitute.For<ISettingsRepository>();
    private readonly StatusCommand _sut;

    #endregion

    #region Constructors

    public StatusCommandTests()
    {
        var idHelper = new IdHelper();
        _dbContext = new AppDbContext($"Data Source={_dbFilePath};Cache=Shared;Pooling=False");

        var frameRepository = new FrameRepository(_dbContext, idHelper);
        _sut = new StatusCommand(
            new DependencyResolver(
                new ProjectRepository(_dbContext, idHelper),
                frameRepository,
                new TagRepository(_dbContext, idHelper),
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
    public async Task Run_ShouldDisplayCurrentFrameStatus()
    {
        // Arrange
        var time = DateTime.Now.Date;
        time = time.AddDays(-1);
        time = time.AddTicks(time.TimeOfDay.Add(new TimeSpan(15, 45, 0)).Ticks);
        await _dbContext.Connection.ExecuteAsync("INSERT INTO Frames (Id,ProjectId,Time) VALUES ('id','id',@Time)",
            new { Time = time.Ticks });
        await _dbContext.Connection.ExecuteAsync("INSERT INTO Projects (Id,Name) VALUES ('id','project')");
        await _dbContext.Connection.ExecuteAsync("INSERT INTO Tags (Id,Name) VALUES ('id','tag')");
        await _dbContext.Connection.ExecuteAsync("INSERT INTO Frames_Tags (Id,FrameId,TagId) VALUES ('id','id','id')");
        var options = new StatusOptions();

        var expectedOutput =
            ConsoleHelper.GetSpectreMarkupOutput(
                "id: [green]project[/] ([purple]tag[/]) started at [blue]15:45[/] (1m 15s)");

        // Act
        var result = await _sut.Run(options);
        var output = ConsoleHelper.GetMockOutput();

        // Assert
        result.ShouldBe(0);
        output.ShouldStartWith(expectedOutput[..expectedOutput.LastIndexOf('(')]);
        var durationStr = output[output.LastIndexOf('(')..];
#pragma warning disable SYSLIB1045
        Regex.IsMatch(durationStr, @"\([0-9]{1,2}h [0-9]{1,2}m\)").ShouldBeTrue();
#pragma warning restore SYSLIB1045
    }

    [Fact]
    public async Task Run_ShouldFail_WhenNotFrameAreCurrentlyRunning()
    {
        // Arrange
        var options = new StatusOptions();

        // Act
        var result = await _sut.Run(options);

        // Assert
        result.ShouldBe(1);
        ConsoleHelper.GetMockOutput().ShouldBeEmpty();
    }

    #endregion
}