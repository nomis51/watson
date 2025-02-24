using System.Security.Cryptography;
using System.Text;
using Watson.Core.Helpers.Abstractions;

namespace Watson.Core.Helpers;

public class IdHelper : IIdHelper
{
    #region Constants

    private const string Base36Chars = "0123456789abcdefghijklmnopqrstuvwxyz";

    #endregion

    #region Public methods

    public string GenerateId(int length = 8)
    {
        if (length < 8) throw new ArgumentException("Length must be at least 8", nameof(length));

        var bytes = SHA1.HashData(Encoding.UTF8.GetBytes(Guid.NewGuid().ToString()));
        return ToBase36(bytes, length)[..length];
    }

    #endregion

    #region Private methods

    private static string ToBase36(byte[] bytes, int length)
    {
        var value = BitConverter.ToUInt64(bytes.Take(length).ToArray(), 0);
        StringBuilder sb = new();

        while (value > 0)
        {
            sb.Append(Base36Chars[(int)(value % 36)]);
            value /= 36;
        }

        return sb.ToString().PadLeft(length, '0');
    }

    #endregion
}