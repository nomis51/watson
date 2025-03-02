using Watson.Core.Models.Database;

namespace Watson.Core.Repositories.Abstractions;

public interface IFrameRepository : IRepository<Frame>
{
    Task<Frame?> GetNextFrameAsync(DateTime time);
    Task<Frame?> GetPreviousFrameAsync(DateTime time);
    Task<IEnumerable<Frame>> GetAsync(DateTime fromTime, DateTime toTime);

    Task<IEnumerable<Frame>> GetAsync(
        DateTime fromTime,
        DateTime toTime,
        List<string> projectIds,
        List<string> tagIds,
        List<string> ignoredProjectIds,
        List<string> ignoredTagIds
    );

    Task AssociateTagsAsync(string frameId, IEnumerable<string> tags);
}