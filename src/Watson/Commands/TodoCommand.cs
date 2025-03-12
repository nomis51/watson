using Spectre.Console;
using Watson.Core.Models.Database;
using Watson.Models.Abstractions;
using Watson.Models.CommandLine;

namespace Watson.Commands;

public class TodoCommand : Command<TodoOptions>
{
    #region Constructors

    public TodoCommand(IDependencyResolver dependencyResolver) : base(dependencyResolver)
    {
    }

    #endregion

    #region Public methods

    public override async Task<int> Run(TodoOptions options)
    {
        return options.Action switch
        {
            "add" => await AddTodo(options),
            "remove" => await RemoveTodo(options),
            "list" => await ListTodos(options),
            _ => 1
        };
    }

    #endregion

    #region Private methods

    private async Task<int> ListTodos(TodoOptions options)
    {
        var todos = await TodoRepository.GetAsync();

        var grid = new Grid();
        grid.AddColumn();
        grid.AddColumn();
        grid.AddColumn();
        grid.AddColumn();
        grid.AddColumn();
        grid.AddColumn();
        grid.AddColumn();

        grid.AddRow(
            "ID",
            "Description",
            "Project",
            "Tags",
            "Completed",
            "Priority",
            "Due Time"
        );

        foreach (var todo in todos)
        {
            grid.AddRow(
                todo.Id,
                todo.Description,
                $"[green]{todo.Project?.Name ?? "[gray]-[/]"}[/]",
                $"([purple]{string.Join("[/], [purple]", todo.Tags.Select(e => e.Name))}[/])",
                todo.IsCompleted ? Emoji.Known.CheckMarkButton : "[gray]-[/]",
                todo.Priority.ToString() ?? "[gray]-[/]",
                todo.DueTimeAsDateTime?.ToString("yyyy-MM-dd HH:mm") ?? "[gray]-[/]"
            );
        }

        AnsiConsole.Write(grid);

        return 0;
    }

    private async Task<int> RemoveTodo(TodoOptions options)
    {
        var arguments = options.Arguments.ToList();
        if (arguments.Count < 1) return 1;

        if (!await TodoRepository.DeleteAsync(arguments[0])) return 1;

        return 0;
    }

    private async Task<int> AddTodo(TodoOptions options)
    {
        var arguments = options.Arguments.ToList();
        if (arguments.Count < 2) return 1;

        var project = await ProjectRepository.EnsureNameExistsAsync(arguments[1]);
        if (project is null) return 1;

        var hasDueTime = TimeHelper.ParseDateTime(options.DueTime, out var dueTime) && dueTime is not null;

        var todo = new Todo
        {
            Description = arguments[0],
            ProjectId = project.Id,
            DueTime = hasDueTime ? dueTime!.Value.Ticks : null,
            Priority = options.Priority
        };

        todo = await TodoRepository.InsertAsync(todo);
        if (todo is null) return 1;

        var tags = options.Arguments.Skip(2)
            .ToList();

        if (tags.Count > 0)
        {
            if (!await TagRepository.EnsureTagsExistsAsync(tags)) return 1;

            await TodoRepository.AssociateTagsAsync(todo.Id, tags);
        }

        return 0;
    }

    #endregion
}