using Watson.Commands.Abstractions;
using Watson.Models.Abstractions;
using Watson.Models.CommandLine;

namespace Watson.Commands;

public class WikiCommand : Command<WikiOptions>
{
    #region Constants

    private static readonly string WikiUrl = $"https://nomis51.github.io/{nameof(Watson).ToLower()}/";

    #endregion

    #region Constructors

    public WikiCommand(IDependencyResolver dependencyResolver) : base(dependencyResolver)
    {
    }

    #endregion

    #region Public methods

    public override Task<int> Run(WikiOptions options)
    {
        ProcessHelper.OpenInBrowser(WikiUrl);
        return Task.FromResult(0);
    }

    public override Task ProvideCompletions(string[] inputs)
    {
        return Task.CompletedTask;
    }

    #endregion
}