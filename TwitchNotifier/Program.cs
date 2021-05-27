using TwitchNotifier.src.config;
using TwitchNotifier.src.Twitch;

namespace TwitchNotifier {
    class Program {
        static void Main(string[] args) {
            var config = new Config();
           
            if (!config.CreateConfig()) {
                // Start monitoring (creates and blocks task)
                new LiveMonitoring();
            }
        }
    }
}
