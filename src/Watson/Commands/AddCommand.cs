using Watson.Commands.Abstractions;
using Watson.Models.CommandLine;

namespace Watson.Commands;

public class AddCommand : ICommand<AddOptions>
{
    #region Public methods

    public async Task<int> Run(AddOptions options)
    {
        return 0;
    }

    #endregion
}