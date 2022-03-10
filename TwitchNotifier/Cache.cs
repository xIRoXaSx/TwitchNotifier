using System.Runtime.Caching;
using TwitchNotifier.models;

namespace TwitchNotifier {
    internal class Cache {
        /// <summary>
        /// Adds a CacheEntry to the MemoryCache.
        /// </summary>
        /// <param name="entry">The CacheEntry to add to the MemoryCache.</param>
        internal static void AddEntry(CacheEntry entry) {
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
            var cacheValue = (CacheEntry)MemoryCache.Default[entry.Key];
            return cacheValue == null;
        }
        
        /// <summary>
        /// Hash the key of an CacheEntry.
        /// </summary>
        /// <param name="entry">The CacheEntry for which the key should be hashed.</param>
        /// <returns><c>CacheEntry</c> - The CacheEntry.</returns>
        internal static CacheEntry HashCacheEntryKey(CacheEntry entry) {
            if (!entry.Sha256HashKey)
                return entry;
            entry.Key = entry.Key.Create256Sha();
            return entry;
        }
    }
}