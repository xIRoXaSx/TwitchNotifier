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

            if (Directory.Exists(configLocation)) {
                // Console.WriteLine("Config directory \"" + configLocation + "\" already exists");
            } else {
                try {
                    Directory.CreateDirectory(configLocation);
                    Console.WriteLine("Config directory \"" + configLocation + "\" has been created!");
                    returnValue = true;
                } catch (Exception e) {
                    new Error() {
                        IsTerminating = true,
                        Message = "Cannot create config folder \"" + configLocation + "\"",
                        Exception = e
                    }.WriteError();
                }
            }

            if (!File.Exists(configFileLocation)) {
                File.WriteAllText(configFileLocation, config);
                Console.WriteLine("Config file has been written to \"" + configFileLocation + "\"");
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
            var deserializer = new DeserializerBuilder().Build();
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


        public TwitchNotifierSettings TwitchNotifier { get; set; } = new TwitchNotifierSettings();
        public TwitchSettings Settings { get; set; } = new TwitchSettings();
    }


    /// <summary>
    /// Contains the complete configuration
    /// </summary>
    public class TwitchNotifierSettings {
        public Event OnStreamOnline { get; set; } = new Event();
        public Event OnStreamOffline { get; set; } = new Event();
        public Event OnFollow { get; set; } = new Event();
    }


    /// <summary>
    /// Contains events from Twitch
    /// </summary>
    public class Event {
        public EventObject StreamerOption1 { get; set; } = new EventObject();
    }


    /// <summary>
    /// Contains events from Twitch
    /// </summary>
    public class EventObject {
        public TwitchListenerSettings Twitch { get; set; } = new TwitchListenerSettings();
        public DiscordEmbed Discord { get; set; } = new DiscordEmbed();
        public string WebHookUrl { get; set; } = "The Discord Webhook URL";
    }


    /// <summary>
    /// Settings for the Twitch channels which are creating events
    /// </summary>
    public class TwitchListenerSettings {
        public List<string> Usernames { get; set; } = new List<string>() { "Channelnames" };
    }


    /// <summary>
    /// Settings for the Twitch channel which are creating events
    /// </summary>
    public class TwitchSettings {
        public string ClientID { get; set; } = "The Client ID of your Twitch app (developer portal)";
        public string AccessToken { get; set; } = "Your App Access Token";
    }


    /// <summary>
    /// The thumbnail (upper right corner of embed)
    /// </summary>
    public class Thumbnail {
        public string Url = "%Channel.User.Logo%";
    }


    /// <summary>
    /// The thumbnail (upper right corner of embed)
    /// </summary>
    public class Image {
        public string Url = "%Stream.ThumbnailUrl%";
    }


    /// <summary>
    /// The author of the embed
    /// </summary>
    public class EmbedAuthor {
        public string Name { get; set; } = "Stream Announcer 📢";
        public string IconUrl { get; set; } = "%Channel.User.Logo%";
        public string Url { get; set; } = "%Channel.User.Url%";
    }


    /// <summary>
    /// The fields for the embed (max = 25)
    /// </summary>
    public class EmbedField {
        public string Name { get; set; } = "Name of the field (max 256 chars)";
        public string Value { get; set; } = "Value of the field (max 1024 chars)";
        public bool Inline { get; set; } = false;
    }


    /// <summary>
    /// The footer of the embed
    /// </summary>
    public class EmbedFooter {
        public string Text { get; set; } = "The footer text (max 2048 chars)";
        public string IconUrl { get; set; } = "%Channel.User.Logo%";
    }


    /// <summary>
    /// The webhook user that sends the embed (to overwrite the existing data from Discord)
    /// </summary>
    public class DiscordEmbed {
        public string Username { get; set; } = "%Channel.Name%";
        public string AvatarUrl { get; set; } = "%Channel.User.Url%";
        public Embed Embed { get; set; } = new Embed();
    }


    /// <summary>
    /// Settings for the Discord embed
    /// </summary>
    public class Embed {
        public string Title { get; set; } = "%Channel.Name% went online!";
        public string Url { get; set; } = "%Channel.User.Url%";
        public string Description { get; set; } = "What are you waiting for?!\\nGo check it out now!";
        public string Color { get; set; } = "#5555FF";
        public bool Timestamp { get; set; } = true;
        public Thumbnail Thumbnail { get; set; } //= new Thumbnail();
        public Image Image { get; set; } //= new Image();
        public EmbedAuthor Author { get; set; } = new EmbedAuthor();
        public List<EmbedField> Fields { get; set; } = new List<EmbedField>() {
            new EmbedField() {
                Name = "Unique Field Name 1",
                Value = "Value of field 1",
                Inline = false
            },
            new EmbedField() {
                Name = "Unique Field Name 2",
                Value = "Value of field 2",
                Inline = false
            },
        };
        public EmbedFooter Footer { get; set; } = new EmbedFooter();
    }
}