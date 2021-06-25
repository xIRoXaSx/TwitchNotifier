using System.Collections.Generic;
using System.IO;
using System.Runtime.Caching;
using System.Text;
using TwitchNotifier.src;
using TwitchNotifier.src.config;
using TwitchNotifier.src.Helper;
using TwitchNotifier.src.Twitch;
using YamlDotNet.Serialization;

namespace TwitchNotifier {
    class Program {
        static void Main(string[] args) {
            System.Console.OutputEncoding = Encoding.UTF8;
            var config = new Config();
            
            if (!config.CreateConfig()) {
                var deserializer = new DeserializerBuilder().Build();
                var deserializedConfig = (dynamic)deserializer.Deserialize<dynamic>(File.ReadAllText(Config.configFileLocation, Encoding.UTF8));

                var listOfCacheEntries = new List<CacheEntry>() {
                    new CacheEntry() {
                        Priority = CacheItemPriority.NotRemovable,
                        Key = LiveMonitoring.defaultConfigCacheKey,
                        Value = File.ReadAllText(Config.configFileLocation, Encoding.UTF8)
                    },
                    new CacheEntry() {
                        Priority = CacheItemPriority.NotRemovable,
                        Key = LiveMonitoring.defaultSkipStartupNotifications,
                        Value = (deserializedConfig?["Settings"]?[LiveMonitoring.defaultSkipStartupNotifications])
                    },
                    new CacheEntry() {
                        Priority = CacheItemPriority.NotRemovable,
                        Key = LiveMonitoring.defaultNotificationThresholdInSeconds,
                        Value = (deserializedConfig?["Settings"]?[LiveMonitoring.defaultNotificationThresholdInSeconds])
                    }
                };

                if (deserializedConfig["Settings"].ContainsKey("Debug")) {
                    listOfCacheEntries.Add(new CacheEntry() {
                        Priority = CacheItemPriority.NotRemovable,
                        CreateSha256Sum = false,
                        Key = "Debug",
                        Value = deserializedConfig["Settings"]["Debug"]
                    });;
                }

                foreach (var cacheEntry in listOfCacheEntries) {
                    Cache.CheckCacheEntryExpiration(cacheEntry);
                }

                // Start LiveMonitoring (creates and blocks task)
                new LiveMonitoring();
            }
        }
    }
}
