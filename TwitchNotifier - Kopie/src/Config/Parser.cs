using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Newtonsoft.Json.Serialization;
using System;
using YamlDotNet.Serialization;
using YamlDotNet.Serialization.NamingConventions;
using TwitchNotifier.src;
using TwitchNotifier.src.Helper;

namespace TwitchNotifier.src.config {
    class Parser {

        /// <summary>
        /// Serialize an object to a string
        /// </summary>
        /// <param name="objectToSerialze">The object to serialze</param>
        public static string Serialize(object objectToSerialze) {
            var serializer = new SerializerBuilder()
                .WithNamingConvention(PascalCaseNamingConvention.Instance)
                .Build();

            return serializer.Serialize(objectToSerialze);
        }


        /// <summary>
        /// Serializes an object to deserialize it into the specified type
        /// </summary>
        /// <param name="type">The type to deserialize the object to</param>
        /// <param name="objectToDeserialize">The object which should be deserialized</param>
        /// <returns>An object that can be easily casted to the desired type</returns>
        public static object Deserialize(Type type, object objectToDeserialize) {
            var serializer = new SerializerBuilder().Build();
            var deserializer = new DeserializerBuilder().Build();

            var yml = serializer.Serialize((dynamic)objectToDeserialize);
            return deserializer.Deserialize(yml, type);
        }


        /// <summary>
        /// Serializes an object to deserialize it into the specified type<br/>
        /// EventArgs contains the information for placeholders<br/>
        /// <c>Todo</c>: Make deserializer ignore empty properties from objectToDeserialize
        /// </summary>
        /// <param name="type">The type to deserialize the object to</param>
        /// <param name="objectToDeserialize">The object which should be deserialized</param>
        /// <param name="placeholderHelper">The PlaceholderHelper which contain the information to replace placeholders</param>
        /// <returns>An object that can be easily casted to the desired type</returns>
        public static object Deserialize(Type type, object objectToDeserialize, PlaceholderHelper placeholderHelper) {
            var serializer = new SerializerBuilder().ConfigureDefaultValuesHandling(DefaultValuesHandling.OmitDefaults & DefaultValuesHandling.OmitNull).Build();
            var deserializer = new DeserializerBuilder().IgnoreUnmatchedProperties().Build();

            var yml = serializer.Serialize((dynamic)objectToDeserialize);
            yml = new Placeholder().ReplacePlaceholders(yml, placeholderHelper);
            return deserializer.Deserialize(yml, type);
        }


        /// <summary>
        /// Get a json string from the passed embed
        /// </summary>
        /// <param name="embed">The embed that should be deserialized to a json string</param>
        /// <returns>Json string of the embed</returns>
        public static string GetEmbedJson(Embed embed) {
            var returnValue = string.Empty;

            var jsonSerializerSettings = new JsonSerializerSettings() {
                NullValueHandling = NullValueHandling.Ignore,
                ContractResolver = new DefaultContractResolver {
                    NamingStrategy = new SnakeCaseNamingStrategy()
                }
            };

            var json = JsonConvert.SerializeObject(embed, jsonSerializerSettings);
            var parsedJson = JObject.Parse(json);

            if (parsedJson["timestamp"].ToString().ToLower() == "true") {
                parsedJson["timestamp"] = DateTime.Now;
            } else {
                parsedJson.Property("timestamp").Remove();
            }

            return parsedJson.ToString();
        }
    }
}
