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
using TwitchNotifier.src.Logging;
using YamlDotNet.Serialization;

namespace TwitchNotifier.src.Twitch {
    /// <summary>
    /// Set up the live monitoring feature
    /// </summary>
    class LiveMonitoring {
        private LiveStreamMonitorService Monitor;
        private TwitchAPI API;

        public LiveMonitoring() {
            //ConfigureLiveMonitorAsync();
            Task.Run(() => ConfigureLiveMonitorAsync()).GetAwaiter().GetResult();
        }


        private async Task ConfigureLiveMonitorAsync() {
            var deserializer = new DeserializerBuilder().Build();
            try {

                var config = deserializer.Deserialize<Config>(File.ReadAllText(Config.configFileLocation, Encoding.UTF8));

                API = new TwitchAPI();
                API.Settings.ClientId = config.Settings.ClientID;
                API.Settings.AccessToken = config.Settings.AccessToken;

                Monitor = new LiveStreamMonitorService(API, 5);

                // Iterate through all event properties of the config
                var usernames = new List<string>();
                foreach (var property in typeof(TwitchNotifierSettings).GetProperties()) {
                    // Events like "OnStreamStart", "OnStreamEnd", "OnFollow", ...
                    var twitchEvent = property.GetValue(config.TwitchNotifier);

                    // eventNode is the key node for all settings below each event
                    foreach (var eventNode in typeof(Event).GetProperties()) {
                        usernames.AddRange(((EventObject)eventNode.GetValue(twitchEvent)).Twitch.Usernames);
                    }
                }

                Monitor.SetChannelsByName(usernames.Distinct().ToList());

                await Monitor.UpdateLiveStreamersAsync();

                Monitor.OnStreamOnline += Monitor_OnStreamOnline;
                Monitor.OnStreamOffline += Monitor_OnStreamOffline;
                //Monitor.OnStreamUpdate += Monitor_OnStreamUpdate;
                Monitor.OnServiceStarted += Monitor_OnServiceStarted;
                Monitor.OnChannelsSet += Monitor_OnChannelsSet;

                Monitor.Start();
                await API.Helix.Channels.CheckCredentialsAsync();

            } catch (Exception e) {
                Console.WriteLine(e);
            }
            
            await Task.Delay(-1);
        }

        private void Monitor_OnChannelsSet(object sender, OnChannelsSetArgs e) {
            Logging.Logging.Info("Channel list has been set!");
            Logging.Logging.Info("  > Channles: " + string.Join(", ", e.Channels));
        }

        private void Monitor_OnServiceStarted(object sender, OnServiceStartedArgs e) {
            Logging.Logging.Info("Service has started!");
        }

        private void Monitor_OnStreamUpdate(object sender, OnStreamUpdateArgs e) {
            // What gets updated here?
            Logging.Logging.Info("Update: " + e.Channel);
        }

        private void Monitor_OnStreamOffline(object sender, OnStreamOfflineArgs e) {
            Logging.Logging.Info("Offline: " + e.Channel);
        }

        private void Monitor_OnStreamOnline(object sender, OnStreamOnlineArgs e) {
            Logging.Logging.Info("Online: " + e.Channel);
        }
    }
}
