using Watson.Commands.Abstractions;
using Watson.Models.Abstractions;
using Watson.Models.CommandLine;

namespace Watson.Commands;

public class GithubCommand : Command<GithubOptions>
{
    #region Constants

    private static readonly string RepositoryUrl =
        $"https://github.com/nomis51/{nameof(Watson).ToLower()}";

    #endregion

    #region Constructors

    public GithubCommand(IDependencyResolver dependencyResolver) : base(dependencyResolver)
    {
    }

    #endregion

    #region Public methods

    public override Task<int> Run(GithubOptions options)
    {
        ProcessHelper.OpenInBrowser(RepositoryUrl);
        return Task.FromResult(0);
    }

    public override Task ProvideCompletions(string[] inputs)
    {
        return Task.CompletedTask;
    }

    #endregion
}