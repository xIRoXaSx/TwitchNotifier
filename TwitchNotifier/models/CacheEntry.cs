using System;
using System.Runtime.Caching;

namespace TwitchNotifier.models {
    internal class CacheEntry {
        /// <summary>
        /// The key of the CacheEntry.
        /// </summary>
        internal string Key { get; set; }

        /// <summary>
        /// The value of the CacheEntry.
        /// </summary>
        internal object Value { get; set; }

        /// <summary>
        /// Determines whether the CacheEntry key should be hashed or not
        /// </summary>
        internal bool Sha256HashKey { get; set; } = true;

        /// <summary>
        /// The DateTime when the CacheEntry should be counted as expired.
        /// </summary>
        internal DateTime ExpirationTime { get; set; } = DateTime.Now.AddSeconds(30);

        /// <summary>
        /// The CacheItemPriority which indicates whether an item should be stored indefinitely or not.
        /// </summary>
        internal CacheItemPriority Priority { get; set; } = CacheItemPriority.Default;

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