# nullable enable
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using TwitchLib.Api.Services;
using TwitchLib.Api.Services.Events;
using TwitchLib.Api.Services.Events.LiveStreamMonitor;
using TwitchNotifier.models;
using TwitchNotifier.placeholders;

namespace TwitchNotifier.twitch {
    internal class StreamMonitor {
        private readonly LiveStreamMonitorService? _monitorService;
        private readonly CancellationTokenSource _cancelSource = new();
        
        internal StreamMonitor() {
            var core = Program.TwitchCore;
            if (!core.IsValid)
                return;
            
            _cancelSource = new CancellationTokenSource();
            _monitorService = new LiveStreamMonitorService(
                core.TwitchApi, Program.Conf.GeneralSettings.LiveCheckIntervalInSeconds
            );

            _monitorService.OnServiceStarted += OnServiceStated;
            _monitorService.OnServiceStopped += OnServiceStopped;
            _monitorService.OnChannelsSet += OnChannelsSet;
            _monitorService.OnStreamOnline += OnStreamOnline;
            _monitorService.OnStreamOffline += OnStreamOffline;
        }

        /// <summary>
        /// Starts the stream monitoring.
        /// </summary>
        internal async void Start() {
            if (_monitorService == null)
                return;
            
            // Set channels of interest.
            _monitorService.SetChannelsByName(
                Program.Conf.GetMonitoredChannels().Distinct(StringComparer.CurrentCultureIgnoreCase).ToList()
            );
            
            // Start the monitor.
            _monitorService.Start();

            // Keep alive.
            try {
                await Task.Delay(-1, _cancelSource.Token);
            } catch (TaskCanceledException ex) {
                Logging.Debug($"StreamMonitor: {ex.Message}");
            }
        }

        /// <summary>
        /// Stop the monitoring service.
        /// </summary>
        internal void Stop() {
            _cancelSource.Cancel();
            if (_monitorService?.Enabled ?? false)
                _monitorService?.Stop();
            _cancelSource.Dispose();
        }
        
        private static void OnServiceStated(object? sender, OnServiceStartedArgs e) {
            Logging.Info("Channel monitoring service started.");
        }

        private static void OnServiceStopped(object? sender, OnServiceStoppedArgs e) {
            Logging.Info("Channel monitoring service stopped.");
        }

        private static void OnChannelsSet(object? sender, OnChannelsSetArgs e) {
            Logging.Info("Channels to monitor have been set.");
            Logging.Debug($"\t> Channel(s) to monitor: {string.Join(", ", e.Channels)}");
        }

        private static async void OnStreamOnline(object? sender, OnStreamOnlineArgs e) {
            Logging.Debug($"{e.Channel} went live!");
            var entry = new CacheEntry {
                Key = e.Stream.UserId,
                ExpirationTime = DateTime.Now.AddSeconds(Program.Conf.GeneralSettings.LiveNotificationThresholdInSeconds)
            }.HashKey();
            
            // Check if the same channel had already pushed a notification.
            if (!Cache.IsCacheEntryExpired(entry)) {
                var eventName = e.GetType().Name.Replace("args", "", StringComparison.OrdinalIgnoreCase);
                Logging.Debug($"{eventName} triggered multiple times and is still in cooldown...");
                return;
            }
            
            Cache.AddEntry(entry);

            // Get the first embed which contains the channel.
            var notification = Program.Conf.NotificationSettings.OnLiveEvent
                .FirstOrDefault(x => x.Channels.Select(y => y.ToLower()).Any(y=> y == e.Channel.ToLower()));
            
            // Check if notification is null or invalid.
            if (notification == null)
                return;
            notification.Embed.Validate();
            if (notification.Embed == null) {
                Logging.Error("Embed validation returned null!");
                return;
            }
            
            // Get User object of streamer for placeholders.
            var users = await Program.TwitchCore.TwitchApi.Helix.Users.GetUsersAsync(new List<string>{e.Stream.UserId});
            var user = users.Users.Length > 0 ? users.Users[0] : null;
            var placeholder = new TwitchPlaceholder {
                Channel = new ChannelPlaceholder(user, e.Channel),
                Stream = e.Stream
            };

            var json = notification.Embed.ToJson(placeholder);
            await new Request(notification.WebHookUrl, json).SendAsync();
        }
        
        private static void OnStreamOffline(object? sender, OnStreamOfflineArgs e) {
            Logging.Debug($"{e.Channel} went offline!");
        }
    }
}