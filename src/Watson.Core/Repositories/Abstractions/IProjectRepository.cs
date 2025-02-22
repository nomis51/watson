using Watson.Core.Models;

namespace Watson.Core.Repositories.Abstractions;

public interface IProjectRepository : IRepository<Project>
{
    Task<bool> DoesNameExistAsync(string name);
}