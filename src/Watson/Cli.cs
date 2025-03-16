using System.Text;
using CommandLine;
using Microsoft.Extensions.Logging;
using Watson.Abstractions;
using Watson.Commands;
using Watson.Models.Abstractions;
using Watson.Models.CommandLine;

namespace Watson;

public class Cli : ICli
{
    #region Constants

    public const string CompletionCommandName = "complete";
    public const string AliasCommandName = "alias";

    #endregion

    #region Members

    private readonly ILogger<Cli> _logger;
    private readonly IDependencyResolver _dependencyResolver;

    #endregion

    #region Constructors

    public Cli(IDependencyResolver dependencyResolver, ILogger<Cli> logger)
    {
        _dependencyResolver = dependencyResolver;
        _logger = logger;
    }

    #endregion

    #region Public methods

    public async Task<int> Run(string[] args)
    {
        var exitCode = await HandleCompletion(args);
        if (exitCode != -1) return exitCode;

        exitCode = await HandleAlias(args);
        if (exitCode != -1) return exitCode;

        var parser = new Parser();
        return await parser.ParseArguments<
                AddOptions,
                AliasOptions,
                CancelOptions,
                ConfigOptions,
                EditOptions,
                LogOptions,
                ProjectOptions,
                RemoveOptions,
                RestartOptions,
                StartOptions,
                StatsOptions,
                StatusOptions,
                StopOptions,
                TagOptions,
                TodoOptions,
                WorkHoursOptions
            >(args)
            .MapResult<
                AddOptions,
                AliasOptions,
                CancelOptions,
                ConfigOptions,
                EditOptions,
                LogOptions,
                ProjectOptions,
                RemoveOptions,
                RestartOptions,
                StartOptions,
                StatsOptions,
                StatusOptions,
                StopOptions,
                TagOptions,
                TodoOptions,
                WorkHoursOptions,
                Task<int>
            >(
                async options => await new AddCommand(_dependencyResolver).Run(options),
                async options => await new AliasCommand(_dependencyResolver).Run(options),
                async options => await new CancelCommand(_dependencyResolver).Run(options),
                async options => await new ConfigCommand(_dependencyResolver).Run(options),
                async options => await new EditCommand(_dependencyResolver).Run(options),
                async options => await new LogCommand(_dependencyResolver).Run(options),
                async options => await new ProjectCommand(_dependencyResolver).Run(options),
                async options => await new RemoveCommand(_dependencyResolver).Run(options),
                async options => await new RestartCommand(_dependencyResolver).Run(options),
                async options => await new StartCommand(_dependencyResolver).Run(options),
                async options => await new StatsCommand(_dependencyResolver).Run(options),
                async options => await new StatusCommand(_dependencyResolver).Run(options),
                async options => await new StopCommand(_dependencyResolver).Run(options),
                async options => await new TagCommand(_dependencyResolver).Run(options),
                async options => await new TodoCommand(_dependencyResolver).Run(options),
                async options => await new WorkHoursCommand(_dependencyResolver).Run(options),
                errors =>
                {
                    foreach (var error in errors)
                    {
                        _logger.LogError("Error while parsing input arguments: {Error}", error);
                    }

                    return Task.FromResult(1);
                });
    }

    #endregion

    #region Private methods

    private async Task<int> HandleAlias(string[] args)
    {
        if (args.Length < 1) return -1;

        var alias = await _dependencyResolver.AliasRepository.GetByNameAsync(args[0]);
        if (alias is null) return -1;

        _dependencyResolver.ConsoleAdapter.MarkupLine("Executing command alias [blue]{0}[/]...", alias.Command);

        return await Run(
            alias.Arguments
                .Concat(args.Skip(1))
                .ToArray()
        );
    }

    private async Task<int> HandleCompletion(string[] args)
    {
        if (args.Length <= 0 || args[0] != CompletionCommandName) return -1;

        _dependencyResolver.ConsoleAdapter.SetEncoding(Encoding.UTF8);

        var inputs = args.Skip(1)
            .FirstOrDefault()?
            .Split(' ', StringSplitOptions.TrimEntries | StringSplitOptions.RemoveEmptyEntries)
            .ToArray() ?? [];
        return await ProvideCompletion(inputs.Skip(1).ToArray());
    }

    private async Task<int> ProvideCompletion(string[] args)
    {
        if (args.Length == 0)
        {
            Console.WriteLine("start");
            return 0;
        }

        var commandName = args[0];
        var actualArgs = args.Skip(1).ToArray();

        if (commandName == AddCommand.CommandName)
        {
            await new AddCommand(_dependencyResolver)
                .ProvideCompletions(actualArgs);
        }
        else if (commandName == AliasCommand.CommandName)
        {
            await new AliasCommand(_dependencyResolver)
                .ProvideCompletions(actualArgs);
        }
        else if (commandName == CancelCommand.CommandName)
        {
            await new CancelCommand(_dependencyResolver)
                .ProvideCompletions(actualArgs);
        }
        else if (commandName == ConfigCommand.CommandName)
        {
            await new ConfigCommand(_dependencyResolver)
                .ProvideCompletions(actualArgs);
        }
        else if (commandName == EditCommand.CommandName)
        {
            await new EditCommand(_dependencyResolver)
                .ProvideCompletions(actualArgs);
        }
        else if (commandName == LogCommand.CommandName)
        {
            await new LogCommand(_dependencyResolver)
                .ProvideCompletions(actualArgs);
        }
        else if (commandName == ProjectCommand.CommandName)
        {
            await new ProjectCommand(_dependencyResolver)
                .ProvideCompletions(actualArgs);
        }
        else if (commandName == RemoveCommand.CommandName)
        {
            await new RemoveCommand(_dependencyResolver)
                .ProvideCompletions(actualArgs);
        }
        else if (commandName == RestartCommand.CommandName)
        {
            await new RestartCommand(_dependencyResolver)
                .ProvideCompletions(actualArgs);
        }
        else if (commandName == StatsCommand.CommandName)
        {
            await new StatsCommand(_dependencyResolver)
                .ProvideCompletions(actualArgs);
        }
        else if (commandName == StatusCommand.CommandName)
        {
            await new StatusCommand(_dependencyResolver)
                .ProvideCompletions(actualArgs);
        }
        else if (commandName == StartCommand.CommandName)
        {
            await new StartCommand(_dependencyResolver)
                .ProvideCompletions(actualArgs);
        }
        else if (commandName == StopCommand.CommandName)
        {
            await new StopCommand(_dependencyResolver)
                .ProvideCompletions(actualArgs);
        }
        else if (commandName == TagCommand.CommandName)
        {
            await new TagCommand(_dependencyResolver)
                .ProvideCompletions(actualArgs);
        }
        else if (commandName == TodoCommand.CommandName)
        {
            await new TodoCommand(_dependencyResolver)
                .ProvideCompletions(actualArgs);
        }
        else if (commandName == WorkHoursCommand.CommandName)
        {
            await new WorkHoursCommand(_dependencyResolver)
                .ProvideCompletions(actualArgs);
        }

        return 0;
    }

    #endregion
}