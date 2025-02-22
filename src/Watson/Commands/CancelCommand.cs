using Watson.Commands.Abstractions;
using Watson.Models;
using Watson.Models.CommandLine;

namespace Watson.Commands;

public class CancelCommand : Command<CancelOptions>
{
    #region Constructors

    public CancelCommand(DependencyResolver dependencyResolver) : base(dependencyResolver)
    {
    }

    #endregion

    #region Public methods

    public override async Task<int> Run(CancelOptions options)
    {
        return 0;
    }

    #endregion
}