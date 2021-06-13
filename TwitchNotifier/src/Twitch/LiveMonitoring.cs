using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.Caching;
using System.Text;
using System.Threading.Tasks;
using TwitchLib.Api;
using TwitchLib.Api.Services;
using TwitchLib.Api.Services.Events;
using TwitchLib.Api.Services.Events.LiveStreamMonitor;
using TwitchNotifier.src.config;
using TwitchNotifier.src.Helper;
using TwitchNotifier.src.Logging;
using TwitchNotifier.src.WebRequests;
using YamlDotNet.Serialization;

namespace TwitchNotifier.src.Twitch {
    /// <summary>
    /// Enum for disposing / stoping the LiveStreamMonitorService and TwitchAPI
    /// </summary>
    public enum DisposableInstance {
        Monitor = 0,
        API = 1,
        All = 2
    }

    /// <summary>
    /// Set up the live monitoring feature
    /// </summary>
    class LiveMonitoring {
        private FileSystemWatcher fileSystemWatcher;
        private LiveStreamMonitorService Monitor;
        private TwitchAPI API;
        private bool disposed = false;
        public static string defaultConfigCacheKey  = "DefaultConfig";

        public LiveMonitoring() {
            Task.Run(() => ConfigureLiveMonitorAsync()).GetAwaiter().GetResult();
        }

        private async Task ConfigureLiveMonitorAsync() {
            var deserializer = new DeserializerBuilder().Build();
            
            try {
                var config = deserializer.Deserialize<dynamic>(File.ReadAllText(Config.configFileLocation, Encoding.UTF8));

                API = new TwitchAPI();
                API.Settings.ClientId = config["Settings"]["ClientID"];
                API.Settings.AccessToken = config["Settings"]["AccessToken"];

                Monitor = new LiveStreamMonitorService(API, 5);

                // Iterate through all event properties of the config
                var usernames = new List<string>();
                foreach (var property in typeof(TwitchNotifierSettings).GetProperties()) {
                    // Events like "OnStreamStart", "OnStreamEnd", "OnFollow", ...
                    var twitchEvent = config["TwitchNotifier"][property.Name];

                    // eventNode is the node for all settings below each event
                    foreach (var eventNode in twitchEvent) {
                        List<string> usernameList = ((List<object>)eventNode.Value["Twitch"]["Usernames"]).Select(x => (string)x).ToList();
                        usernames.AddRange(usernameList);
                    }
                }

                Monitor.OnStreamOnline += Monitor_OnStreamOnline;
                Monitor.OnStreamOffline += Monitor_OnStreamOffline;
                Monitor.OnChannelsSet += Monitor_OnChannelsSet;
                Monitor.OnServiceStarted += Monitor_OnServiceStarted;
                Monitor.OnServiceStopped += Monitor_OnServiceStopped;

                Monitor.SetChannelsByName(usernames.Distinct().ToList());

                await Monitor.UpdateLiveStreamersAsync();

                Monitor.Start();
                await API.Helix.Channels.CheckCredentialsAsync();

            } catch (Exception e) {
                Console.WriteLine(e);
            }

            SetFileWatchers();

            await Task.Delay(-1);
        }

        /// <summary>
        /// Dispose the Monitor instance
        /// </summary>
        protected virtual void DisposeInstance(DisposableInstance disposableInstance) {
            if (!disposed) {
                // Called if the channels in the config file have been modified
                if (Monitor != null && (disposableInstance == DisposableInstance.Monitor || disposableInstance == DisposableInstance.All)) {
                    Monitor.ClearCache();
                    Monitor.Stop();
                    GC.SuppressFinalize(this);
                }

                // Called if the token has been changed
                if (API != null && (disposableInstance == DisposableInstance.API || disposableInstance == DisposableInstance.All)) {
                    API = null;
                    disposed = true;
                }

                disposed = true;
            }
        }

