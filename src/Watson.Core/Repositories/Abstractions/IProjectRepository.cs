using Watson.Core.Models.Database;

namespace Watson.Core.Repositories.Abstractions;

public interface IProjectRepository : IRepository<Project>
{
    Task<bool> DoesNameExistAsync(string name);
    Task<Project?> GetByNameAsync(string name);
    Task<Project?> EnsureNameExistsAsync(string name);
    Task<bool> RenameAsync(string id, string name);
}