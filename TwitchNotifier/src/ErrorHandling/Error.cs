using System;
using System.Collections.Generic;
using System.Text;

namespace TwitchNotifier.src.ErrorHandling {
    class Error {
        /// <summary>
        /// The message that should be displayed (1st line)
        /// </summary>
        public string Message { get; set; } = "Something went wrong, sorry!";
        
        /// <summary>
        /// If caught, use the exception here to get the message out of it
        /// </summary>
        public Exception Exception { get; set; } = new Exception();

        /// <summary>
        /// Whether the error should terminate the proram or not
        /// </summary>
        public bool IsTerminating { get; set; } = false;


        /// <summary>
        /// Write an error to the console
        /// </summary>
        public void WriteError() {
            var colorBeforeChange = Console.ForegroundColor;

            Console.WriteLine("=== Exception caught ===");
            Console.ForegroundColor = colorBeforeChange;
            Console.WriteLine("  > MESSAGE: " + Message);
            
            if (Exception != null) {
                Console.WriteLine("  > EXCEPTION: " + Exception.Message);
            }

            if (IsTerminating) {
                Console.WriteLine("\nPress any key to close the program");
                Console.Read();
                Environment.Exit(1);
            }
        }
    }
}
