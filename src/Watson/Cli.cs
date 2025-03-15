﻿using System.Text;
using CommandLine;
using Microsoft.Extensions.Logging;
using Watson.Abstractions;
using Watson.Commands;
using Watson.Models.Abstractions;
using Watson.Models.CommandLine;

namespace Watson;

public class Cli : ICli
{
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

    public Task<int> Run(string[] args)
    {
        if (args.Length > 0 && args[0] == "complete")
        {
            _dependencyResolver.ConsoleAdapter.SetEncoding(Encoding.UTF8);

            var inputs = args.Skip(1)
                .FirstOrDefault()?
                .Split(' ', StringSplitOptions.TrimEntries | StringSplitOptions.RemoveEmptyEntries)
                .ToArray() ?? [];
            return ProvideCompletion(inputs.Skip(1).ToArray());
        }

        var parser = new Parser();
        return parser.ParseArguments<
                AddOptions,
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

    private async Task<int> ProvideCompletion(string[] args)
    {
        if (args.Length == 0)
        {
            Console.WriteLine("start");
            return 0;
        }

        var commandName = args[0];
        var actualArgs = args.Skip(1).ToArray();

        if (commandName == StartCommand.CommandName)
        {
            await new StartCommand(_dependencyResolver)
                .ProvideCompletions(actualArgs);
        }
        else if ()
        {
        }

        return 0;
    }

    #endregion
}