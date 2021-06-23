using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Newtonsoft.Json.Serialization;
using System;
using YamlDotNet.Serialization;
using YamlDotNet.Serialization.NamingConventions;
using TwitchNotifier.src.Placeholders;
using TwitchNotifier.src.Helper;
using System.Linq;
using System.Collections.Generic;
using System.Text.RegularExpressions;

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
        private static string[] arrayOfSeperators = new[] {
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

        // Enum for logical operators "&&" and "||"
        public enum LogicalOperator {
            And = 0,
            Or = 1
        }

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
                parsedJson.Remove("timestamp");
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

            if (!string.IsNullOrEmpty(condition)) {
                // Check if condition contains one of the strings before trying to split it
                if (arrayOfSeperators.Any(x => condition.ToLower().Contains(x.ToLower()))) {
                    foreach (var logicalCondition in arrayOfSeperators) {
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

        /// <summary>
        /// Get nested parentheses from string
        /// </summary>
        /// <param name="value">The string which contains the condition / parentheses</param>
        /// <returns></returns>
        private static IEnumerable<string> GetNestedParentheses(string value) {
            Stack<int> brackets = new Stack<int>();
            bool containsOpened = false;

            if (string.IsNullOrEmpty(value)) {
                yield break;
            }

            for (int i = 0; i < value.Length; ++i) {
                char currentChar = value[i];

                if (i > 8 && string.Join("", value.Skip(i - 9).Take(9)) == Contains) {
                    containsOpened = true;
                }

                if (currentChar == '(' && !containsOpened) {
                    brackets.Push(i);
                } else if (currentChar == ')') {
                    if (containsOpened) {
                        containsOpened = false;
                    } else {
                        int openBracket = brackets.Pop();

                        yield return value.Substring(openBracket + 1, i - openBracket - 1);
                    }
                }
            }

            yield return value;
        }

        /// <summary>
        /// Get boolean of a condition (containing parentheses)
        /// </summary>
        /// <param name="condition">The condition to check</param>
        /// <returns></returns>
        internal static bool GetBooleanOfParenthesesCondition(string condition) {
            var tempEntry = condition;
            var openingParenthesesCount = tempEntry.Where(x => x == '(').Count();
            var closingParenthesesCount = tempEntry.Where(x => x == ')').Count();

            if ((tempEntry.Substring(0, 1) != "(" || tempEntry.Substring(tempEntry.Length - 1, 1) != ")") && (openingParenthesesCount == closingParenthesesCount)) {
                tempEntry = "(" + condition + ")";
            } else if (tempEntry.Where(x => x == '(').Count() != tempEntry.Where(x => x == ')').Count()) {
                // Not every parentheses has been closed / opened
            }

            var nestedElements = GetNestedParentheses(tempEntry);
            var count = nestedElements.Count();
            var lastElement = nestedElements.Last();
            var result = string.Empty;
            var lastCondition = string.Empty;
            var nextLogicalOperator = string.Empty;
            var lastLogicalOperator = string.Empty;

            for (int i = 0; i < count; i++) {
                var currentElement = nestedElements.ElementAt(i);
                var leftOver = lastElement.Replace(currentElement, "").Replace("()", "");

                if (currentElement == lastElement) {
                    break;
                }

                var match = Regex.Match(leftOver, @"&&|\|\|");
                nextLogicalOperator = match.Success ? match.Groups[0].Value : nextLogicalOperator;
                match = Regex.Match(currentElement, @"\((" + Regex.Escape(lastCondition) + @")\)");
                var lastLogicalOperatorEnum = lastLogicalOperator == "&&" ? LogicalOperator.And : LogicalOperator.Or;
                var nextLogicalOperatorEnum = nextLogicalOperator == "&&" ? LogicalOperator.And : LogicalOperator.Or;

                if (match.Success) {
                    var leftOverBeforeAppending = Regex.Replace(Regex.Replace(currentElement, Regex.Escape(lastCondition), "").Replace("()", ""), @"(&&|\|\|)\s?", "").Trim();
                    var boolOfLeftOver = CheckEventCondition(leftOverBeforeAppending);
                    var replacedLastCondition = currentElement.Replace(lastCondition, "").Replace("()", "").Trim();

                    if (replacedLastCondition.Split("&&").Length > 1 || replacedLastCondition.Split("||").Length > 1) {
                        var tempLogicalOperator = replacedLastCondition.Replace(leftOverBeforeAppending, "").Trim();
                        lastLogicalOperator = string.IsNullOrEmpty(tempLogicalOperator) || (tempLogicalOperator != "&&" && tempLogicalOperator != "||") ? lastLogicalOperator : tempLogicalOperator;
                        result = result.Length > 0 ? CalculateAndOr(result + lastLogicalOperator + boolOfLeftOver, lastLogicalOperatorEnum).ToString() : boolOfLeftOver.ToString();
                    } else {
                        result = result.Length > 0 ? CalculateAndOr(result + nextLogicalOperator + boolOfLeftOver, nextLogicalOperatorEnum).ToString() : boolOfLeftOver.ToString();
                    }
                } else {
                    var currentElementBoolean = CalculateAndOr(currentElement);

                    if (currentElement.Split("&&").Length > 1 || currentElement.Split("||").Length > 1) {
                        result = result.Length > 0 ? CalculateAndOr(result + lastLogicalOperator + currentElementBoolean, lastLogicalOperatorEnum).ToString() : currentElementBoolean.ToString();
                    } else {
                        result = result.Length > 0 ? CalculateAndOr(result + nextLogicalOperator + currentElementBoolean, nextLogicalOperatorEnum).ToString() : currentElementBoolean.ToString();
                    }
                }

                lastLogicalOperator = nextLogicalOperator;
                lastCondition = currentElement;
            }

            bool.TryParse(result, out bool returnValue);
            return returnValue;
        }

        /// <summary>
        /// Get the boolean of a condition (containing logical operators)
        /// </summary>
        /// <param name="condition"></param>
        /// <param name="splitWithOperator"></param>
        /// <returns></returns>
        private static bool CalculateAndOr(string condition, LogicalOperator splitWithOperator = 0) {
            var returnValue = true;
            var oppositeOpertor = splitWithOperator == LogicalOperator.And ? LogicalOperator.Or : LogicalOperator.And;
            var andCondtions = condition.Split(splitWithOperator == 0 ? "&&" : "||");
            var lastBoolean = true;

            foreach (var splittedCondition in andCondtions.Select(x => x.Trim())) {
                if (Regex.Match(splittedCondition, @"\.Contains\(.*\)").Success || IsConditionalString(condition)) {
                    lastBoolean = CheckEventCondition(splittedCondition);
                } else if (splittedCondition == True || splittedCondition == False) {
                    bool.TryParse(splittedCondition, out lastBoolean);
                } else {
                    lastBoolean = false;
                }
                //} else {
                //    lastBoolean = CalculateAndOr(splittedCondition, oppositeOpertor);
                //}

                if (splitWithOperator == LogicalOperator.And) {
                    returnValue = returnValue && lastBoolean;
                } else {
                    returnValue = returnValue || lastBoolean;
                }
            }

            return returnValue;
        }

        /// <summary>
        /// Check if string is a condition (eg. contains == or !=)
        /// </summary>
        /// <param name="condition">The condition to check</param>
        /// <returns></returns>
        private static bool IsConditionalString(string condition) {
            var returnValue = arrayOfSeperators.Any(x => condition.ToLower().Contains(x.ToLower()));
            return returnValue;
        }
    }
}