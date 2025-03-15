using System.Reflection;
using CommandLine;
using Watson.Core.Repositories.Abstractions;
using Watson.Helpers.Abstractions;
using Watson.Models.Abstractions;

namespace Watson.Commands.Abstractions;

public abstract class Command<TOptions> : ICommand<TOptions>
{
    #region Members

    protected readonly IDependencyResolver DependencyResolver;

    #endregion

    #region Props

    public static string CommandName => typeof(TOptions).GetCustomAttribute<VerbAttribute>()!.Name;
    protected ITimeHelper TimeHelper => DependencyResolver.TimeHelper;
    protected IFrameHelper FrameHelper => DependencyResolver.FrameHelper;
    protected IFrameRepository FrameRepository => DependencyResolver.FrameRepository;
    protected IProjectRepository ProjectRepository => DependencyResolver.ProjectRepository;
    protected ITagRepository TagRepository => DependencyResolver.TagRepository;
    protected ISettingsRepository SettingsRepository => DependencyResolver.SettingsRepository;
    protected ITodoRepository TodoRepository => DependencyResolver.TodoRepository;
    protected IConsoleAdapter Console => DependencyResolver.ConsoleAdapter;

    #endregion

    #region Constructors

    protected Command(IDependencyResolver dependencyResolver)
    {
        DependencyResolver = dependencyResolver;
    }

    #endregion

    #region Public methods

    public abstract Task<int> Run(TOptions options);
    public abstract Task ProvideCompletions(string[] inputs);

    #endregion
}