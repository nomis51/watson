using Watson.Core.Helpers.Abstractions;
using Watson.Core.Repositories.Abstractions;
using Watson.Helpers.Abstractions;
using Watson.Models.Abstractions;

namespace Watson.Models;

public class DependencyResolver : IDependencyResolver
{
    public IProjectRepository ProjectRepository { get; }
    public ITagRepository TagRepository { get; }
    public IFrameRepository FrameRepository { get; }
    public ITimeHelper TimeHelper { get; }
    public IFrameHelper FrameHelper { get; }
    public ISettingsRepository SettingsRepository { get; }
    public ITodoRepository TodoRepository { get; }
    public IConsoleAdapter ConsoleAdapter { get; }
    public IAliasRepository AliasRepository { get; }
    public IProcessHelper ProcessHelper { get; }

    public DependencyResolver(
        IProjectRepository projectRepository,
        IFrameRepository frameRepository,
        ITagRepository tagRepository,
        ITimeHelper timeHelper,
        IFrameHelper frameHelper,
        ISettingsRepository settingsRepository,
        ITodoRepository todoRepository,
        IConsoleAdapter consoleAdapter,
        IAliasRepository aliasRepository,
        IProcessHelper processHelper
    )
    {
        ProjectRepository = projectRepository;
        FrameRepository = frameRepository;
        TagRepository = tagRepository;
        TimeHelper = timeHelper;
        FrameHelper = frameHelper;
        SettingsRepository = settingsRepository;
        TodoRepository = todoRepository;
        ConsoleAdapter = consoleAdapter;
        AliasRepository = aliasRepository;
        ProcessHelper = processHelper;
    }
}