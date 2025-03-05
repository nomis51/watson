﻿using System.Text.RegularExpressions;
using Dapper;
using NSubstitute;
using Shouldly;
using Watson.Commands;
using Watson.Core;
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

public class StatusCommandTests : ConsoleTest
{
    #region Members

    private readonly ISettingsRepository _settingsRepository = Substitute.For<ISettingsRepository>();
    private readonly StatusCommand _sut;

    #endregion

    #region Constructors

    public StatusCommandTests()
    {
        var idHelper = new IdHelper();
        _settingsRepository.GetSettings()
            .Returns(new Settings());

        var frameRepository = new FrameRepository(DbContext, idHelper);
        _sut = new StatusCommand(
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

    #endregion

    #region Tests

    [Fact]
    public async Task Run_ShouldDisplayCurrentFrameStatus()
    {
        // Arrange
        var time = DateTime.Now.Date;
        time = time.AddDays(-1);
        time = time.AddTicks(time.TimeOfDay.Add(new TimeSpan(15, 45, 0)).Ticks);
        await DbContext.Connection.ExecuteAsync("INSERT INTO Frames (Id,ProjectId,Time) VALUES ('id','id',@Time)",
            new { Time = time.Ticks });
        await DbContext.Connection.ExecuteAsync("INSERT INTO Projects (Id,Name) VALUES ('id','project')");
        await DbContext.Connection.ExecuteAsync("INSERT INTO Tags (Id,Name) VALUES ('id','tag')");
        await DbContext.Connection.ExecuteAsync("INSERT INTO Frames_Tags (Id,FrameId,TagId) VALUES ('id','id','id')");
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

    [Fact]
    public async Task Run_ShouldDisplayElapsedUntilNow_WhenFrameIsRunning()
    {
        // Arrange
        var time = DateTime.Now;
        await DbContext.Connection.ExecuteAsync("INSERT INTO Frames (Id,ProjectId,Time) VALUES ('id','id',@Time)",
            new { Time = time.Ticks });
        await DbContext.Connection.ExecuteAsync("INSERT INTO Projects (Id,Name) VALUES ('id','project')");
        await DbContext.Connection.ExecuteAsync("INSERT INTO Tags (Id,Name) VALUES ('id','tag')");
        await DbContext.Connection.ExecuteAsync("INSERT INTO Frames_Tags (Id,FrameId,TagId) VALUES ('id','id','id')");
        var options = new StatusOptions();

        var expectedOutput =
            ConsoleHelper.GetSpectreMarkupOutput(
                $"id: [green]project[/] ([purple]tag[/]) started at [blue]{time:HH:mm}[/] (00h 00m)");

        // Act
        var result = await _sut.Run(options);
        var output = ConsoleHelper.GetMockOutput();

        // Assert
        result.ShouldBe(0);
        output.ShouldStartWith(expectedOutput);
    }

    [Fact]
    public async Task Run_ShouldTakeLunchTimeIntoAccount()
    {
        // Arrange
        _settingsRepository.GetSettings()
            .Returns(new Settings
            {
                WorkTime =
                {
                    LunchStartTime = new TimeSpan(DateTime.Now.Hour, 0, 0),
                    LunchEndTime = new TimeSpan(DateTime.Now.Hour + 1, 0, 0),
                }
            });
        var time = DateTime.Now.AddHours(-1);
        await DbContext.Connection.ExecuteAsync("INSERT INTO Frames (Id,ProjectId,Time) VALUES ('id','id',@Time)",
            new { Time = time.Ticks });
        await DbContext.Connection.ExecuteAsync("INSERT INTO Projects (Id,Name) VALUES ('id','project')");
        await DbContext.Connection.ExecuteAsync("INSERT INTO Tags (Id,Name) VALUES ('id','tag')");
        await DbContext.Connection.ExecuteAsync("INSERT INTO Frames_Tags (Id,FrameId,TagId) VALUES ('id','id','id')");
        var options = new StatusOptions();

        var expectedMinutes = Convert.ToInt32(60 - time.TimeOfDay.Minutes) - 1;
        var expectedOutput =
            ConsoleHelper.GetSpectreMarkupOutput(
                $"id: [green]project[/] ([purple]tag[/]) started at [blue]{time:HH:mm}[/] (00h {expectedMinutes}m)");

        // Act
        var result = await _sut.Run(options);
        var output = ConsoleHelper.GetMockOutput();

        // Assert
        result.ShouldBe(0);
        output.ShouldStartWith(expectedOutput);
    }

    #endregion
}