using System.IO;
using System.Runtime.Caching;
using System.Text;
using TwitchNotifier.src;
using TwitchNotifier.src.config;
using TwitchNotifier.src.Helper;
using TwitchNotifier.src.Twitch;

namespace TwitchNotifier {
    class Program {

        static void Main(string[] args) {
            var config = new Config();
            
            if (!config.CreateConfig()) {

                var cacheEntry = new CacheEntry() {
                    Priority = CacheItemPriority.NotRemovable,
                    Key = LiveMonitoring.defaultConfigCacheKey,
                    Value = File.ReadAllText(Config.configFileLocation, Encoding.UTF8)
                };

                Cache.CheckCacheEntryExpiration(cacheEntry);
                
                // Start LiveMonitoring (creates and blocks task)
                new LiveMonitoring();
            }
        }
    }
}
