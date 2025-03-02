using Microsoft.Data.Sqlite;

namespace Watson.Core.Abstractions;

public interface IAppDbContext
{
    SqliteConnection Connection { get; }
}