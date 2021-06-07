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
                    AbsoluteExpiration = cacheEntry.ExpirationTime
                };

                if (cacheEntry.CreateSha256Sum) {
                    using (SHA256 sha256 = SHA256.Create()) {
                        var chars = sha256.ComputeHash(Encoding.UTF8.GetBytes(cacheEntry.Key));
                        var stringBuilder = new StringBuilder();
                            
                        for (int i = 0; i < chars.Length; i++) {
                            stringBuilder.Append(chars[i].ToString("x2"));
                        }

                        cacheEntry.Value = stringBuilder.ToString();
                    }
                }

                MemoryCache.Default.Set(cacheEntry.Key, cacheEntry.Value, policy);
            }
        }

        /// <summary>
        /// Checks whether the specified CacheEntry has been cached or not 
        /// </summary>
        /// <param name="cacheEntry">The CacheEntry to check</param>
        /// <returns>
        /// <c>true</c> if cache contains entry (expired)<br/>
        /// <c>false</c> if cache does not contain entry (not expired)
        /// </returns>
        public static bool CheckCacheEntryExpiration(CacheEntry cacheEntry) {
            var returnValue = false;
            string cacheValue = (string)MemoryCache.Default[cacheEntry.Key];

            if (!string.IsNullOrEmpty(cacheValue)) {
                returnValue = true;
            } else {
                if (cacheEntry.AddIfNotCached) {
                    AddCacheEntry(cacheEntry);
                }
            }

            return returnValue;
        }
    }
}
