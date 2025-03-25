using Watson.Core.Helpers.Abstractions;
using Watson.Core.Repositories.Abstractions;
using Watson.Helpers.Abstractions;

namespace Watson.Models.Abstractions;

public interface IDependencyResolver
{
    IProjectRepository ProjectRepository { get; }
    ITagRepository TagRepository { get; }
    IFrameRepository FrameRepository { get; }
    ITimeHelper TimeHelper { get; }
    IFrameHelper FrameHelper { get; }
    ISettingsRepository SettingsRepository { get; }
    IConsoleAdapter ConsoleAdapter { get; }
    IAliasRepository AliasRepository { get; }
    IProcessHelper ProcessHelper { get; }
}