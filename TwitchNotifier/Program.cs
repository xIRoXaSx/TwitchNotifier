using System.Reflection;
using System.Threading.Tasks;
using TwitchNotifier.twitch;

namespace TwitchNotifier {
     internal class Program {
        internal static readonly string BinaryName = Assembly.GetExecutingAssembly().GetName().Name;
        internal static Config Conf { get; private set; }
        internal static Core TwitchCore { get; private set; }
        private StreamMonitor _streamMonitor;

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

            // Keep alive.
            Logging.Info("Setup finished.");
            await Task.Delay(-1);
        }
    }
}