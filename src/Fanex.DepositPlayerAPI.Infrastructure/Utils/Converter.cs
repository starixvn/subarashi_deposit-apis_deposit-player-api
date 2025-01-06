namespace Fanex.DepositPlayerAPI.Infrastructure.Utils;

using System.Diagnostics;
using System.Globalization;

public static class Converter
{
    [DebuggerStepThrough]
    public static decimal ToDecimal(object value, decimal defaultValue = 0)
        => decimal.TryParse(ToString(value), out var parseValue) ? parseValue : defaultValue;

    [DebuggerStepThrough]
    public static bool ToBoolean(object value)
        => bool.TryParse(ToString(value), out var parseValue) && parseValue;

    [DebuggerStepThrough]
    public static byte ToByte(object value, byte defaultValue = 0)
        => byte.TryParse(ToString(value), out var parseValue) ? parseValue : defaultValue;

    [DebuggerStepThrough]
    public static int ToInt(object value, int defaultValue = 0)
        => int.TryParse(ToString(value), out var parseValue) ? parseValue : defaultValue;

    [DebuggerStepThrough]
    public static int? ToNullableInt(object value, int? defaultValue = null)
        => int.TryParse(ToString(value), out var parseValue) ? parseValue : defaultValue;

    [DebuggerStepThrough]
    public static long ToLong(object value, long defaultValue = 0)
        => long.TryParse(ToString(value), out var parseValue) ? parseValue : defaultValue;

    [DebuggerStepThrough]
    public static long? ToNullableLong(object value, long? defaultValue = null)
        => long.TryParse(ToString(value), out var parseValue) ? parseValue : defaultValue;

    [DebuggerStepThrough]
    public static string ToString(object value, string defaultValue = "")
        => value?.ToString() ?? defaultValue;

    [DebuggerStepThrough]
    public static DateTime ToDateTime(object value, DateTime defaultValue = default)
        => DateTime.TryParse(ToString(value), DateTimeFormatInfo.CurrentInfo, out var parseValue) ? parseValue : defaultValue;
}