using Watson.Core.Models.Database;

namespace Watson.Core.Repositories.Abstractions;

public interface ITagRepository : IRepository<Tag>
{
    Task<bool> DoesNameExistAsync(string name);
    Task<Tag?> GetByNameAsync(string name);
    Task<bool> EnsureTagsExistsAsync(IEnumerable<string> tags);
    Task<bool> RenameAsync(string id, string name);
}