using CommandLine;
using Watson.Abstractions;
using Watson.Commands;
using Watson.Models.Abstractions;
using Watson.Models.CommandLine;

namespace Watson;

public class Cli : ICli
{
    #region Members

    private readonly IDependencyResolver _dependencyResolver;

    #endregion

    #region Constructors

    public Cli(IDependencyResolver dependencyResolver)
    {
        _dependencyResolver = dependencyResolver;
    }

    #endregion

    #region Public methods

    public Task<int> Run(string[] args)
    {
        return Parser.Default.ParseArguments<
                AddOptions,
                CancelOptions,
                ConfigOptions,
                CreateOptions,
                EditOptions,
                ListOptions,
                LogOptions,
                RemoveOptions,
                RenameOptions,
                RestartOptions,
                StartOptions,
                StatusOptions,
                StopOptions
            >(args)
            .MapResult<
                AddOptions,
                CancelOptions,
                ConfigOptions,
                CreateOptions,
                EditOptions,
                ListOptions,
                LogOptions,
                RemoveOptions,
                RenameOptions,
                RestartOptions,
                StartOptions,
                StatusOptions,
                StopOptions,
                Task<int>
            >(
                async options => await new AddCommand(_dependencyResolver).Run(options),
                async options => await new CancelCommand(_dependencyResolver).Run(options),
                async options => await new ConfigCommand(_dependencyResolver).Run(options),
                async options => await new CreateCommand(_dependencyResolver).Run(options),
                async options => await new EditCommand(_dependencyResolver).Run(options),
                async options => await new ListCommand(_dependencyResolver).Run(options),
                async options => await new LogCommand(_dependencyResolver).Run(options),
                async options => await new RemoveCommand(_dependencyResolver).Run(options),
                async options => await new RenameCommand(_dependencyResolver).Run(options),
                async options => await new RestartCommand(_dependencyResolver).Run(options),
                async options => await new StartCommand(_dependencyResolver).Run(options),
                async options => await new StatusCommand(_dependencyResolver).Run(options),
                async options => await new StopCommand(_dependencyResolver).Run(options),
                errors => Task.FromResult(1));
    }

    #endregion
}