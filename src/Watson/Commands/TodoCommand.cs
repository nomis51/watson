using System.ComponentModel;
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
            "complete" or "done" => await ToggleCompletionTodo(options, true),
            "uncomplete" or "undone" or "undo" or "reset" => await ToggleCompletionTodo(options, false),
            "edit" => await EditTodo(options),
            _ => 1
        };
    }

    #endregion

    #region Private methods

    private async Task<int> EditTodo(TodoOptions options)
    {
        var arguments = options.Arguments.ToList();
        if (arguments.Count < 1) return 1;

        var todo = await TodoRepository.GetByIdAsync(arguments[0]);
        if (todo is null) return 1;

        if (options.Priority is not null)
        {
            todo.Priority = options.Priority.Value;
        }

        if (options.DueTime is not null)
        {
            if (!TimeHelper.ParseDateTime(options.DueTime, out var dueTime)) return 1;

            todo.DueTime = dueTime!.Value.Ticks;
        }

        if (arguments.Count > 1)
        {
            todo.Description = arguments[1];
        }

        if (arguments.Count > 2)
        {
            var project = await ProjectRepository.EnsureNameExistsAsync(arguments[2]);
            if (project is null) return 1;

            todo.ProjectId = project.Id;
        }

        var ok = await TodoRepository.UpdateAsync(todo);

        if (arguments.Count > 3)
        {
            await TodoRepository.AssociateTagsAsync(todo.Id, arguments.Skip(3));
        }

        return ok ? 0 : 1;
    }

    private async Task<int> ToggleCompletionTodo(TodoOptions options, bool isCompleted)
    {
        var arguments = options.Arguments.ToList();
        if (arguments.Count < 1) return 1;

        var todo = await TodoRepository.GetByIdAsync(arguments[0]);
        if (todo is null) return 1;

        todo.IsCompleted = isCompleted;
        return await TodoRepository.UpdateAsync(todo) ? 0 : 1;
    }

    private async Task<int> ListTodos(TodoOptions options)
    {
        var todos = await TodoRepository.GetAsync();

        var table = new Table();
        table.Border(TableBorder.Rounded);
        table.AddColumn("ID");
        table.AddColumn("Description");
        table.AddColumn("Project");
        table.AddColumn("Tags");
        table.AddColumn("Completed")
            .Centered();
        table.AddColumn("Priority")
            .Centered();
        table.AddColumn("Due Time");

        foreach (var todo in todos)
        {
            table.AddRow(
                new Text(todo.Id),
                new Text(todo.Description),
                new Markup($"[green]{todo.Project?.Name ?? "[gray]-[/]"}[/]"),
                new Markup($"[purple]{string.Join("[/], [purple]", todo.Tags.Select(e => e.Name))}[/]"),
                new Markup(todo.IsCompleted ? Emoji.Known.CheckMarkButton : "[gray]-[/]")
                    .Centered(),
                new Markup(todo.Priority.ToString() ?? "[gray]-[/]")
                    .Centered(),
                new Markup(todo.DueTimeAsDateTime?.ToString("yyyy-MM-dd HH:mm") ?? "[gray]-[/]")
                    .Centered()
            );
        }

        AnsiConsole.Write(table);

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