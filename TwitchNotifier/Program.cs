using System;
using System.Reflection;

namespace TwitchNotifier {
     class Program {
        internal static readonly string BinaryName = Assembly.GetExecutingAssembly().GetName().Name;
        internal static Config Conf { get; private set; }

        static Program() {
            Conf = new Config();
            Conf.CreateIfNotExisting();
            Conf.Load();
        }
        
        private static void Main(string[] args) {
            Console.WriteLine(Conf.GeneralSettings.ClientId);
        }
    }
}