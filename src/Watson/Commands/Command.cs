using Watson.Commands.Abstractions;
using Watson.Core.Repositories.Abstractions;
using Watson.Helpers.Abstractions;
using Watson.Models.Abstractions;

namespace Watson.Commands;

public abstract class Command<TOptions> : ICommand<TOptions>
{
    #region Members

    private readonly IDependencyResolver _dependencyResolver;

    #endregion

    #region Props

    protected ITimeHelper TimeHelper => _dependencyResolver.TimeHelper;
    protected IFrameHelper FrameHelper => _dependencyResolver.FrameHelper;
    protected IFrameRepository FrameRepository => _dependencyResolver.FrameRepository;
    protected IProjectRepository ProjectRepository => _dependencyResolver.ProjectRepository;
    protected ITagRepository TagRepository => _dependencyResolver.TagRepository;

    #endregion

    #region Constructors

    protected Command(IDependencyResolver dependencyResolver)
    {
        _dependencyResolver = dependencyResolver;
    }

    #endregion

    #region Public methods

    public abstract Task<int> Run(TOptions options);

    #endregion
}