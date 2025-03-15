using Watson.Commands.Abstractions;
using Watson.Models.Abstractions;
using Watson.Models.CommandLine;

namespace Watson.Commands;

public class ProjectCommand : Command<ProjectOptions>
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

    public ProjectCommand(IDependencyResolver dependencyResolver) : base(dependencyResolver)
    {
    }

    #endregion

    #region Public methods

    public override async Task<int> Run(ProjectOptions options)
    {
        return options.Action switch
        {
            AddAction or CreateAction => await AddProject(options),
            RemoveAction or DeleteAction => await RemoveProject(options),
            RenameAction => await RenameProject(options),
            ListAction => await ListProjects(options),
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

    private async Task<int> AddProject(ProjectOptions options)
    {
        var arguments = options.Arguments.ToList();
        if (arguments.Count != 1) return 1;

        var project = await ProjectRepository.EnsureNameExistsAsync(arguments[0]);
        return project is null ? 1 : 0;
    }

    private async Task<int> RemoveProject(ProjectOptions options)
    {
        var arguments = options.Arguments.ToList();
        if (arguments.Count != 1) return 1;

        return !await ProjectRepository.DeleteAsync(arguments[0]) ? 1 : 0;
    }

    private async Task<int> RenameProject(ProjectOptions options)
    {
        var arguments = options.Arguments.ToList();
        if (arguments.Count != 2) return 1;

        var project = await ProjectRepository.EnsureNameExistsAsync(arguments[0]);
        if (project is null) return 1;

        project.Name = arguments[1];
        return !await ProjectRepository.UpdateAsync(project) ? 1 : 0;
    }

    private async Task<int> ListProjects(ProjectOptions _)
    {
        var projects = await ProjectRepository.GetAsync();
        foreach (var project in projects)
        {
            Console.WriteLine("{0}: {1}", project.Id, project.Name);
        }

        return 0;
    }

    #endregion
}