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
            _ => 1
        };
    }

    #endregion

    #region Private methods

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