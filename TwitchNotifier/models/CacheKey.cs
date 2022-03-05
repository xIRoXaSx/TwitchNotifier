using System;
using System.Runtime.Caching;

namespace TwitchNotifier.models {
    internal class CacheEntry {
        /// <summary>
        /// The key of the CacheEntry.
        /// </summary>
        public string Key { get; set; }

        /// <summary>
        /// The value of the CacheEntry.
        /// </summary>
        public object Value { get; set; }

        /// <summary>
        /// Determines whether the CacheEntry key should be hashed or not
        /// </summary>
        public bool Sha256HashKey { get; set; } = true;

        /// <summary>
        /// The DateTime when the CacheEntry should be counted as expired.
        /// </summary>
        public DateTime ExpirationTime { get; set; } = DateTime.Now.AddSeconds(30);

        /// <summary>
        /// The CacheItemPriority which indicates whether an item should be stored indefinitely or not.
        /// </summary>
        public CacheItemPriority Priority { get; set; } = CacheItemPriority.Default;
    }
}