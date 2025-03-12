using Watson.Core.Models.Database;

namespace Watson.Core.Repositories.Abstractions;

public interface ITodoRepository : IRepository<Todo>
{
    Task<IEnumerable<Todo>> GetAsync(
        DateTime fromTime,
        DateTime toTime,
        List<string> projectIds,
        List<string> tagIds,
        List<string> ignoredProjectIds,
        List<string> ignoredTagIds
    );

    Task AssociateTagsAsync(string todoId, IEnumerable<string> tags);
}