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
    ITodoRepository TodoRepository { get; }
    IConsoleAdapter ConsoleAdapter { get; }
    IAliasRepository AliasRepository { get; }
}