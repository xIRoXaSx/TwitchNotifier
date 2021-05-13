using System;
using TwitchNotifier.src.config;

namespace TwitchNotifier {
    class Program {
        static void Main(string[] args) {
            Console.WriteLine("Hello World!");
            Parser.Serialize(new Config());
        }
    }
}
