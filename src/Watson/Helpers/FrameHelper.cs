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

    public async Task<Frame?> CreateFrame(Frame frame, DateTime? toTime = null)
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
            return null;
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

    private async Task<Frame?> CreateFrameOverMultipleFrames(
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
            if (!result2) return null;
        }

        var result3 = await _frameRepository.InsertAsync(frame);
        if (result3 is null) return null;

        toTimeNextFrame.Time = toTime.Ticks;
        if (!await _frameRepository.UpdateAsync(toTimeNextFrame)) return null;

        return result3;
    }

    private async Task<Frame?> CreateFrameOverTwoFrames(Frame frame, DateTime toTime, Frame toTimePreviousFrame)
    {
        var result = await _frameRepository.InsertAsync(frame);
        if (result is null) return null;

        toTimePreviousFrame.Time = toTime.Ticks;
        if (!await _frameRepository.UpdateAsync(toTimePreviousFrame)) return null;

        return result;
    }

    private async Task<Frame?> CreateFrameContainedInAFrame(
        Frame frame,
        DateTime toTime,
        Frame fromTimePreviousFrame
    )
    {
        // we add the new one, and cloned the previous frame and add it after the new frame with toTime
        var result = await _frameRepository.InsertAsync(frame);
        if (result is null) return null;

        fromTimePreviousFrame.Time = toTime.Ticks;
        fromTimePreviousFrame.Id = string.Empty;
        if (await _frameRepository.InsertAsync(fromTimePreviousFrame) is null) return null;

        return result;
    }

    private async Task<Frame?> CreateFrameAtTheBeginningOfTheDay(
        Frame frame,
        DateTime toTime,
        Frame toTimeNextFrame
    )
    {
        var result = await _frameRepository.InsertAsync(frame);
        if (result is null) return null;

        toTimeNextFrame.Time = toTime.Ticks;
        if (!await _frameRepository.UpdateAsync(toTimeNextFrame)) return null;

        return result;
    }

    #endregion
}