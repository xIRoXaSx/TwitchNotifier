using System;
using System.IO;
using TwitchNotifier.src.config;

namespace TwitchNotifier.src.Logging {
    class Log {
        private static void LogToFile(string text) {
            try {
                var logPath = Config.configLocation + Path.DirectorySeparatorChar + "logs" + Path.DirectorySeparatorChar;
                
                if (!Directory.Exists(logPath)) {
                    Directory.CreateDirectory(logPath);
                }

                File.AppendAllText(logPath + GetLogFileDateString() + ".log", text + Environment.NewLine);
            } catch (Exception e) {
                Error(e.ToString());
            }
        }

        /// <summary>
        /// Log debug information (console = cyan) + file
        /// </summary>
        /// <param name="text">The text to log</param>
        public static void Debug(string text) {
            var colorBeforeChange = Console.ForegroundColor;

            Console.Write("[" + GetLogDateString() + "] ");
            Console.ForegroundColor = ConsoleColor.Cyan;
            Console.Write("DBG: ");

            Console.ForegroundColor = colorBeforeChange;
            Console.WriteLine(text);
            LogToFile(text);
        }

        /// <summary>
        /// Log information (console = green)
        /// </summary>
        /// <param name="text">The text to log</param>
        public static void Info(string text) {
            var colorBeforeChange = Console.ForegroundColor;
            
            Console.Write("[" + GetLogDateString() + "] ");
            Console.ForegroundColor = ConsoleColor.Green;
            Console.Write("INF: ");

            Console.ForegroundColor = colorBeforeChange;
            Console.WriteLine(text);
        }

        /// <summary>
        /// Log warnings (console = yellow)
        /// </summary>
        /// <param name="text">The text to log</param>
        public static void Warn(string text) {
            var colorBeforeChange = Console.ForegroundColor;

            Console.Write("[" + GetLogDateString() + "] ");
            Console.ForegroundColor = ConsoleColor.Yellow;
            Console.Write("WRN: ");

            Console.ForegroundColor = colorBeforeChange;
            Console.WriteLine(text);
        }

        /// <summary>
        /// Log errors (console = red)
        /// </summary>
        /// <param name="text">The text to log</param>
        public static void Error(string text) {
            var colorBeforeChange = Console.ForegroundColor;

            Console.Write("[" + GetLogDateString() + "] ");
            Console.ForegroundColor = ConsoleColor.Red;
            Console.Write("ERR: ");

            Console.ForegroundColor = colorBeforeChange;
            Console.WriteLine(text);
        }

        /// <summary>
        /// Gets the date and time for console logging (eg.: 2021-05-21 09:00:01)
        /// </summary>
        public static string GetLogDateString() {
            return DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");
        }

        /// <summary>
        /// Gets the date and time for file logging (eg.: 2021-05-21_09-00-01)
        /// </summary>
        public static string GetLogFileDateString() {
            return DateTime.Now.ToString("yyyy-MM-dd");
        }
    }
}
