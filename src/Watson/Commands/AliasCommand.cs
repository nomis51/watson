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

            var alias = await DependencyResolver.AliasRepository.GetByNameAsync(name);
            if (alias is not null)
            {
                Console.MarkupLine("[red]Alias already exists.[/]");
                return 1;
            }

            var command = string.Join(' ', arguments.Skip(2));

            alias = new Alias
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
        throw new NotImplementedException();
    }

    #endregion
}