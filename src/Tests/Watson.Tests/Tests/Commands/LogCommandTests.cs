using System.Text.RegularExpressions;
using Dapper;
using NSubstitute;
using Shouldly;
using Watson.Commands;
using Watson.Core.Helpers;
using Watson.Core.Models.Settings;
using Watson.Core.Repositories;
using Watson.Core.Repositories.Abstractions;
using Watson.Helpers;
using Watson.Models;
using Watson.Models.CommandLine;
using Watson.Tests.Abstractions;
using Watson.Tests.Helpers;

namespace Watson.Tests.Tests.Commands;

public class LogCommandTests : ConsoleTest
{
    #region Members

    private readonly ISettingsRepository _settingsRepository = Substitute.For<ISettingsRepository>();
    private readonly LogCommand _sut;

    #endregion

    #region Constructors

    public LogCommandTests()
    {
        var idHelper = new IdHelper();

        _settingsRepository.GetSettings()
            .Returns(new Settings());

        var frameRepository = new FrameRepository(DbContext, idHelper);
        _sut = new LogCommand(
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
    public async Task Run_ShouldDisplayToday_WhenDay()
    {
        // Arrange
        var time = new DateTime(
            DateTime.Now.Year,
            DateTime.Now.Month,
            DateTime.Now.Day,
            8,
            45,
            0
        );
        var time2 = new DateTime(
            DateTime.Now.Year,
            DateTime.Now.Month,
            DateTime.Now.Day,
            9,
            15,
            0
        );

        await DbContext.Connection.ExecuteAsync("INSERT INTO Frames (Id,ProjectId,Time) VALUES ('id','id',@Time)",
            new
            {
                Time = time.Ticks
            });
        await DbContext.Connection.ExecuteAsync("INSERT INTO Frames (Id,ProjectId,Time) VALUES ('id2','id2',@Time)",
            new
            {
                Time = time2.Ticks
            });
        await DbContext.Connection.ExecuteAsync("INSERT INTO Projects (Id,Name) VALUES ('id','project')");
        await DbContext.Connection.ExecuteAsync("INSERT INTO Projects (Id,Name) VALUES ('id2','project2')");
        await DbContext.Connection.ExecuteAsync("INSERT INTO Tags (Id,Name) VALUES ('id','tag')");
        await DbContext.Connection.ExecuteAsync("INSERT INTO Tags (Id,Name) VALUES ('id2','tag2')");
        await DbContext.Connection.ExecuteAsync("INSERT INTO Frames_Tags (Id,FrameId,TagId) VALUES ('id','id','id')");
        await DbContext.Connection.ExecuteAsync("INSERT INTO Frames_Tags (Id,FrameId,TagId) VALUES ('id2','id','id2')");
        await DbContext.Connection.ExecuteAsync("INSERT INTO Frames_Tags (Id,FrameId,TagId) VALUES ('id3','id2','id')");
        var options = new LogOptions
        {
            Day = true
        };
        var expectedLines = new[]
        {
            $"{DateTime.Today:dddd dd MMMM yyyy} (6h 15m)",
            ConsoleHelper.GetSpectreMarkupOutput(
                "id 08:45 to 09:15 [blue]00h 30m[/] [green]project[/] ([purple]tag[/], [purple]tag2[/])"),
        };

        // Act
        var result = await _sut.Run(options);
        var output = ConsoleHelper.GetMockOutput();

        // Assert
        result.ShouldBe(0);
        var lines = output.Split(Environment.NewLine,
            StringSplitOptions.TrimEntries | StringSplitOptions.RemoveEmptyEntries);
        for (var i = 0; i < expectedLines.Length; ++i)
        {
            if (i >= lines.Length) Assert.Fail("Missing line: " + expectedLines[i]);
            var line = Regex.Replace(lines[i].Trim(), @"\s+", " ");

            if (i + 1 < lines.Length)
            {
                line.ShouldBe(expectedLines[i]);
            }
        }

        var lastLine = Regex.Replace(lines[^1].Trim(), @"\s+", " ");
        lastLine.StartsWith("id2 09:15 to ").ShouldBeTrue();
        lastLine.EndsWith(
                ConsoleHelper.GetSpectreMarkupOutput(" [green]project2[/] ([purple]tag[/])"))
            .ShouldBeTrue();
    }

    #endregion
}