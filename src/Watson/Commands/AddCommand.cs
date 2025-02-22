using Watson.Commands.Abstractions;
using Watson.Models;
using Watson.Models.CommandLine;

namespace Watson.Commands;

public class AddCommand : Command<AddOptions>
{
    #region Constructors

    public AddCommand(DependencyResolver dependencyResolver) : base(dependencyResolver)
    {
    }

    #endregion

    #region Public methods

    public override async Task<int> Run(AddOptions options)
    {
        return 0;
    }

    #endregion
}