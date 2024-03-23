using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class WarpExtensions 
{
    // from: https://stackoverflow.com/questions/2431732/checking-if-a-bit-is-set-or-not
    /// <summary>
    /// Returns whether the bit at the specified position is set.
    /// </summary>
    /// <typeparam name="T">Any integer type.</typeparam>
    /// <param name="t">The value to check.</param>
    /// <param name="pos">
    /// The position of the bit to check, 0 refers to the least significant bit.
    /// </param>
    /// <returns>true if the specified bit is on, otherwise false.</returns>
    public static bool IsBitSet<T>(this T t, int pos) where T : struct, IConvertible
    {
        var value = t.ToInt64(System.Globalization.CultureInfo.CurrentCulture);
        return (value & (1 << pos)) != 0;
    }


    // from: https://stackoverflow.com/questions/2776673/how-do-i-truncate-a-net-string
    public static string Truncate(this string value, int maxLength)
    {
        if (string.IsNullOrEmpty(value)) return value;
        return value.Length <= maxLength ? value : value.Substring(0, maxLength);
    }
}
