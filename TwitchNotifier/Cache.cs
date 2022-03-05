using System.Runtime.Caching;
using System.Security.Cryptography;
using System.Text;
using TwitchNotifier.models;

namespace TwitchNotifier {
    public class Cache {
        /// <summary>
        /// Adds a CacheEntry to the MemoryCache.
        /// </summary>
        /// <param name="entry">The CacheEntry to add to the MemoryCache.</param>
        internal static void AddCacheEntry(CacheEntry entry) {
            var policy = new CacheItemPolicy {
                AbsoluteExpiration = entry.Priority == CacheItemPriority.NotRemovable ? 
                    ObjectCache.InfiniteAbsoluteExpiration : entry.ExpirationTime,
                Priority = entry.Priority
            };

            MemoryCache.Default.Set(entry.Key, entry, policy);
        }
        
        /// <summary>
        /// Whether the specified CacheEntry has been expired or not.
        /// </summary>
        /// <param name="entry">The CacheEntry to check.</param>
        /// <returns>
        /// <c>True</c> - If cache contains the provided entry.<br/>
        /// <c>False</c> - If cache does not contain the provided entry.
        /// </returns>
        internal static bool IsCacheEntryExpired(CacheEntry entry) {
            entry = HashCacheEntryKey(entry);
            var cacheValue = (CacheEntry)MemoryCache.Default[entry.Key];
            return cacheValue != null;
        }
        
        /// <summary>
        /// Hash the key of an CacheEntry.
        /// </summary>
        /// <param name="entry">The CacheEntry for which the key should be hashed.</param>
        /// <returns><c>CacheEntry</c> - The CacheEntry.</returns>
        private static CacheEntry HashCacheEntryKey(CacheEntry entry) {
            if (!entry.Sha256HashKey)
                return entry;
            entry.Key = HashString(entry.Key);
            return entry;
        }
        
        /// <summary>
        /// Hash the given value.
        /// </summary>
        /// <param name="value">The string that should be hashed.</param>
        /// <returns><c>string</c> - The hashed string.</returns>
        private static string HashString(string value) {
            using var sha256 = SHA256.Create();
            var chars = sha256.ComputeHash(Encoding.UTF8.GetBytes(value));
            var stringBuilder = new StringBuilder();
            foreach (var c in chars) {
                stringBuilder.Append(c.ToString("x2"));
            }
            return stringBuilder.ToString();
        }
    }
}