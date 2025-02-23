using Watson.Core.Models;

namespace Watson.Core.Repositories.Abstractions;

public interface ITagRepository : IRepository<Tag>
{
    Task<bool> DoesNameExistAsync(string name);
    Task<Tag?> GetByNameAsync(string name);
}