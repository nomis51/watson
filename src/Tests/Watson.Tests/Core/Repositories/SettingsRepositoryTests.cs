using System.IO.Abstractions;
using System.Text.Json;
using Microsoft.Extensions.Logging;
using NSubstitute;
using NSubstitute.ExceptionExtensions;
using Shouldly;
using Watson.Core.Models.Settings;
using Watson.Core.Repositories;

namespace Watson.Tests.Core.Repositories;

public class SettingsRepositoryTests
{
    #region Members

    private readonly ILogger<SettingsRepository> _logger = Substitute.For<ILogger<SettingsRepository>>();
    private readonly IFileSystem _fileSystem = Substitute.For<IFileSystem>();
    private readonly SettingsRepository _sut;

    #endregion

    #region Constructors

    public SettingsRepositoryTests()
    {
        _sut = new SettingsRepository(_fileSystem, _logger);
    }

    #endregion

    #region Tests

    [Fact]
    public async Task GetSettings_ShouldReturnDefaultSettings_WhenFileDoesNotExist()
    {
        // Arrange

        // Act
        var result = await _sut.GetSettings();

        // Assert
        result.ShouldNotBeNull();
    }

    [Fact]
    public async Task GetSettings_ShouldReturnSettings_WhenFileExists()
    {
        // Arrange
        _fileSystem.File.Exists(Arg.Any<string>())
            .Returns(true);
        _fileSystem.File.ReadAllTextAsync(Arg.Any<string>())
            .Returns(JsonSerializer.Serialize(new Settings
            {
                WorkTime =
                {
                    StartTime = new TimeSpan(9, 0, 0),
                }
            }));

        // Act
        var result = await _sut.GetSettings();

        // Assert
        result.ShouldNotBeNull();
        result.WorkTime.StartTime.ShouldBe(new TimeSpan(9, 0, 0));
    }

    [Fact]
    public async Task GetSettings_ShouldLogAndReturnDefault_WhenThrows()
    {
        // Arrange
        _fileSystem.File.Exists(Arg.Any<string>())
            .Returns(true);
        _fileSystem.File.ReadAllTextAsync(Arg.Any<string>())
            .Throws(new Exception("test"));

        // Act
        var result = await _sut.GetSettings();

        // Assert
        result.ShouldNotBeNull();
        _logger.Received()
            .Log(
                LogLevel.Error,
                Arg.Any<EventId>(),
                Arg.Any<object>(),
                Arg.Any<Exception>(),
                Arg.Any<Func<object, Exception?, string>>()
            );
    }

    [Fact]
    public async Task SaveSettings_ShouldSave()
    {
        // Arrange
        var settings = new Settings
        {
            WorkTime =
            {
                StartTime = new TimeSpan(9, 0, 0),
            }
        };

        // Act
        await _sut.SaveSettings(settings);

        // Assert
        await _fileSystem.Received()
            .File.WriteAllTextAsync(Arg.Any<string>(), Arg.Any<string>());
    }

    [Fact]
    public async Task SaveSettings_ShouldLogAndThrow_WhenThrows()
    {
        // Arrange
        var settings = new Settings
        {
            WorkTime =
            {
                StartTime = new TimeSpan(9, 0, 0),
            }
        };
        _fileSystem.File.WriteAllTextAsync(Arg.Any<string>(), Arg.Any<string>())
            .Throws(new Exception("test"));

        // Act
        await _sut.SaveSettings(settings);

        // Assert
        _logger.Received()
            .Log(
                LogLevel.Error,
                Arg.Any<EventId>(),
                Arg.Any<object>(),
                Arg.Any<Exception>(),
                Arg.Any<Func<object, Exception?, string>>()
            );
    }

    #endregion
}