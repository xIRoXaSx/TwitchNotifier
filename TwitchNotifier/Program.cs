using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using TwitchNotifier.twitch;

namespace TwitchNotifier {
    internal class Program {
        internal static readonly string BinaryName = Assembly.GetExecutingAssembly().GetName().Name;
        internal static Config Conf { get; }
        internal static Core TwitchCore { get; set; }
        private CancellationTokenSource _cancelSource = new();
        private bool _disposeSubscribed;

        static Program() {
            Conf = new Config();
            Conf.CreateIfNotExisting();
            Conf.Load();
            if (Conf.GeneralSettings.EnableHotLoad)
                Conf.SetFileWatcher();
        }
        
        private static void Main(string[] args) {
            Task.Run(() => new Program().MainAsync()).GetAwaiter().GetResult();
        }

        private async Task MainAsync() {
            // Create a new TwitchCore instance and validate the data for it.
            TwitchCore = new Core();
            await TwitchCore.ValidateOrThrowAsync();
            if (!_disposeSubscribed) {
                TwitchCore.DisposeRequested += (_, _) => TwitchCoreOnDisposeRequested();
                _disposeSubscribed = true;
            }
            
            // Start the stream monitor.
            TwitchCore.StreamMonitor.Start();
            
            // Instantiate and start the clip monitor.
            var channels = new List<string>();
            foreach (var clipEvent in Conf.NotificationSettings.OnClipCreated) {
                channels.AddRange(clipEvent.Channels);
            }

            channels = channels.Distinct().ToList();
            var channelIds = await TwitchCore.TwitchApi.Helix.Users.GetUsersAsync(logins: channels);
            TwitchCore.ClipMonitor.SetChannelIds(channelIds.Users.Select(x => x.Id));
            Logging.Info("Starting clip monitor");
            Logging.Debug($"\t> Channel(s) to monitor clips: {string.Join(", ", channels)}");
            TwitchCore.ClipMonitor.Start();

            // Keep alive.
            Logging.Info("Setup finished.");
            try {
                await Task.Delay(-1, _cancelSource.Token);
            } catch (TaskCanceledException) {}
            _cancelSource.Dispose();
            _cancelSource = new CancellationTokenSource();
            await MainAsync();
        }

        /// <summary>
        /// Stops and disposes all instances related to the core and creates a new instance of it.
        /// </summary>
        private void TwitchCoreOnDisposeRequested() {
            _cancelSource.Cancel();
            _cancelSource.Dispose();
            _cancelSource = new CancellationTokenSource();
            TwitchCore.StreamMonitor.Stop();
            TwitchCore.ClipMonitor.Stop();
        }
    }
}