using Watson.Core.Models;

namespace Watson.Core.Repositories.Abstractions;

public interface IFrameRepository : IRepository<Frame>
{
    Task<Frame?> GetNextFrameAsync(DateTimeOffset time);
    Task<Frame?> GetPreviousFrameAsync(DateTimeOffset time);
    Task<IEnumerable<Frame>> GetAsync(DateTimeOffset fromTime, DateTimeOffset toTime);
    Task AssociateTagsAsync(string frameId, IEnumerable<string> tags);
}