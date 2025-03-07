﻿using Dapper;
using NSubstitute;
using Shouldly;
using Watson.Commands;
using Watson.Core;
using Watson.Core.Helpers;
using Watson.Core.Models.Database;
using Watson.Core.Repositories;
using Watson.Core.Repositories.Abstractions;
using Watson.Helpers;
using Watson.Models;
using Watson.Models.CommandLine;
using Watson.Tests.Abstractions;

namespace Watson.Tests.Tests.Commands;

public class EditCommandTests : CommandTest
{
    #region Members

    private readonly ISettingsRepository _settingsRepository = Substitute.For<ISettingsRepository>();
    private readonly EditCommand _sut;

    #endregion

    #region Constructors

    public EditCommandTests()
    {
        var idHelper = new IdHelper();

        var frameRepository = new FrameRepository(DbContext, idHelper);
        _sut = new EditCommand(
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
    public async Task Run_ShouldEditProject()
    {
        // Arrange
        var options = new EditOptions
        {
            FrameId = "id",
            Project = "newName"
        };
        await DbContext.Connection.ExecuteAsync("INSERT INTO Frames (Id,ProjectId,Time) VALUES ('id','id',1)");

        // Act
        var result = await _sut.Run(options);

        // Assert
        result.ShouldBe(0);
        var project =
            await DbContext.Connection.QueryFirstOrDefaultAsync<Project>(
                "SELECT * FROM Projects WHERE Name = 'newName'");
        project.ShouldNotBeNull();
        var frame = await DbContext.Connection.QueryFirstAsync<Frame>("SELECT * FROM Frames");
        frame.ProjectId.ShouldBe(project.Id);
    }

    [Fact]
    public async Task Run_ShouldFail_WhenFrameDoesNotExist()
    {
        // Arrange
        var options = new EditOptions
        {
            FrameId = "id",
            Project = "newName"
        };

        // Act
        var result = await _sut.Run(options);

        // Assert
        result.ShouldBe(1);
    }

    [Fact]
    public async Task Run_ShouldFail_WhenNowFrameDoesNotExist()
    {
        // Arrange
        var options = new EditOptions
        {
            Project = "newName"
        };

        // Act
        var result = await _sut.Run(options);

        // Assert
        result.ShouldBe(1);
    }

    [Fact]
    public async Task Run_ShouldUpdateTime_WhenFromTimeIsSpecified()
    {
        // Arrange
        var fromTime = DateTime.Now.AddMinutes(-5);
        var options = new EditOptions
        {
            FrameId = "id",
            Project = "newName",
            FromTime = fromTime.ToString("yyyy-MM-dd HH:mm")
        };
        await DbContext.Connection.ExecuteAsync("INSERT INTO Frames (Id,ProjectId,Time) VALUES ('id','id',1)");

        // Act
        var result = await _sut.Run(options);

        // Assert
        result.ShouldBe(0);
        var frame = await DbContext.Connection.QueryFirstAsync<Frame>("SELECT * FROM Frames");
        (frame.TimeAsDateTime - fromTime).TotalMinutes.ShouldBeLessThan(1);
    }

    #endregion
}