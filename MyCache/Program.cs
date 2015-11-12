using System;
using System.Runtime.Caching;

namespace MyCache
{
    class Program
    {
        static void Main(string[] args)
        {
            var myCache = new MyCache();

            myCache.Add("1");

            while (myCache.IsInCache("1"))
            {
                System.Threading.SpinWait.SpinUntil(() => false, TimeSpan.FromMilliseconds(500));
            }

            Console.ReadLine();
        }
    }


    public class MyCache
    {
        private const string typeName = "MyCache";
        private object lockStore = new object();

        private readonly MemoryCache store = new MemoryCache(typeName);

        private class CacheData
        {
            public CacheItemPolicy CacheItemPolicy { get; set; }
        }

        public void Add(string id)
        {
            Console.WriteLine("{0} - Add... for '{1}'", typeName, id);

            var cacheItemPolicy = new CacheItemPolicy();

            cacheItemPolicy.AbsoluteExpiration = DateTimeOffset.Now.AddSeconds(5);

            var cacheItem = new CacheItem(id, new CacheData { CacheItemPolicy = cacheItemPolicy });
            store.Add(cacheItem, cacheItemPolicy);
        }

        public bool IsInCache(string id)
        {
            var cacheData = store.Get(id);
            var exists = cacheData != null;
            var remainingTime = GetRemainingTime(cacheData);

            Console.WriteLine("{0} - IsInCache... for '{1}'... '{2}' Remaining time {3}s",
                typeName,
                id,
                exists,
                remainingTime.TotalSeconds.ToString("##0"));

            return exists;
        }

        private TimeSpan GetRemainingTime(object o)
        {
            if (o == null) return TimeSpan.Zero;

            var cacheData = o as CacheData;

            if (cacheData == null) return TimeSpan.Zero;

            var cacheItemPolicy = cacheData.CacheItemPolicy;

            return cacheItemPolicy.AbsoluteExpiration.Subtract(DateTimeOffset.Now);
        }

    }
}
