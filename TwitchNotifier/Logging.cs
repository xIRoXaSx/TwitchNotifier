using System;

namespace TwitchNotifier; 

internal class Logging {
    private static bool IsDebugEnabled = Program.Conf.GeneralSettings.Debug;

    /// <summary>
    /// Log debug information.
    /// </summary>
    /// <param name="value"><c>String</c> - The value to log.</param>
    internal static void Debug(string value) {
        if (!IsDebugEnabled)
            return;
        var initColor = Console.ForegroundColor;
        Console.ForegroundColor = ConsoleColor.Gray;
        Console.WriteLine("DBG: {0} - {1}", CurrentDateTime(), value);
        Console.ForegroundColor = initColor;
    }
    
    /// <summary>
    /// Log information.
    /// </summary>
    /// <param name="value"><c>String</c> - The value to log.</param>
    internal static void Info(string value) {
        var initColor = Console.ForegroundColor;
        Console.ForegroundColor = ConsoleColor.White;
        Console.WriteLine("INF: {0} - {1}", CurrentDateTime(), value);
        Console.ForegroundColor = initColor;
    }
    
    /// <summary>
    /// Log errors.
    /// </summary>
    /// <param name="value"><c>String</c> - The value to log.</param>
    internal static void Error(string value) {
        var initColor = Console.ForegroundColor;
        Console.ForegroundColor = ConsoleColor.Red;
        Console.WriteLine("ERR: {0} - {1}", CurrentDateTime(), value);
        Console.ForegroundColor = initColor;
    }

    /// <summary>
    /// Get the current date and time as string.
    /// </summary>
    /// <returns><c>String</c> - Current DateTime</returns>
    private static string CurrentDateTime() {
        return DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");
    }
}