# nullable enable
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using TwitchLib.Api.Services;
using TwitchLib.Api.Services.Events;
using TwitchLib.Api.Services.Events.LiveStreamMonitor;

namespace TwitchNotifier.twitch {
    internal class StreamMonitor {
        private readonly LiveStreamMonitorService? _monitorService;
        private CancellationTokenSource _cancellation = new();
        
        internal StreamMonitor() {
            var core = Program.TwitchCore;
            if (!core.IsValid)
                return;
            
            _cancellation = new CancellationTokenSource();
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
                Program.Conf.GetMonitoredChannels().Distinct(System.StringComparer.CurrentCultureIgnoreCase).ToList()
            );
            
            // Update live streams
            await _monitorService.UpdateLiveStreamersAsync();
            
            // Start the monitor.
            _monitorService.Start();

            // Keep alive.
            try {
                await Task.Delay(-1, _cancellation.Token);
            } catch (TaskCanceledException ex) {
                Logging.Debug(string.Concat("StreamMonitor: ", ex.Message));
            }
        }

        internal void Stop() {
            _cancellation.Cancel();
            if (_monitorService?.Enabled ?? false)
                _monitorService?.Stop();
        }
        
        private static void OnServiceStated(object? sender, OnServiceStartedArgs e) {
            Logging.Info("Channel monitoring service started.");
        }

        private void OnServiceStopped(object? sender, OnServiceStoppedArgs e) {
            Logging.Info("Channel monitoring service stopped.");
        }

        private static void OnChannelsSet(object? sender, OnChannelsSetArgs e) {
            Logging.Info("Channels to monitor have been set.");
            Logging.Debug(string.Concat("\t> Channel(s) to monitor: ", string.Join(", ", e.Channels)));
        }

        private static void OnStreamOnline(object? sender, OnStreamOnlineArgs e) {
            Logging.Debug(string.Concat(e.Channel, " is live!"));
        }
        
        private static void OnStreamOffline(object? sender, OnStreamOfflineArgs e) {
            Logging.Debug(string.Concat(e.Channel, " went offline!"));
        }
    }
}