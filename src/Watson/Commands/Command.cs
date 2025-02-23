using Watson.Commands.Abstractions;
using Watson.Models.Abstractions;

namespace Watson.Commands;

public abstract class Command<TOptions> : ICommand<TOptions>
{
    #region Members

    protected readonly IDependencyResolver DependencyResolver;

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