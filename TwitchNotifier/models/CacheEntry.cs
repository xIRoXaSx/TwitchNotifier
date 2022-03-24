using System;
using System.Runtime.Caching;

namespace TwitchNotifier.models {
    internal class CacheEntry {
        /// <summary>
        /// The key of the CacheEntry.
        /// </summary>
        internal string Key { get; set; }

        /// <summary>
        /// Determines whether the CacheEntry key should be hashed or not
        /// </summary>
        internal bool Sha256HashKey { get; init; } = true;

        /// <summary>
        /// The DateTime when the CacheEntry should be counted as expired.
        /// </summary>
        internal DateTime ExpirationTime { get; init; } = DateTime.Now.AddSeconds(30);

        /// <summary>
        /// The CacheItemPriority which indicates whether an item should be stored indefinitely or not.
        /// </summary>
        internal CacheItemPriority Priority { get; } = CacheItemPriority.Default;

        /// <summary>
        /// Hash the key of an CacheEntry.
        /// </summary>
        /// <returns><c>CacheEntry</c> - The CacheEntry.</returns>
        internal CacheEntry HashKey() {
            if (!Sha256HashKey)
                return this;
            Key = Key.Create256Sha();
            return this;
        }
    }
}