        /// <summary>
        /// The filewatcher to listen to changes of the config file
        /// </summary>
        public void SetFileWatchers() {
            fileSystemWatcher = new FileSystemWatcher() {
                Path = Config.configLocation,
                Filter = "config.yml",
                EnableRaisingEvents = true,
                NotifyFilter = NotifyFilters.LastWrite
            };

            fileSystemWatcher.Changed += HotloadConfig;
        }

        /// <summary>
        /// Called if the config file has been changed
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void HotloadConfig(object sender, FileSystemEventArgs e) {
            var cachedConfig = MemoryCache.Default.Get(Cache.HashString(defaultConfigCacheKey));
            var needToDispose = false;

            if (cachedConfig != null) {
                List<string> beforeChange;
                List<string> afterChange;
                IEnumerable<string> configOld = ((CacheEntry)cachedConfig).Value.ToString().Split(Environment.NewLine).Distinct();

                try {
                    using (FileStream fileStream = new FileStream(e.FullPath, FileMode.Open, FileAccess.Read, FileShare.ReadWrite)) {
                        using (StreamReader streamReader = new StreamReader(fileStream)) {
                            IEnumerable<string> configNew = streamReader.ReadToEnd().Split(Environment.NewLine).Distinct(); // ReadConfigNotifierSettings(e.FullPath);
                            afterChange = configOld.Except(configNew).ToList();
                            beforeChange = configNew.Except(configOld).ToList();
                        }
                    }

                    // If afterChange is in node "Usernames" or is value of "WebHookUrl", dispose Monitor instance
                    var deserializer = new DeserializerBuilder().Build();
                    var config = deserializer.Deserialize<dynamic>(File.ReadAllText(Config.configFileLocation, Encoding.UTF8));
                    var oldConfig = deserializer.Deserialize<dynamic>((((CacheEntry)cachedConfig).Value.ToString()));
                    List<string> twitchChannelsAfterwards = GetAllTwitchUsernamesFromConfig(config);
                    List<string> twitchChannelsBefore = GetAllTwitchUsernamesFromConfig(oldConfig);
                    List<string> webhookUrlsAfterwards = new List<string>();
                    List<string> webhookUrlsBefore = new List<string>();

                    webhookUrlsAfterwards = GetWebhookProperties(config, "WebHookUrl");
                    webhookUrlsBefore = GetWebhookProperties(oldConfig, "WebHookUrl");

                    // Line removed from config file
                    var twitchChannelsModified = afterChange.Select(x => x.Trim()).Any(x => twitchChannelsBefore.Contains(x.Replace("- ", "")) && !twitchChannelsAfterwards.Contains(x.Replace("- ", "")));
                    var webhooksModified = afterChange.Select(x => x.Trim()).Any(x => webhookUrlsBefore.Contains(x.Replace("WebHookUrl: ", "")) && !webhookUrlsAfterwards.Contains(x.Replace("WebHookUrl: ", "")));
                    needToDispose = twitchChannelsModified || webhooksModified;

                    // Line added to config file
                    if (!needToDispose) {
                        twitchChannelsModified = beforeChange.Select(x => x.Trim()).Any(x => !twitchChannelsBefore.Contains(x.Replace("- ", "")) && twitchChannelsAfterwards.Contains(x.Replace("- ", "")));
                        webhooksModified = beforeChange.Select(x => x.Trim()).Any(x => !webhookUrlsBefore.Contains(x.Replace("WebHookUrl: ", "")) && webhookUrlsAfterwards.Contains(x.Replace("WebHookUrl: ", "")));
                        needToDispose = twitchChannelsModified || webhooksModified;
                    }

                } catch (Exception ex) {
                    Log.Error("Cannot read config file: " + ex.Message);
                }
            }
            
            var cacheEntry = new CacheEntry() {
                Key = e.FullPath,
                Value = string.Empty
            };

            if (!Cache.CheckCacheEntryExpiration(cacheEntry)) {
                Log.Debug("Configuration file has been changed!");
                
                if (needToDispose) {
                    Log.Debug("Disposing Monitor!");
                    DisposeInstance(DisposableInstance.Monitor);
                    new LiveMonitoring();
                }
            } else {
                var cachedEntry = MemoryCache.Default.Get(cacheEntry.Key);
                Log.Debug("Cooldown: " + (((CacheEntry)cachedEntry).ExpirationTime - DateTime.Now).TotalSeconds + " seconds");
            }
        }

