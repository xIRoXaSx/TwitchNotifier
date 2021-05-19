using System;
using System.Collections.Generic;
using System.Text;

namespace TwitchNotifier.src.Logging {
    class Logging {

        /// <summary>
        /// For basic logging
        /// </summary>
        /// <param name="text">The text to log</param>
        public static void Log(string text) {
            Console.WriteLine(text);
        }


        /// <summary>
        /// Log information (console = green)
        /// </summary>
        /// <param name="text">The text to log</param>
        public static void Info(string text) {
            var colorBeforeChange = Console.ForegroundColor;
            
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

            Console.ForegroundColor = ConsoleColor.Red;
            Console.Write("WRN: ");

            Console.ForegroundColor = colorBeforeChange;
            Console.WriteLine(text);
        }
    }
}
