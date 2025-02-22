using System.ComponentModel;
using System.Reflection;
using Dapper;
using Watson.Core.Abstractions;
using Watson.Core.Models;
using Watson.Core.Repositories.Abstractions;

namespace Watson.Core.Repositories;

public class FrameRepository : Repository<Frame>, IFrameRepository
{
    #region Constructors

    public FrameRepository(IAppDbContext dbContext) : base(dbContext)
    {
        InitializeTable();
    }

    #endregion

    #region Protected methods

    protected override void InitializeTable()
    {
        var sql = $"""
                   CREATE TABLE IF NOT EXISTS {typeof(Frame).GetCustomAttribute<DescriptionAttribute>()!.Description} (
                      Id TEXT NOT NULL PRIMARY KEY,
                   )
                   """;
        var result = DbContext.Connection.Execute(sql);
    }

    #endregion
}