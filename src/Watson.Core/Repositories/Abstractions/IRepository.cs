using Watson.Core.Models.Abstractions;

namespace Watson.Core.Repositories.Abstractions;

public interface IRepository<TModel>
    where TModel : DbModel
{
    Task<TModel?> GetByIdAsync(string id);
    Task<IEnumerable<TModel>> GetAsync();
    Task<bool> InsertAsync(TModel model);
    Task<bool> UpdateAsync(TModel model);
    Task<bool> DeleteAsync(string id);
    Task<bool> DeleteManyAsync(IEnumerable<string> ids);
}