        /// <summary>
        /// Get all Twitch channels of the config file
        /// </summary>
        /// <param name="config">The deserialized config</param>
        /// <returns></returns>
        private List<string> GetAllTwitchUsernamesFromConfig(dynamic config) {
            var usernames = new List<string>();
            foreach (var property in typeof(TwitchNotifierSettings).GetProperties()) {
                // Events like "OnStreamStart", "OnStreamEnd", "OnFollow", ...
                var twitchEvent = config["TwitchNotifier"][property.Name];

                // eventNode is the node for all settings below each event
                foreach (var eventNode in twitchEvent) {
                    List<string> usernameList = ((List<object>)eventNode.Value["Twitch"]["Usernames"]).Select(x => (string)x).ToList();
                    usernames.AddRange(usernameList);
                }
            }

            return usernames;
        }

        /// <summary>
        /// Get property of config file
        /// </summary>
        /// <returns><c>List&lt;string&gt;</c> containing the property of the config</returns>
        private List<string> GetWebhookProperties(dynamic config, dynamic configProperty) {
            var returnValue = new List<string>();
            
            foreach (var property in typeof(TwitchNotifierSettings).GetProperties()) {
                // Events like "OnStreamStart", "OnStreamEnd", "OnFollow", ...
                var twitchEvent = config["TwitchNotifier"][property.Name];

                // eventNode is the node for all settings below each event
                foreach (var eventNode in twitchEvent) {
                    returnValue.Add((string)eventNode.Value[configProperty]);
                }
            }

            return returnValue;
        }

        /// <summary>
        /// Read the config files TwitchNotifier settings
        /// </summary>
        /// <param name="fileFullPath">The full path of the file which should be read</param>
        /// <returns></returns>
        private string ReadConfigNotifierSettings(string fileFullPath) {
            using (var fileStream = new FileStream(fileFullPath, FileMode.Open, FileAccess.Read, FileShare.ReadWrite)) {
                using (var streamReader = new StreamReader(fileStream, Encoding.UTF8)) {
                    var yml = string.Empty;
                    var deserializer = new DeserializerBuilder().Build();
                    var serializer = new SerializerBuilder().ConfigureDefaultValuesHandling(DefaultValuesHandling.OmitDefaults & DefaultValuesHandling.OmitNull).Build();
                    var changedConfig = streamReader.ReadToEnd();
                    
                    if (!string.IsNullOrEmpty(changedConfig)) {
                        var dynamicConfig = deserializer.Deserialize<dynamic>(changedConfig);
                        yml = serializer.Serialize(dynamicConfig["TwitchNotifier"]);
                    }

                    return yml;
                }
            }
        }

        /// <summary>
        /// Called when SetChannels() is being called
        /// </summary>
        /// <param name="sender">The sender object</param>
        /// <param name="e">The event args</param>
        private void Monitor_OnChannelsSet(object sender, OnChannelsSetArgs e) {
            Log.Info("Channel list has been set!");
            Log.Debug("  > Channles: ");
            e.Channels.ForEach(x => Log.Debug("    - " + x));
        }

