using System.Diagnostics;
using System.Runtime.InteropServices;
using Watson.Core.Helpers.Abstractions;

namespace Watson.Core.Helpers;

public class ProcessHelper : IProcessHelper
{
    #region Public methods

    public void OpenInBrowser(string url)
    {
        string command;
        string arguments;

        if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
        {
            command = "cmd";
            arguments = $"/C start {url}";
        }
        else if (RuntimeInformation.IsOSPlatform(OSPlatform.Linux))
        {
            command = "open";
            arguments = url;
        }
        else if (RuntimeInformation.IsOSPlatform(OSPlatform.OSX))
        {
            command = "xdg-open";
            arguments = url;
        }
        else
        {
            throw new PlatformNotSupportedException();
        }

        Process.Start(new ProcessStartInfo
        {
            FileName = command,
            Arguments = arguments,
            UseShellExecute = true
        });
    }

    #endregion
}