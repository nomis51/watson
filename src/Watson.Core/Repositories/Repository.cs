using Watson.Core.Abstractions;
using Watson.Core.Models.Abstractions;
using Watson.Core.Repositories.Abstractions;

namespace Watson.Core.Repositories;

public abstract class Repository<TModel> : IRepository<TModel>
    where TModel : DbModel
{
    #region Members

    protected readonly IAppDbContext DbContext;

    #endregion
    
    #region Constructors

    protected Repository(IAppDbContext dbContext)
    {
        DbContext = dbContext;
    }

    #endregion
    
    #region Public methods

    public Task<TModel?> GetByIdAsync(string id)
    {
        throw new NotImplementedException();
    }

    #endregion

    #region Protected methods

    protected abstract void InitializeTable();

    #endregion
}