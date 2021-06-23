using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using TwitchNotifier.src.ErrorHandling;
using YamlDotNet.Serialization;

namespace TwitchNotifier.src.config {
    /// <summary>
    /// The configuration of this program
    /// </summary>
    public class Config {
        public TwitchNotifierSettings TwitchNotifier { get; set; } = new TwitchNotifierSettings();
        public TwitchSettings Settings { get; set; } = new TwitchSettings();

        /// <summary>
        /// The local AppData directory
        /// </summary>
        private static string appdataDirectory = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);

        /// <summary>
        /// The full path of the directory in which the config lays
        /// </summary>
        public static string configLocation = appdataDirectory + Path.DirectorySeparatorChar + Assembly.GetExecutingAssembly().GetName().Name;

        /// <summary>
        /// The full path of the config file
        /// </summary>
        public static string configFileLocation = configLocation + Path.DirectorySeparatorChar + "config.yml";

        /// <summary>
        /// Create a new config file and its directory if it does not exist!
        /// </summary>
        /// <returns><code>true</code> if config got created. <code>false</code> if config already exists</returns>
        public bool CreateConfig() {
            var returnValue = false;
            var newConfig = new Config();

            // Overwrite properties if they are not set to an instance
            newConfig.TwitchNotifier.OnFollow.StreamerOption1.Discord.Embed = new Embed();
            newConfig.TwitchNotifier.OnStreamOnline.StreamerOption1.Discord.Embed = new Embed();
            newConfig.TwitchNotifier.OnStreamOffline.StreamerOption1.Discord.Embed = new Embed();

            newConfig.TwitchNotifier.OnFollow.StreamerOption1.Discord.Embed.Image = new Image();
            newConfig.TwitchNotifier.OnStreamOnline.StreamerOption1.Discord.Embed.Image = new Image();
            newConfig.TwitchNotifier.OnStreamOffline.StreamerOption1.Discord.Embed.Image = new Image();

            newConfig.TwitchNotifier.OnFollow.StreamerOption1.Discord.Embed.Thumbnail = new Thumbnail();
            newConfig.TwitchNotifier.OnStreamOnline.StreamerOption1.Discord.Embed.Thumbnail = new Thumbnail();
            newConfig.TwitchNotifier.OnStreamOffline.StreamerOption1.Discord.Embed.Thumbnail = new Thumbnail();

            var config = Parser.Serialize(newConfig);

            if (!Directory.Exists(configLocation)) {
                try {
                    Directory.CreateDirectory(configLocation);
                    Logging.Log.Info("Config directory \"" + configLocation + "\" has been created!");
                    returnValue = true;
                } catch (Exception e) {
                    new Error() {
                        IsTerminating = true,
                        Message = "Cannot create config folder \"" + configLocation + "\"",
                        Exception = e
                    }.WriteError();
                }

                Logging.Log.Info("Press any key to exit the application...");
                Console.ReadKey();
            }

            if (!File.Exists(configFileLocation)) {
                File.WriteAllText(configFileLocation, config);
                Logging.Log.Info("Config file has been written to \"" + configFileLocation + "\"");
                returnValue = true;
            }

            return returnValue;
        }

        /// <summary>
        /// Returns all EventObjects by the event name of a twitch channel
        /// </summary>
        /// <param name="eventName">The name of the event</param>
        /// <param name="username">The username / channel name</param>
        /// <returns>A Dictionary of EventObjects in form of <b>Dictionary&lt;string, object&gt;</b></returns>
        public static Dictionary<string, object> GetEventObjectsByTwitchChannelName(string eventName, string username) {
            Dictionary<string, object> returnValue = new Dictionary<string, object>();
            var deserializer = new DeserializerBuilder().IgnoreUnmatchedProperties().Build();
            var config = deserializer.Deserialize<dynamic>(File.ReadAllText(configFileLocation, Encoding.UTF8));
            var eventNodes = config["TwitchNotifier"][eventName];

            foreach (var listElement in eventNodes) {
                // The matched usernames from one eventnode (eg.: "StreamerOption1")
                var userNamesMatched = ((List<object>)listElement.Value["Twitch"]["Usernames"]).Where(x => x.ToString() == username).ToList();

                if (userNamesMatched.Count > 0) {
                    var a = listElement.Value;
                    returnValue.Add(listElement.Key.ToString(), listElement.Value);
                }
            }

            return returnValue;
        }
    }
}