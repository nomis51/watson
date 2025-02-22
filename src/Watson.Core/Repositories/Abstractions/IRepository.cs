﻿using Watson.Core.Models.Abstractions;

namespace Watson.Core.Repositories.Abstractions;

public interface IRepository<TModel>
    where TModel : DbModel
{
    Task<TModel?> GetByIdAsync(string id);
    Task<IEnumerable<TModel>> GetAsync();
    Task<bool> InsertAsync(TModel model);
}