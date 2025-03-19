using System.Reflection;
using Watson.Commands.Abstractions;
using Watson.Core.Models.Database;
using Watson.Models.Abstractions;
using Watson.Models.CommandLine;

namespace Watson.Commands;

public class AliasCommand : Command<AliasOptions>
{
    #region Constants

    private const string CreateAction = "create";
    private const string SetAction = "set";
    private const string AddAction = "add";
    private const string RemoveAction = "remove";
    private const string DeleteAction = "delete";

    #endregion

    #region Constructors

    public AliasCommand(IDependencyResolver dependencyResolver) : base(dependencyResolver)
    {
    }

    #endregion

    #region Public methods

    public override async Task<int> Run(AliasOptions options)
    {
        var arguments = options.Arguments.ToList();
        if (arguments.Count < 2) return 1;

        if (SetAction.StartsWith(arguments[0], StringComparison.OrdinalIgnoreCase) ||
            AddAction.StartsWith(arguments[0], StringComparison.OrdinalIgnoreCase) ||
            CreateAction.StartsWith(arguments[0], StringComparison.OrdinalIgnoreCase))
        {
            if (arguments.Count < 3) return 1;

            var name = arguments[1];

            if (!await IsAliasNameValid(name))
            {
                Console.MarkupLine("[red]Alias already exists.[/]");
                return 1;
            }

            var command = string.Join(' ', arguments.Skip(2));

            var alias = new Alias
            {
                Name = name,
                Command = command
            };
            alias = await DependencyResolver.AliasRepository.InsertAsync(alias);
            if (alias is null)
            {
                Console.MarkupLine("[red]Failed to create alias.[/]");
                return 1;
            }

            Console.MarkupLine("[green]Alias created.[/]");
            return 0;
        }

        if (RemoveAction.StartsWith(arguments[0], StringComparison.OrdinalIgnoreCase) ||
            DeleteAction.StartsWith(arguments[0], StringComparison.OrdinalIgnoreCase))
        {
            if (arguments.Count < 2) return 1;

            var alias = await DependencyResolver.AliasRepository.GetByNameAsync(arguments[1]);
            if (alias is null)
            {
                Console.MarkupLine("[red]Alias not found.[/]");
                return 1;
            }

            if (!await DependencyResolver.AliasRepository.DeleteAsync(alias.Id))
            {
                Console.MarkupLine("[red]Failed to delete alias.[/]");
                return 1;
            }

            Console.MarkupLine("[green]Alias deleted.[/]");
            return 0;
        }

        var newAlias = new Alias
        {
            Name = arguments[0],
            Command = string.Join(' ', arguments.Skip(1))
        };

        newAlias = await DependencyResolver.AliasRepository.InsertAsync(newAlias);
        if (newAlias is null)
        {
            Console.MarkupLine("[red]Failed to create alias.[/]");
            return 1;
        }

        Console.MarkupLine("[green]Alias created.[/]");
        return 0;
    }


    public override Task ProvideCompletions(string[] inputs)
    {
        if (inputs.Length == 1)
        {
            if (AddAction.StartsWith(inputs[0], StringComparison.OrdinalIgnoreCase))
            {
                Console.WriteLine(AddAction);
                return Task.CompletedTask;
            }

            if (CreateAction.StartsWith(inputs[0], StringComparison.OrdinalIgnoreCase))
            {
                Console.WriteLine(CreateAction);
                return Task.CompletedTask;
            }

            if (SetAction.StartsWith(inputs[0], StringComparison.OrdinalIgnoreCase))
            {
                Console.WriteLine(SetAction);
                return Task.CompletedTask;
            }

            if (DeleteAction.StartsWith(inputs[0], StringComparison.OrdinalIgnoreCase))
            {
                Console.WriteLine(DeleteAction);
                return Task.CompletedTask;
            }

            if (RemoveAction.StartsWith(inputs[0], StringComparison.OrdinalIgnoreCase))
            {
                Console.WriteLine(RemoveAction);
                return Task.CompletedTask;
            }
        }

        return Task.CompletedTask;
    }

    #endregion

    #region Private methods

    private async Task<bool> IsAliasNameValid(string name)
    {
        var alias = await AliasRepository.GetByNameAsync(name);
        if (alias is not null) return false;

        var types = Assembly.GetExecutingAssembly()
            .GetTypes()
            .Where(e =>
                e.Namespace is not null &&
                e.Namespace.StartsWith($"{nameof(Watson)}.Commands") &&
                e.Name.EndsWith("Command")
            );
        foreach (var type in types)
        {
            var commandName =
                type.GetProperty(nameof(AddCommand.CommandName),
                        BindingFlags.Public | BindingFlags.Static | BindingFlags.FlattenHierarchy)?
                    .GetValue(null)?
                    .ToString() ?? string.Empty;
            if (string.IsNullOrEmpty(commandName)) continue;

            if (commandName.Equals(name, StringComparison.OrdinalIgnoreCase)) return false;
        }

        return true;
    }

    #endregion
}