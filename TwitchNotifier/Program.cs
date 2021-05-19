using System;
using TwitchNotifier.src.config;
using TwitchNotifier.src.Logging;
using TwitchNotifier.src.Twitch;

namespace TwitchNotifier {
    class Program {
        //static void Main(string[] args) 
        //     => new LiveMonitoring().ConfigureLiveMonitorAsync().GetAwaiter().GetResult();

        static void Main(string[] args) {
            var config = new Config();
            config.CreateConfig();
            //new LiveMonitoring();

            var eventObject = Config.GetEventObjectByTwitchChannelName("OnStreamStart", "xiroxasx");

        }
    }
}
