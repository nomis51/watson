using CommandLine;
using Watson.Abstractions;
using Watson.Commands;
using Watson.Models.CommandLine;

namespace Watson;

public class Cli : ICli
{
    #region Public methods

    public Task<int> Run(string[] args)
    {
       return  Parser.Default.ParseArguments<AddOptions, CancelOptions>(args)
            .MapResult<AddOptions, CancelOptions, Task<int>>(
                async options => await new AddCommand().Run(options),
                async options => await new CancelCommand().Run(options),
                errors => Task.FromResult(1));
    }

    #endregion
}