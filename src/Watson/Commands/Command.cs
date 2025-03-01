using Watson.Commands.Abstractions;
using Watson.Core.Repositories.Abstractions;
using Watson.Helpers.Abstractions;
using Watson.Models.Abstractions;

namespace Watson.Commands;

public abstract class Command<TOptions> : ICommand<TOptions>
{
    #region Members

    protected readonly IDependencyResolver DependencyResolver;

    #endregion

    #region Props

    protected ITimeHelper TimeHelper => DependencyResolver.TimeHelper;
    protected IFrameHelper FrameHelper => DependencyResolver.FrameHelper;
    protected IFrameRepository FrameRepository => DependencyResolver.FrameRepository;
    protected IProjectRepository ProjectRepository => DependencyResolver.ProjectRepository;
    protected ITagRepository TagRepository => DependencyResolver.TagRepository;

    #endregion

    #region Constructors

    protected Command(IDependencyResolver dependencyResolver)
    {
        DependencyResolver = dependencyResolver;
    }

    #endregion

    #region Public methods

    public abstract Task<int> Run(TOptions options);

    #endregion
}