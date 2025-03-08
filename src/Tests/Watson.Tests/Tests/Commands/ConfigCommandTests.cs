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

namespace Watson.Tests.Tests.Commands;

public class ConfigCommandTests : ConsoleTest
{
    #region Members

    private readonly ISettingsRepository _settingsRepository = Substitute.For<ISettingsRepository>();
    private readonly ConfigCommand _sut;

    #endregion

    #region Constructors

    public ConfigCommandTests()
    {
        var idHelper = new IdHelper();

        var frameRepository = new FrameRepository(DbContext, idHelper);
        _sut = new ConfigCommand(
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
    public async Task Run_Get_ShouldDisplaySettingValue_WhenExists()
    {
        // Arrange
        _settingsRepository.GetSettings()
            .Returns(new Settings
            {
                WorkTime = new SettingsWorkTime
                {
                    StartTime = new TimeSpan(1, 0, 0)
                }
            });
        var options = new ConfigOptions
        {
            Action = "get",
            Key = "workTime.startTime"
        };

        // Act
        var result = await _sut.Run(options);
        var output = ConsoleHelper.GetMockOutput();

        // Assert
        result.ShouldBe(0);
        output.ShouldStartWith("workTime.startTime: 01:00:00");
    }

    [Fact]
    public async Task Run_Get_ShouldFail_WhenSettingDoesNotExist()
    {
        // Arrange
        var options = new ConfigOptions
        {
            Action = "get",
            Key = "foo"
        };

        // Act
        var result = await _sut.Run(options);

        // Assert
        result.ShouldBe(1);
    }

    [Fact]
    public async Task Run_Set_ShouldSetSettingValue_WhenExists()
    {
        // Arrange
        _settingsRepository.GetSettings()
            .Returns(new Settings
            {
                WorkTime = new SettingsWorkTime
                {
                    StartTime = new TimeSpan(1, 0, 0)
                }
            });
        var options = new ConfigOptions
        {
            Action = "set",
            Key = "workTime.startTime",
            Value = "02:00:00"
        };

        // Act
        var result = await _sut.Run(options);

        // Assert
        result.ShouldBe(0);
        await _settingsRepository.Received()
            .SaveSettings(Arg.Is<Settings>(s =>
                    s.WorkTime.StartTime == new TimeSpan(2, 0, 0)
                )
            );
    }

    [Fact]
    public async Task Run_Set_ShouldFail_WhenSettingDoesNotExist()
    {
        // Arrange
        var options = new ConfigOptions
        {
            Action = "set",
            Key = "foo",
            Value = "02:00:00"
        };

        // Act
        var result = await _sut.Run(options);

        // Assert
        result.ShouldBe(1);
    }

    #endregion
}