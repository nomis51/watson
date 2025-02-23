using Watson.Core.Models;

namespace Watson.Core.Repositories.Abstractions;

public interface IFrameRepository : IRepository<Frame>
{
    Task<Frame?> GetNextFrameAsync(DateTimeOffset time);
    Task<Frame?> GetPreviousFrameAsync(DateTimeOffset time);
    Task<IEnumerable<Frame>> GetAsync(DateTimeOffset fromTime, DateTimeOffset toTime);
}