        /// <summary>
        /// Called when the LiveStreamMonitorService has started
        /// </summary>
        /// <param name="sender">The sender object</param>
        /// <param name="e">The event args</param>
        private void Monitor_OnServiceStarted(object sender, OnServiceStartedArgs e) {
            Log.Info("Service has started!");
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="sender">The sender object</param>
        /// <param name="e">The event args</param>
        private void Monitor_OnServiceStopped(object sender, OnServiceStoppedArgs e) {
            Log.Debug("Service has stopped!");
        }

        /// <summary>
        /// Called when stream went offlne<br/>
        /// <c>Todo</c>: Usful Try {} catch {}
        /// </summary>
        /// <param name="sender">The sender object</param>
        /// <param name="e">The event args</param>
        private async void Monitor_OnStreamOffline(object sender, OnStreamOfflineArgs e) {
            var configEventName = e.GetType().Name.Replace("args", "", StringComparison.OrdinalIgnoreCase);
            Log.Info("Offline: " + e.Channel);
            Log.Debug(e.Channel + " went offline!");

            var cacheEntry = new CacheEntry() {
                Key = e.Stream.UserId,
                Value = e.Stream.UserId
            };

            if (!Cache.CheckCacheEntryExpiration(cacheEntry)) {
                var channels = Config.GetEventObjectsByTwitchChannelName(configEventName, e.Channel);
                var placeholderHelper = new PlaceholderHelper() {
                    Stream = e.Stream,
                    Channel = new PlaceHolderChannelHelper() {
                        Name = e.Channel,
                        User = await API.V5.Channels.GetChannelByIDAsync(e.Stream.UserId)
                    }
                };

                SendEmbed(placeholderHelper, channels);
            } else {
                Log.Debug("Event \"" + configEventName + "\" triggered multiple times! Still in cooldown!");
                Log.Debug("Cooldown: " + (cacheEntry.ExpirationTime - DateTime.Now).TotalSeconds + " seconds");
            }
        }

        /// <summary>
        /// Called when stream went live<br/>
        /// <c>Todo</c>: Usful Try {} catch {}
        /// </summary>
        /// <param name="sender">The sender object</param>
        /// <param name="e">The event args</param>
        private async void Monitor_OnStreamOnline(object sender, OnStreamOnlineArgs e) {
            var configEventName = e.GetType().Name.Replace("args", "", StringComparison.OrdinalIgnoreCase);
            Log.Info(e.Channel + " went online!");

            var cacheEntry = new CacheEntry() {
                Key = e.Stream.UserId,
                Value = e.Stream.UserId
            };

            if (!Cache.CheckCacheEntryExpiration(cacheEntry)) {
                var channels = Config.GetEventObjectsByTwitchChannelName(configEventName, e.Channel);
                var placeholderHelper = new PlaceholderHelper() {
                    Stream = e.Stream,
                    Channel = new PlaceHolderChannelHelper() {
                        Name = e.Channel,
                        User = await API.V5.Channels.GetChannelByIDAsync(e.Stream.UserId)
                    }
                };

                SendEmbed(placeholderHelper, channels);
            } else {
                Log.Debug("Event \"" + configEventName + "\" triggered multiple times! Still in cooldown!");
                Log.Debug("Cooldown: " + (cacheEntry.ExpirationTime - DateTime.Now).TotalSeconds + " seconds");
            }
        }

        /// <summary>
        /// Send the embed over the specified URL
        /// </summary>
        /// <param name="placeholderHelper">The PlaceholderHelper to replace placeholders for the embed</param>
        /// <param name="channels">The Dictionary to loop through all Twitch channels</param>
        private static void SendEmbed(PlaceholderHelper placeholderHelper, Dictionary<string, object> channels) {
            if (channels.Count > 0) {
                foreach (var eventObject in channels) {
                    var condition = string.Empty;
                    
                    try {
                        condition = new Placeholders.Placeholder().ReplacePlaceholders((string)((dynamic)eventObject.Value)["Condition"], placeholderHelper);
                    } catch {
                        Log.Warn("Node \"Condition\" could not be found on eventnode \"" + eventObject.Key + "\"... Defaulted to empty condition!");
                    }
                    
                    if (Parser.CheckEventCondition(condition)) {
                        var embed = Parser.Deserialize(typeof(DiscordEmbed), ((dynamic)eventObject.Value)["Discord"], placeholderHelper);
                        new WebRequest() {
                            webHookUrl = ((dynamic)eventObject.Value)["WebHookUrl"],
                            discordEmbed = embed
                        }.SendRequest();
                    }
                }
            }
        }
    }
}