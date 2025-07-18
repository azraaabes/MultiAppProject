using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.Caching.StackExchangeRedis;

namespace WebApplication1.Helpers
{
    public class RedisCacheHelper
    {
        private static readonly Lazy<IDistributedCache> _lazyCache = new Lazy<IDistributedCache>(() =>  //Bu satır Redis cache nesnesini yalnızca 1 kere oluşturmak için kullanılır.Lazy<T> ile nesne ilk kez kullanıldığında oluşturulur (lazy initialization).
        {
            var options = new RedisCacheOptions
            {
                Configuration = System.Configuration.ConfigurationManager.AppSettings["RedisConnection"], // appSettings’de Redis bağlantı string’i olmalı
                InstanceName = "MyApp:" // key prefix
            };
            return new RedisCache(options);
        });

        public static IDistributedCache Cache => _lazyCache.Value;
    }
}
