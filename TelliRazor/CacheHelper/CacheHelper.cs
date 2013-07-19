using System;
using Telligent.Evolution.Extensibility.Caching.Version1;

namespace TelliRazor
{
    internal static class CacheHelper
    {
        private const string Prefix = "TelliRazor";

        internal static T Get<T>(string key, Func<T> getFunction, CacheScope scope = CacheScope.Context | CacheScope.Process)
        {
            return Get(key, getFunction, TimeSpan.FromSeconds(100), scope);
        }

        internal static T Get<T>(string key, Func<T> getFunction, TimeSpan timeout, CacheScope scope = CacheScope.Context | CacheScope.Process)
        {
            key = Prefix + key;

            var cachedObj = CacheService.Get(key, scope);
            if (cachedObj != null)
                return (T)cachedObj;

            var obj = getFunction();
            CacheService.Put(key, obj, scope, timeout);
            return obj;
        }

        internal static void Remove(string key, CacheScope scope = CacheScope.Context | CacheScope.Process)
        {
            key = Prefix + key;
            CacheService.Remove(key, scope);
        }
    }
}
