using System;
using System.Linq;
using TwitchNotifier.src.config;
using TwitchNotifier.src.Logging;
using TwitchNotifier.src.Twitch;
using TwitchNotifier.src.WebRequest;
using YamlDotNet.Serialization;

namespace TwitchNotifier {
    class Program {
        static void Main(string[] args) {
            var config = new Config();
           
            if (!config.CreateConfig()) {
                var channelOffline = Config.GetEventObjectsByTwitchChannelName("OnStreamOnline", "xIRoXaSx");

                if (channelOffline.Count > 0) {
                    foreach (var eventObject in channelOffline) {
                        var embed = Parser.Deserialize(typeof(Embed), ((dynamic)eventObject.Value)["Embed"]);
                        var serializer = new SerializerBuilder().Build();
                        var deserializer = new DeserializerBuilder().Build();

                        var webRequest = new WebRequest() {
                            webHookUrl = ((dynamic)eventObject.Value)["WebHookUrl"],
                            embed = embed
                        }.SendRequest();
                    }
                }

                //webRequest.
                new LiveMonitoring();
            }
        }
    }
}
