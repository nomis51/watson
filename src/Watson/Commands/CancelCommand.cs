using Watson.Commands.Abstractions;
using Watson.Models.CommandLine;

namespace Watson.Commands;

public class CancelCommand : ICommand<CancelOptions>
{
    #region Public methods

    public async Task<int> Run(CancelOptions options)
    {
        return 0;
    }

    #endregion
}