using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using TwitchNotifier.twitch;

namespace TwitchNotifier {
    internal class Program {
        internal static readonly string BinaryName = Assembly.GetExecutingAssembly().GetName().Name;
        internal static Config Conf { get; private set; }
        internal static Core TwitchCore { get; private set; }
        private StreamMonitor _streamMonitor;
        private ClipMonitor _clipMonitor;

        static Program() {
            Conf = new Config();
            Conf.CreateIfNotExisting();
            Conf.Load();
        }
        
        private static void Main(string[] args) {
            Task.Run(() => new Program().MainAsync()).GetAwaiter().GetResult();
        }

        private async Task MainAsync() {
            // Create and validate data for TwitchCore.
            TwitchCore = new Core();
            await TwitchCore.ValidateOrThrowAsync();
            
            // Instantiate and start the stream monitor.
            _streamMonitor = new StreamMonitor();
            _streamMonitor.Start();
            
            // Instantiate and start the clip monitor.
            var channels = new List<string>();
            foreach (var clipEvent in Conf.NotificationSettings.OnClipCreated) {
                channels.AddRange(clipEvent.Channels);
            }

            channels = channels.Distinct().ToList();
            var channelIds = await TwitchCore.TwitchApi.Helix.Users.GetUsersAsync(logins: channels);
            _clipMonitor = new ClipMonitor(channelIds.Users.Select(x => x.Id));
            Logging.Info("Starting clip monitor");
            Logging.Debug($"\t> Channel(s) to monitor clips: {string.Join(", ", channels)}");
            _clipMonitor.Start();

            // Keep alive.
            Logging.Info("Setup finished.");
            await Task.Delay(-1);
        }
    }
}