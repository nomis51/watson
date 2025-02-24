using Microsoft.Data.Sqlite;
using Watson.Core.Abstractions;

namespace Watson.Core;

public class AppDbContext : IAppDbContext
{
    #region Props

    public SqliteConnection Connection { get; }

    #endregion

    #region Constructors

    public AppDbContext(string? connectionString = null)
    {
        var filePath = Path.Join(
            Environment.GetFolderPath(Environment.SpecialFolder.UserProfile),
            $".{nameof(Watson).ToLower()}",
            "data.db"
        );
        Connection = new SqliteConnection(
            string.IsNullOrEmpty(connectionString) ? $"Data Source={filePath};" : connectionString
        );
    }

    #endregion
}