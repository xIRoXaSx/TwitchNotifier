using System.Linq;

namespace TwitchNotifier.src {
    /// <summary>
    /// Extension methods for easier placeholder formatting
    /// </summary>
    public static class ExtensionMethods {
        public static string ToSnakeCase(this string str) {
            return string.Concat(str.Select((x, i) => i > 0 && char.IsUpper(x) && str[i - 1] != '.' ? "_" + x.ToString() : x.ToString())).ToLower();
        }

        /// <summary>
        /// Truncate a string if it is longer thanm maxChars <br/>
        /// Also adds <c>...</c> at the end of the string if truncated
        /// </summary>
        /// <param name="value">The string to truncate</param>
        /// <param name="maxChars">The amount of chars after which the string should be truncated</param>
        /// <returns></returns>
        public static string Truncate(this string value, int maxChars) {
            return value.Length <= maxChars ? value : value.Substring(0, maxChars - 3) + "...";
        }
    }
}
