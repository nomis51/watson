using Watson.Core.Models.Database;

namespace Watson.Helpers.Abstractions;

public interface IFrameHelper
{
    Task<Frame?> CreateFrame(Frame frame, DateTime? toTime = null);
}