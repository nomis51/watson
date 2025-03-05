using Watson.Core.Models.Database.Abstractions;

namespace Watson.Core.Repositories.Abstractions;

public interface IRepository<TModel>
    where TModel : DbModel
{
    Task<TModel?> GetByIdAsync(string id);
    Task<IEnumerable<TModel>> GetAsync();
    Task<TModel?> InsertAsync(TModel model);
    Task<bool> UpdateAsync(TModel model);
    Task<bool> DeleteAsync(string id);
    Task<bool> DeleteManyAsync(IEnumerable<string> ids);
}