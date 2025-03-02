using System.Security.Cryptography;
using Watson.Core.Helpers.Abstractions;

namespace Watson.Core.Helpers;

public class IdHelper : IIdHelper
{
    #region Public methods

    public string GenerateId()
    {
        var timestamp = DateTime.Now.Ticks;
        var randomValue = RandomNumberGenerator.GetInt32(0, 0xFFFF);
        var combined = (timestamp << 16) | (uint)randomValue;
        return combined.ToString("x8")[^8..];
    }

    #endregion
}