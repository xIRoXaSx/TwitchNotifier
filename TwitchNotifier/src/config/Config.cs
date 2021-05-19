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
        private static string configLocation = appdataDirectory + Path.DirectorySeparatorChar + System.Reflection.Assembly.GetExecutingAssembly().GetName().Name;

        /// <summary>
        /// The full path of the config file
        /// </summary>
        public static string configFileLocation = configLocation + Path.DirectorySeparatorChar + "config.yml";


        /// <summary>
        /// Create a new config file and its directory if it does not exist!
        /// </summary>
        public void CreateConfig() {
            var config = Parser.Serialize(this);

            if (Directory.Exists(configLocation)) {
                // Console.WriteLine("Config directory \"" + configLocation + "\" already exists");
            } else {
                try {
                    Directory.CreateDirectory(configLocation);
                    Console.WriteLine("Config directory \"" + configLocation + "\" has been created!");
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
            }
        }


        public static EventObject GetEventObjectByTwitchChannelName(string eventName, string username) {
            EventObject returnValue = null;
            var deserializer = new DeserializerBuilder().Build();
            //var config = deserializer.Deserialize<Config>(File.ReadAllText(configFileLocation, Encoding.UTF8));
            var config = deserializer.Deserialize<dynamic>(File.ReadAllText(configFileLocation, Encoding.UTF8));
            var eventNodes = (Event)config["TwitchNotifier"][eventName];
            
            // eventNode is the key node for all settings below each event
            foreach (var eventNode in typeof(Event).GetProperties()) {
                var eventNodeObjects = ((EventObject)eventNode.GetValue(eventNodes)).Twitch.Usernames;
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
        public Event OnStreamStart { get; set; } = new Event();
        public Event OnStreamEnd { get; set; } = new Event();
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
        public Embed Embed { get; set; } = new Embed();
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
    /// The author of the embed
    /// </summary>
    public class EmbedAuthor {
        public string Name { get; set; } = "The author's name for this message";
        public string IconUrl { get; set; } = "The URL of the author's image";
        public string Url { get; set; } = "The URL when the author's name is clicked";
    }


    /// <summary>
    /// The fields for the embed (max = 25)
    /// </summary>
    public class EmbedFields {
        public string Name { get; set; } = "Name of the field (max 256 chars)";
        public string Value { get; set; } = "Value of the field (max 1024 chars)";
        public bool Inline { get; set; } = false;
    }


    /// <summary>
    /// The footer of the embed
    /// </summary>
    public class EmbedFooter {
        public string Text { get; set; } = "The footer text (max 2048 chars)";
        public string IconUrl { get; set; } = "The URL of the footer's image";
    }


    /// <summary>
    /// Settings for the Discord embed
    /// </summary>
    public class Embed {
        public string Title { get; set; } = "Something has happened at %Username%'s channel!";
        public string Url { get; set; } = "The title's URL which can be clicked";
        public string Description { get; set; } = "The embeds text / description";
        public string Color { get; set; } = "The embeds hex color (like #5555FF)";
        public bool Timestamp { get; set; } = true;
        public EmbedAuthor Author { get; set; } = new EmbedAuthor();
        public EmbedFields Fields { get; set; } = new EmbedFields();
        public EmbedFooter Footer { get; set; } = new EmbedFooter();
    }
}