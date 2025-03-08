using System.Reflection;
using System.Text.Json.Serialization;

namespace Watson.Core.Extensions;

public static class ObjectExtensions
{
    #region Public methods

    public static bool GetJsonPathValue(this object obj, string jsonPath, out object? value)
    {
        if (string.IsNullOrEmpty(jsonPath))
        {
            value = null;
            return false;
        }

        var currentValue = obj;
        var parts = jsonPath.Split('.', StringSplitOptions.TrimEntries | StringSplitOptions.RemoveEmptyEntries);

        foreach (var segment in parts)
        {
            if (currentValue is null)
            {
                value = null;
                return false;
            }

            var type = currentValue.GetType();

            var property = type.GetProperties()
                .FirstOrDefault(p =>
                    p.GetCustomAttribute<JsonPropertyNameAttribute>()?.Name == segment ||
                    string.Equals(p.Name, segment, StringComparison.OrdinalIgnoreCase)
                );
            if (property == null)
            {
                value = null;
                return false;
            }

            currentValue = property.GetValue(currentValue);
        }

        value = currentValue;
        return true;
    }

    #endregion
}