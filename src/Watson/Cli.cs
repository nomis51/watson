using CommandLine;
using Watson.Abstractions;
using Watson.Commands;
using Watson.Core.Repositories.Abstractions;
using Watson.Models;
using Watson.Models.CommandLine;

namespace Watson;

public class Cli : ICli
{
    #region Members

    private readonly DependencyResolver _dependencyResolver;

    #endregion

    #region Constructors

    public Cli(DependencyResolver dependencyResolver)
    {
        _dependencyResolver = dependencyResolver;
    }

    #endregion

    #region Public methods

    public Task<int> Run(string[] args)
    {
        return Parser.Default.ParseArguments<AddOptions, CancelOptions, CreateOptions>(args)
            .MapResult<AddOptions, CancelOptions, CreateOptions, Task<int>>(
                async options => await new AddCommand(_dependencyResolver).Run(options),
                async options => await new CancelCommand(_dependencyResolver).Run(options),
                async options => await new CreateCommand(_dependencyResolver).Run(options),
                errors => Task.FromResult(1));
    }

    #endregion
}