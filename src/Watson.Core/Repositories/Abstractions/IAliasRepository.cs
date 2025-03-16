using Watson.Core.Models.Database;

namespace Watson.Core.Repositories.Abstractions;

public interface IAliasRepository : IRepository<Alias>
{
    Task<Alias?> GetByNameAsync(string name);
}