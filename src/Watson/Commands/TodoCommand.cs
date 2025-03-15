using Spectre.Console;
using Watson.Commands.Abstractions;
using Watson.Core.Models.Database;
using Watson.Models.Abstractions;
using Watson.Models.CommandLine;

namespace Watson.Commands;

public class TodoCommand : Command<TodoOptions>
{
    #region Constants

    private const string AddAction = "add";
    private const string CreateAction = "create";
    private const string RemoveAction = "remove";
    private const string DeleteAction = "delete";
    private const string CompleteAction = "complete";
    private const string DoneAction = "done";
    private const string ListAction = "list";
    private const string UncompleteAction = "uncomplete";
    private const string UndoneAction = "undone";
    private const string UndoAction = "undo";
    private const string ResetAction = "reset";
    private const string EditAction = "edit";

    #endregion

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
            AddAction or CreateAction => await AddTodo(options),
            RemoveAction or DeleteAction => await RemoveTodo(options),
            ListAction => await ListTodos(options),
            CompleteAction or DoneAction => await ToggleCompletionTodo(options, true),
            UncompleteAction or UndoneAction or UndoAction or ResetAction => await ToggleCompletionTodo(options, false),
            EditAction => await EditTodo(options),
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

            if (ListAction.StartsWith(inputs[0], StringComparison.OrdinalIgnoreCase))
            {
                Console.WriteLine(ListAction);
                return;
            }

            if (CompleteAction.StartsWith(inputs[0], StringComparison.OrdinalIgnoreCase))
            {
                Console.WriteLine(CompleteAction);
                return;
            }

            if (DoneAction.StartsWith(inputs[0], StringComparison.OrdinalIgnoreCase))
            {
                Console.WriteLine(DoneAction);
                return;
            }

            if (UncompleteAction.StartsWith(inputs[0], StringComparison.OrdinalIgnoreCase))
            {
                Console.WriteLine(UncompleteAction);
                return;
            }

            if (UndoAction.StartsWith(inputs[0], StringComparison.OrdinalIgnoreCase))
            {
                Console.WriteLine(UndoAction);
                return;
            }

            if (UndoneAction.StartsWith(inputs[0], StringComparison.OrdinalIgnoreCase))
            {
                Console.WriteLine(UndoneAction);
                return;
            }

            if (ResetAction.StartsWith(inputs[0], StringComparison.OrdinalIgnoreCase))
            {
                Console.WriteLine(ResetAction);
                return;
            }

            if (EditAction.StartsWith(inputs[0], StringComparison.OrdinalIgnoreCase))
            {
                Console.WriteLine(EditAction);
                return;
            }
        }

        if (inputs.Length == 3)
        {
            if (inputs[0].Equals(EditAction, StringComparison.OrdinalIgnoreCase) ||
                inputs[0].Equals(AddAction, StringComparison.OrdinalIgnoreCase) ||
                inputs[0].Equals(CreateAction, StringComparison.OrdinalIgnoreCase))
            {
                var projects = await DependencyResolver.ProjectRepository.GetAsync();
                var project =
                    projects.FirstOrDefault(e => e.Name.StartsWith(inputs[2], StringComparison.OrdinalIgnoreCase));
                if (project is not null)
                {
                    Console.WriteLine(project.Name);
                    return;
                }
            }
        }

        if (inputs.Length == 4)
        {
            if (inputs[0].Equals(EditAction, StringComparison.OrdinalIgnoreCase) ||
                inputs[0].Equals(AddAction, StringComparison.OrdinalIgnoreCase) ||
                inputs[0].Equals(CreateAction, StringComparison.OrdinalIgnoreCase))
            {
                var tags = await DependencyResolver.TagRepository.GetAsync();
                var tag = tags.FirstOrDefault(e => e.Name.StartsWith(inputs[3], StringComparison.OrdinalIgnoreCase));
                if (tag is not null)
                {
                    Console.WriteLine(tag.Name);
                    return;
                }
            }
        }
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
        table.Alignment(Justify.Left);
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

        Console.Write(table);

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