using System.Linq;

namespace TwitchNotifier.src {
    /// <summary>
    /// Extension methods for easier placeholder formatting
    /// </summary>
    public static class ExtensionMethods {
        public static string ToSnakeCase(this string str) {
            return string.Concat(str.Select((x, i) => i > 0 && char.IsUpper(x) && str[i - 1] != '.' ? "_" + x.ToString() : x.ToString())).ToLower();
        }
    }
}
