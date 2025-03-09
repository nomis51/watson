using Watson.Core.Models.Database;
using Watson.Models.Abstractions;
using Watson.Models.CommandLine;

namespace Watson.Commands;

public class TodoCommands : Command<TodoOptions>
{
    #region Constructors

    public TodoCommands(IDependencyResolver dependencyResolver) : base(dependencyResolver)
    {
    }

    #endregion

    #region Public methods

    public override async Task<int> Run(TodoOptions options)
    {
        return options.Action switch
        {
            "add" => await AddTodo(options),
            _ => 1
        };
    }

    #endregion

    #region Private methods

    private async Task<int> AddTodo(TodoOptions options)
    {
        if (options.Arguments.Count < 2) return 1;

        var project = await ProjectRepository.EnsureNameExistsAsync(options.Arguments[1]);
        if (project is null) return 1;

        var hasDueTime = TimeHelper.ParseDateTime(options.DueTime, out var dueTime);

        var todo = new Todo
        {
            Description = options.Arguments[0],
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
            await TodoRepository.AssociateTagsAsync(todo.Id, tags);
        }

        return 1;
    }

    #endregion
}