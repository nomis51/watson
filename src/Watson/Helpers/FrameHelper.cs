using Watson.Core.Models.Database;
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

    public async Task<bool> CreateFrame(Frame frame, DateTime? toTime = null)
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

        var fromTimePreviousFrame = await _frameRepository.GetPreviousFrameAsync(frame.TimeAsDateTime);
        if (fromTimePreviousFrame is null)
        {
            return await CreateFrameAtTheBeginningOfTheDay(frame, toTime.Value, toTimeNextFrame);
        }

        var toTimePreviousFrame = await _frameRepository.GetPreviousFrameAsync(toTime.Value);
        if (toTimePreviousFrame is null)
        {
            // we have a problem, toTime cannot not have a previous frame if fromTime does, that's impossible
            return false;
        }

        if (fromTimePreviousFrame.Id == toTimePreviousFrame.Id)
        {
            return await CreateFrameContainedInAFrame(frame, toTime.Value, fromTimePreviousFrame);
        }

        // if not the same previous frame, but toTime previous frame is fromTime next frame
        var fromTimeNextFrame = await _frameRepository.GetNextFrameAsync(frame.TimeAsDateTime);
        if (fromTimeNextFrame is not null && toTimePreviousFrame.Id == fromTimeNextFrame.Id)
        {
            return await CreateFrameOverTwoFrames(frame, toTime.Value, toTimePreviousFrame);
        }

        return await CreateFrameOverMultipleFrames(frame, toTime.Value, toTimeNextFrame, toTimePreviousFrame);
    }

    #endregion

    #region Private methods

    private async Task<bool> CreateFrameOverMultipleFrames(
        Frame frame,
        DateTime toTime,
        Frame toTimeNextFrame,
        Frame toTimePreviousFrame
    )
    {
        var framesToDelete = await _frameRepository.GetAsync(
            new DateTime(toTimeNextFrame.Time),
            new DateTime(toTimePreviousFrame.Time)
        );
        var framesToDeleteIds = framesToDelete.Select(e => e.Id).ToList();

        if (framesToDeleteIds.Count != 0)
        {
            var result2 = await _frameRepository.DeleteManyAsync(
                framesToDeleteIds
            );
            if (!result2) return false;
        }

        var result3 = await _frameRepository.InsertAsync(frame);
        if (!result3) return false;

        toTimeNextFrame.Time = toTime.Ticks;
        result3 = await _frameRepository.UpdateAsync(toTimeNextFrame);
        return result3;
    }

    private async Task<bool> CreateFrameOverTwoFrames(Frame frame, DateTime toTime, Frame toTimePreviousFrame)
    {
        var result = await _frameRepository.InsertAsync(frame);
        if (!result) return false;

        toTimePreviousFrame.Time = toTime.Ticks;
        result = await _frameRepository.UpdateAsync(toTimePreviousFrame);

        return result;
    }

    private async Task<bool> CreateFrameContainedInAFrame(
        Frame frame,
        DateTime toTime,
        Frame fromTimePreviousFrame
    )
    {
        // we add the new one, and cloned the previous frame and add it after the new frame with toTime
        var result = await _frameRepository.InsertAsync(frame);
        if (!result) return false;

        fromTimePreviousFrame.Time = toTime.Ticks;
        fromTimePreviousFrame.Id = string.Empty;
        result = await _frameRepository.InsertAsync(fromTimePreviousFrame);
        return result;
    }

    private async Task<bool> CreateFrameAtTheBeginningOfTheDay(
        Frame frame,
        DateTime toTime,
        Frame toTimeNextFrame
    )
    {
        var result = await _frameRepository.InsertAsync(frame);
        if (!result) return false;

        toTimeNextFrame.Time = toTime.Ticks;
        result = await _frameRepository.UpdateAsync(toTimeNextFrame);

        return result;
    }

    #endregion
}