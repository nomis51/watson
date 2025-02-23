using Watson.Commands.Abstractions;
using Watson.Models;
using Watson.Models.Abstractions;
using Watson.Models.CommandLine;

namespace Watson.Commands;

public class CancelCommand : Command<CancelOptions>
{
    #region Constructors

    public CancelCommand(IDependencyResolver dependencyResolver) : base(dependencyResolver)
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