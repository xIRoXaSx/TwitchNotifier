using System;

namespace TwitchNotifier.src.Helper {
    /// <summary>
    /// Class representing all required information to cache values via MemoryCache
    /// </summary>
    class CacheEntry {
        /// <summary>
        /// The key of the CacheEntry
        /// </summary>
        public string Key { get; set; }

        /// <summary>
        /// The value of the CacheEntry
        /// </summary>
        public object Value { get; set; }

        /// <summary>
        /// Determines whether the CacheEntry should be added to the MemoryCache or not
        /// <para>Default: <c>true</c></para>
        /// </summary>
        public bool AddIfNotCached { get; set; } = true;

        /// <summary>
        /// Determines whether the CacheEntry key (streamer) should be hashed or not
        /// <para>Default: <c>true</c></para>
        /// </summary>
        public bool CreateSha256Sum { get; set; } = true;

        /// <summary>
        /// The DateTime when the CacheEntry should be flagged as expired
        /// <para>Default: <c>Now + 30 seconds</c></para>
        /// </summary>
        public DateTime ExpirationTime { get; set; } = DateTime.Now.AddSeconds(30);
    }
}
