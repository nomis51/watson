using Watson.Core.Models.Database;

namespace Watson.Helpers.Abstractions;

public interface IFrameHelper
{
    Task<bool> CreateFrame(Frame frame, DateTime? toTime = null);
}