using Watson.Core.Models;

namespace Watson.Core.Repositories.Abstractions;

public interface IFrameRepository : IRepository<Frame>
{
    Task<Frame?> GetNextFrameAsync(DateTimeOffset time);
    Task<Frame?> GetPreviousFrameAsync(DateTimeOffset time);
    Task<IEnumerable<Frame>> GetAsync(DateTimeOffset fromTime, DateTimeOffset toTime);

    Task<IEnumerable<Frame>> GetAsync(
        DateTimeOffset fromTime,
        DateTimeOffset toTime,
        List<string> projectIds,
        List<string> tagIds
    );

    Task AssociateTagsAsync(string frameId, IEnumerable<string> tags);
}