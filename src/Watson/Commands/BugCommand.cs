using System.Diagnostics;
using System.Runtime.InteropServices;
using Watson.Commands.Abstractions;
using Watson.Models.Abstractions;
using Watson.Models.CommandLine;

namespace Watson.Commands;

public class BugCommand : Command<BugOptions>
{
    #region Constants

    private static readonly string BugReportUrl =
        $"https://github.com/nomis51/{nameof(Watson).ToLower()}/issues/new?template=bug_report.md";

    #endregion

    #region Constructors

    public BugCommand(IDependencyResolver dependencyResolver) : base(dependencyResolver)
    {
    }

    #endregion

    #region Public methods

    public override Task<int> Run(BugOptions options)
    {
        ProcessHelper.OpenInBrowser(BugReportUrl);
        return Task.FromResult(0);
    }

    public override Task ProvideCompletions(string[] inputs)
    {
        return Task.CompletedTask;
    }

    #endregion
}