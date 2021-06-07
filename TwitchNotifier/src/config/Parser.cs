using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Newtonsoft.Json.Serialization;
using System;
using YamlDotNet.Serialization;
using YamlDotNet.Serialization.NamingConventions;
using TwitchNotifier.src.Placeholders;
using TwitchNotifier.src.Helper;
using System.Linq;

namespace TwitchNotifier.src.config {
    class Parser {
        public new const string Equals = "==";
        public const string NotEquals = "!=";
        public const string GreaterEquals = ">=";
        public const string LessEquals = "<=";
        public const string GreaterThan = ">";
        public const string LessThan = "<";
        public const string Contains = ".Contains";
        public const string True = "true";
        public const string False = "false";


        /// <summary>
        /// Serialize an object to a yaml string
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
                parsedJson["timestamp"].Remove();
            }

            return parsedJson.ToString();
        }


        /// <summary>
        /// Returns the boolean of a condition string
        /// </summary>
        /// <param name="condition">The string which contains the condition</param>
        /// <returns>Default: <c>false</c> if not matched</returns>
        public static bool CheckEventCondition(string condition) {
            var returnValue = false;
            string[] seperatedCondition = null;
            var matchedLogicalCondition = string.Empty;
            var listOfSeperators = new[] {
                Equals,
                NotEquals,
                GreaterEquals,
                LessEquals,
                GreaterThan,
                LessThan,
                Contains,
                True,
                False
            };

            if (!string.IsNullOrEmpty(condition)) {
                // Check if condition contains one of the strings before trying to split it
                if (listOfSeperators.Any(x => condition.ToLower().Contains(x.ToLower()))) {
                    foreach (var logicalCondition in listOfSeperators) {
                        seperatedCondition = condition.Split(logicalCondition, StringSplitOptions.RemoveEmptyEntries).Select(x => x.Trim()).ToArray();

                        if (seperatedCondition.Length == 2) {
                            matchedLogicalCondition = logicalCondition;
                            break;
                        } else {
                            if (condition.ToLower() == logicalCondition) {
                                matchedLogicalCondition = logicalCondition;
                            }
                        }
                    }
                }


                if (!string.IsNullOrEmpty(matchedLogicalCondition)) {
                    int int1, int2;
                    switch (matchedLogicalCondition) {
                        case Equals:
                            returnValue = seperatedCondition[0] == seperatedCondition[1];
                            break;
                        case NotEquals:
                            returnValue = seperatedCondition[0] != seperatedCondition[1];
                            break;
                        case GreaterEquals:
                            if (int.TryParse(seperatedCondition[0], out int1) && int.TryParse(seperatedCondition[0], out int2)) {
                                returnValue = int1 >= int2;
                            }

                            break;
                        case LessEquals:
                            if (int.TryParse(seperatedCondition[0], out int1) && int.TryParse(seperatedCondition[0], out int2)) {
                                returnValue = int1 <= int2;
                            }

                            break;
                        case GreaterThan:
                            if (int.TryParse(seperatedCondition[0], out int1) && int.TryParse(seperatedCondition[0], out int2)) {
                                returnValue = int1 > int2;
                            }

                            break;
                        case LessThan:
                            if (int.TryParse(seperatedCondition[0], out int1) && int.TryParse(seperatedCondition[0], out int2)) {
                                returnValue = int1 < int2;
                            }

                            break;

                        case Contains:
                            var match = System.Text.RegularExpressions.Regex.Match(seperatedCondition[1], @"\((.*)\)");
                            if (match.Success) {
                                returnValue = seperatedCondition[0].Contains(match.Groups[1].Value);
                            }
                            break;

                        case True:
                            returnValue = true;
                            break;

                        case False:
                            returnValue = false;
                            break;
                    }
                }
            } else {
                returnValue = true;
            }

            return returnValue;
        }
    }
}
