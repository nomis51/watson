using Watson.Core.Models;
using Watson.Core.Repositories.Abstractions;
using Watson.Helpers.Abstractions;

namespace Watson.Helpers;

public class FrameHelper : IFrameHelper
{
    #region Members

    private readonly IFrameRepository _frameRepository;

    #endregion

    #region Constructors

    public FrameHelper(IFrameRepository frameRepository)
    {
        _frameRepository = frameRepository;
    }

    #endregion

    #region Public methods

    public async Task<bool> CreateFrame(Frame frame, DateTimeOffset? toTime = null)
    {
        if (toTime is null)
        {
            return await _frameRepository.InsertAsync(frame);
        }

        var toTimeNextFrame = await _frameRepository.GetNextFrameAsync(toTime.Value);
        if (toTimeNextFrame is null)
        {
            return await _frameRepository.InsertAsync(frame);
        }

        var fromTimePreviousFrame = await _frameRepository.GetPreviousFrameAsync(frame.TimestampAsDateTime);
        if (fromTimePreviousFrame is null)
        {
            // we're at the beginning of the day
            var result = await _frameRepository.InsertAsync(frame);
            if (!result) return false;

            toTimeNextFrame.Timestamp = toTime.Value.ToUnixTimeSeconds();
            result = await _frameRepository.UpdateAsync(toTimeNextFrame);

            return result;
        }

        var toTimePreviousFrame = await _frameRepository.GetPreviousFrameAsync(toTime.Value);
        if (toTimePreviousFrame is null)
        {
            // we have a problem, toTime cannot not have a previous if fromTime does
            return false;
        }

        if (fromTimePreviousFrame.Id == toTimePreviousFrame.Id)
        {
            // same previous frame, means the frame is contained in a single frame
            // we add the new one, and cloned the previous frame and add it after the new frame with toTime
            var result = await _frameRepository.InsertAsync(frame);
            if (!result) return false;

            fromTimePreviousFrame.Timestamp = toTime.Value.ToUnixTimeSeconds();
            fromTimePreviousFrame.Id = string.Empty;
            result = await _frameRepository.InsertAsync(fromTimePreviousFrame);
            return result;
        }

        // if not the same previous frame, but toTime previous frame is fromTime next frame
        if (toTimePreviousFrame.Id == toTimeNextFrame.Id)
        {
            // we're over 2 frames
            var result = await _frameRepository.InsertAsync(frame);
            if (!result) return false;

            toTimePreviousFrame.Timestamp = toTime.Value.ToUnixTimeSeconds();
            result = await _frameRepository.UpdateAsync(toTimePreviousFrame);

            return result;
        }

        // we're over multiple frames, so we delete the middle frames
        var framesToDelete = await _frameRepository.GetAsync(
            DateTimeOffset.FromUnixTimeSeconds(toTimeNextFrame.Timestamp),
            DateTimeOffset.FromUnixTimeSeconds(toTimePreviousFrame.Timestamp)
        );

        var result2 = await _frameRepository.DeleteManyAsync(
            framesToDelete.Select(e => e.Id)
        );
        if (!result2) return false;

        result2 = await _frameRepository.InsertAsync(frame);
        if (!result2) return false;

        toTimeNextFrame.Timestamp = toTime.Value.ToUnixTimeSeconds();
        result2 = await _frameRepository.UpdateAsync(toTimeNextFrame);
        return result2;
    }

    #endregion
}