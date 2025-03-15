using Watson.Commands.Abstractions;
using Watson.Models.Abstractions;
using Watson.Models.CommandLine;

namespace Watson.Commands;

public class TagCommand : Command<TagOptions>
{
    #region Constructors

    public TagCommand(IDependencyResolver dependencyResolver) : base(dependencyResolver)
    {
    }

    #endregion

    #region Public methods

    public override async Task<int> Run(TagOptions options)
    {
        return options.Action switch
        {
            "add" or "create" => await AddTag(options),
            "remove" or "delete" => await RemoveTag(options),
            "rename" => await RenameTag(options),
            "list" => await ListTags(options),
            _ => 1
        };
    }

    public override Task ProvideCompletions(string[] inputs)
    {
        throw new NotImplementedException();
    }

    #endregion

    #region Private methods

    private async Task<int> AddTag(TagOptions options)
    {
        var arguments = options.Arguments.ToList();
        if (arguments.Count != 1) return 1;

        return await TagRepository.EnsureTagsExistsAsync([arguments[0]]) ? 0 : 1;
    }

    private async Task<int> RemoveTag(TagOptions options)
    {
        var arguments = options.Arguments.ToList();
        if (arguments.Count != 1) return 1;

        return !await TagRepository.DeleteAsync(arguments[0]) ? 1 : 0;
    }

    private async Task<int> RenameTag(TagOptions options)
    {
        var arguments = options.Arguments.ToList();
        if (arguments.Count != 2) return 1;

        var tag = await TagRepository.GetByIdAsync(arguments[0]);
        if (tag is null) return 1;

        tag.Name = arguments[1];

        return !await TagRepository.UpdateAsync(tag) ? 1 : 0;
    }

    private async Task<int> ListTags(TagOptions _)
    {
        var tags = await TagRepository.GetAsync();
        foreach (var tag in tags)
        {
            Console.WriteLine("{0}: {1}", tag.Id, tag.Name);
        }

        return 0;
    }

    #endregion
}