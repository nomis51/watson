using NSubstitute;
using Watson.Commands;
using Watson.Core.Helpers;
using Watson.Core.Repositories;
using Watson.Core.Repositories.Abstractions;
using Watson.Helpers;
using Watson.Models;
using Watson.Tests.Abstractions;

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

        var frameRepository = new FrameRepository(DbContext, idHelper);
        _sut = new LogCommand(
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

    #endregion
}