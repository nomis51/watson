using Watson.Commands.Abstractions;
using Watson.Models;

namespace Watson.Commands;

public abstract class Command<TOptions> : ICommand<TOptions>
{
    #region Members

    protected readonly DependencyResolver DependencyResolver;

    #endregion

    #region Constructors

    protected Command(DependencyResolver dependencyResolver)
    {
        DependencyResolver = dependencyResolver;
    }

    #endregion

    #region Public methods

    public abstract Task<int> Run(TOptions options);

    #endregion
}