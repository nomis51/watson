using Watson.Commands.Abstractions;
using Watson.Models.Abstractions;
using Watson.Models.CommandLine;

namespace Watson.Commands;

public class TagCommand : Command<TagOptions>
{
    #region Constants

    private const string AddAction = "add";
    private const string CreateAction = "create";
    private const string RemoveAction = "remove";
    private const string DeleteAction = "delete";
    private const string RenameAction = "rename";
    private const string ListAction = "list";

    #endregion

    #region Constructors

    public TagCommand(IDependencyResolver dependencyResolver) : base(dependencyResolver)
    {
    }

    #endregion

    #region Public methods

    public override async Task<int> Run(TagOptions options)
    {
        return options.Action switch
        {
            AddAction or CreateAction => await AddTag(options),
            RemoveAction or DeleteAction => await RemoveTag(options),
            RenameAction => await RenameTag(options),
            ListAction => await ListTags(options),
            _ => 1
        };
    }

    public override async Task ProvideCompletions(string[] inputs)
    {
        if (inputs.Length == 1)
        {
            if (AddAction.StartsWith(inputs[0], StringComparison.OrdinalIgnoreCase))
            {
                Console.WriteLine(AddAction);
                return;
            }

            if (CreateAction.StartsWith(inputs[0], StringComparison.OrdinalIgnoreCase))
            {
                Console.WriteLine(CreateAction);
                return;
            }

            if (RemoveAction.StartsWith(inputs[0], StringComparison.OrdinalIgnoreCase))
            {
                Console.WriteLine(RemoveAction);
                return;
            }

            if (DeleteAction.StartsWith(inputs[0], StringComparison.OrdinalIgnoreCase))
            {
                Console.WriteLine(DeleteAction);
                return;
            }

            if (RenameAction.StartsWith(inputs[0], StringComparison.OrdinalIgnoreCase))
            {
                Console.WriteLine(RenameAction);
                return;
            }

            if (ListAction.StartsWith(inputs[0], StringComparison.OrdinalIgnoreCase))
            {
                Console.WriteLine(ListAction);
                return;
            }
        }

        if (inputs.Length == 2)
        {
            if (inputs[0].Equals(AddAction, StringComparison.OrdinalIgnoreCase) ||
                inputs[0].Equals(CreateAction, StringComparison.OrdinalIgnoreCase) ||
                inputs[0].Equals(RemoveAction, StringComparison.OrdinalIgnoreCase) ||
                inputs[0].Equals(DeleteAction, StringComparison.OrdinalIgnoreCase) ||
                inputs[0].Equals(RenameAction, StringComparison.OrdinalIgnoreCase))
            {
                var projects = await DependencyResolver.ProjectRepository.GetAsync();
                var project =
                    projects.FirstOrDefault(e => e.Name.StartsWith(inputs[1], StringComparison.OrdinalIgnoreCase));
                if (project is not null)
                {
                    Console.WriteLine(project.Name);
                }

                return;
            }
        }
    }

    #endregion

    #region Private methods

    private async Task<int> AddTag(TagOptions options)
    {
        var arguments = options.Arguments.ToList();
        if (arguments.Count != 1) return 1;

        return await TagRepository.EnsureTagsExistsAsync([arguments[0]]) ? 0 : 1;
    }

    private async Task<int> RemoveTag(TagOptions options)
    {
        var arguments = options.Arguments.ToList();
        if (arguments.Count != 1) return 1;

        return !await TagRepository.DeleteAsync(arguments[0]) ? 1 : 0;
    }

    private async Task<int> RenameTag(TagOptions options)
    {
        var arguments = options.Arguments.ToList();
        if (arguments.Count != 2) return 1;

        var tag = await TagRepository.GetByIdAsync(arguments[0]);
        if (tag is null) return 1;

        tag.Name = arguments[1];

        return !await TagRepository.UpdateAsync(tag) ? 1 : 0;
    }

    private async Task<int> ListTags(TagOptions _)
    {
        var tags = await TagRepository.GetAsync();
        foreach (var tag in tags)
        {
            Console.WriteLine("{0}: {1}", tag.Id, tag.Name);
        }

        return 0;
    }

    #endregion
}