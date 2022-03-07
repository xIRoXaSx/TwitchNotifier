namespace TwitchNotifier; 

public static class ExtensionMethods {
    /// <summary>
    /// Try to parse a boolean from string.
    /// If parsing is not possible, use fallback value instead.
    /// </summary>
    /// <param name="value"><c>String</c> - The value to parse.</param>
    /// <param name="fallback"><c>Bool</c> - The fallback if the parsing fails.</param>
    /// <returns><c>Bool</c> - The parsed value or its fallback.</returns>
    public static bool ParseToBoolean(this string value, bool fallback = false) {
        return !bool.TryParse(value, out var returnValue) ? fallback : returnValue;
    }
}