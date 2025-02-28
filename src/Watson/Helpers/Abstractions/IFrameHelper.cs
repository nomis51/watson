﻿using Watson.Core.Models;

namespace Watson.Helpers.Abstractions;

public interface IFrameHelper
{
    Task<bool> CreateFrame(Frame frame, DateTime? toTime = null);
}