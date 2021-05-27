using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
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
    /// Set up the live monitoring feature
    /// </summary>
    class LiveMonitoring {
        private LiveStreamMonitorService Monitor;
        private TwitchAPI API;

        public LiveMonitoring() {
            Task.Run(() => ConfigureLiveMonitorAsync()).GetAwaiter().GetResult();
        }


        private async Task ConfigureLiveMonitorAsync() {
            var deserializer = new DeserializerBuilder().Build();
            try {

                //var config = deserializer.Deserialize<Config>(File.ReadAllText(Config.configFileLocation, Encoding.UTF8));
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

                //Monitor.OnStreamUpdate += Monitor_OnStreamUpdate;
                Monitor.OnStreamOnline += Monitor_OnStreamOnline;
                Monitor.OnStreamOffline += Monitor_OnStreamOffline;
                Monitor.OnServiceStarted += Monitor_OnServiceStarted;
                Monitor.OnChannelsSet += Monitor_OnChannelsSet;

                Monitor.SetChannelsByName(usernames.Distinct().ToList());

                await Monitor.UpdateLiveStreamersAsync();

                Monitor.Start();
                await API.Helix.Channels.CheckCredentialsAsync();

            } catch (Exception e) {
                Console.WriteLine(e);
            }
            
            await Task.Delay(-1);
        }

        private void Monitor_OnChannelsSet(object sender, OnChannelsSetArgs e) {
            Log.Info("Channel list has been set!");
            Log.Debug("  > Channles: ");
            e.Channels.ForEach(x => Log.Debug("    - " + x));
        }

        private void Monitor_OnServiceStarted(object sender, OnServiceStartedArgs e) {
            Log.Info("Service has started!");
        }

        private void Monitor_OnStreamUpdate(object sender, OnStreamUpdateArgs e) {
            // What gets updated here?

            Log.Info("Update: " + e.Channel);
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
                    Channel = new Channel() {
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
            Log.Info("Online: " + e.Channel);
            Log.Debug(e.Channel + " went online!");

            var cacheEntry = new CacheEntry() {
                Key = e.Stream.UserId,
                Value = e.Stream.UserId
            };

            if (!Cache.CheckCacheEntryExpiration(cacheEntry)) {
                var channels = Config.GetEventObjectsByTwitchChannelName(configEventName, e.Channel);
                var placeholderHelper = new PlaceholderHelper() {
                    Stream = e.Stream,
                    Channel = new Channel() {
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
