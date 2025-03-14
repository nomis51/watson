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

public class WorkHoursCommandTests : CommandWithConsoleTest
{
    #region Members

    private readonly ISettingsRepository _settingsRepository = Substitute.For<ISettingsRepository>();
    private readonly WorkHoursCommand _sut;

    #endregion

    #region Constructors

    public WorkHoursCommandTests()
    {
        var idHelper = new IdHelper();
        _settingsRepository.GetSettings()
            .Returns(new Settings());

        var frameRepository = new FrameRepository(DbContext, idHelper);
        _sut = new WorkHoursCommand(
            new DependencyResolver(
                new ProjectRepository(DbContext, idHelper),
                frameRepository,
                new TagRepository(DbContext, idHelper),
                new TimeHelper(),
                new FrameHelper(frameRepository),
                _settingsRepository,
                new TodoRepository(DbContext, idHelper),
                ConsoleAdapter
            )
        );
    }

    #endregion

    #region Tests

    [Test]
    public async Task Run_ShouldAddCustomWorkHours_WhenStart()
    {
        // Arrange
        var startTime = new TimeSpan(8, 56, 0);

        // Act
        var result = await _sut.Run(new WorkHoursOptions
        {
            Type = "start",
            Time = new TimeHelper().FormatTime(startTime)
        });

        // Assert
        result.ShouldBe(0);
        await _settingsRepository.Received()
            .SaveSettings(Arg.Is<Settings>(e =>
                    e.CustomWorkTimes[0].WorkTime.StartTime == startTime
                )
            );
    }

    [Test]
    public async Task Run_ShouldAddCustomWorkHours_WhenEnd()
    {
        // Arrange
        var startTime = new TimeSpan(8, 56, 0);

        // Act
        var result = await _sut.Run(new WorkHoursOptions
        {
            Type = "end",
            Time = new TimeHelper().FormatTime(startTime)
        });

        // Assert
        result.ShouldBe(0);
        await _settingsRepository.Received()
            .SaveSettings(Arg.Is<Settings>(e =>
                    e.CustomWorkTimes[0].WorkTime.EndTime == startTime
                )
            );
    }

    [Test]
    public async Task Run_ShouldResetCustomWorkHours()
    {
        // Arrange
        _settingsRepository.GetSettings()
            .Returns(new Settings
            {
                CustomWorkTimes =
                [
                    new SettingsCustomWorkTime()
                    {
                        Date = DateTime.Today,
                        WorkTime = new SettingsWorkTime
                        {
                            StartTime = new TimeSpan(8, 0, 0),
                            EndTime = new TimeSpan(17, 0, 0)
                        }
                    }
                ]
            });

        // Act
        var result = await _sut.Run(new WorkHoursOptions
        {
            Type = "reset",
        });

        // Assert
        result.ShouldBe(0);
        await _settingsRepository.Received()
            .SaveSettings(Arg.Is<Settings>(e =>
                    e.CustomWorkTimes.Count == 0
                )
            );
    }

    #endregion
}