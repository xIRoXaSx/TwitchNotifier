using System;
using System.Runtime.Caching;
using System.Security.Cryptography;
using System.Text;
using TwitchNotifier.src.Helper;

namespace TwitchNotifier.src {
    class Cache {
        /// <summary>
        /// Adds a CacheEntry to the MemoryCache
        /// </summary>
        /// <param name="cacheEntry">The CacheEntry to add to the MemoryCache</param>
        public static void AddCacheEntry(CacheEntry cacheEntry) {
            string cacheValue = (string)MemoryCache.Default[cacheEntry.Key];

            if (string.IsNullOrEmpty(cacheValue)) {
                CacheItemPolicy policy = new CacheItemPolicy() {
                    AbsoluteExpiration = cacheEntry.Priority == CacheItemPriority.NotRemovable ? ObjectCache.InfiniteAbsoluteExpiration : cacheEntry.ExpirationTime,
                    Priority = cacheEntry.Priority
                };

                MemoryCache.Default.Set(cacheEntry.Key, cacheEntry, policy);
            }
        }

        /// <summary>
        /// Checks whether the specified CacheEntry has been cached or not and adds it if AddIfNotCached is set to true
        /// </summary>
        /// <param name="cacheEntry">The CacheEntry to check</param>
        /// <returns>
        /// <c>true</c> if cache contains entry (not expired)<br/>
        /// <c>false</c> if cache does not contain entry (expired or not added)
        /// </returns>
        public static bool CheckCacheEntryExpiration(CacheEntry cacheEntry) {
            var returnValue = false;
            cacheEntry = HashCacheEntryKey(cacheEntry);
            var cacheValue = (CacheEntry)MemoryCache.Default[cacheEntry.Key];

            if (cacheValue != null) {
                returnValue = true;
            } else {
                if (cacheEntry.AddIfNotCached) {
                    AddCacheEntry(cacheEntry);
                }
            }

            return returnValue;
        }

        /// <summary>
        /// Hash the key of an CachEntry
        /// </summary>
        /// <param name="cacheEntry">The CacheEntry for which the key should be hashed</param>
        /// <returns></returns>
        public static CacheEntry HashCacheEntryKey(CacheEntry cacheEntry) {
            var returnValue = cacheEntry;

            if (cacheEntry.CreateSha256Sum) {
                cacheEntry.Key = HashString(cacheEntry.Key);
                returnValue = cacheEntry;
            }

            return returnValue;
        }

        /// <summary>
        /// Hash a string
        /// </summary>
        /// <param name="text">The string that should be hashed</param>
        /// <returns></returns>
        public static string HashString(string text) {
            var returnValue = string.Empty;

            using (SHA256 sha256 = SHA256.Create()) {
                var chars = sha256.ComputeHash(Encoding.UTF8.GetBytes(text));
                var stringBuilder = new StringBuilder();

                for (int i = 0; i < chars.Length; i++) {
                    stringBuilder.Append(chars[i].ToString("x2"));
                }

                returnValue = stringBuilder.ToString();
            }

            return returnValue;
        }
    }
}
