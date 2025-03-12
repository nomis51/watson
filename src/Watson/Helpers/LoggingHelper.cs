using Serilog;

namespace Watson.Helpers;

public static class LoggingHelper
{
    #region Public methods

    public static void Configure()
    {
        Log.Logger = new LoggerConfiguration()
            .WriteTo.Console()
            .WriteTo.File(
                Path.Join(
                    Environment.GetFolderPath(Environment.SpecialFolder.UserProfile),
                    $".{nameof(Watson).ToLower()}",
                    "logs",
                    ".txt"
                ),
                rollingInterval: RollingInterval.Day
            )
            .CreateLogger();
    }

    #endregion
}