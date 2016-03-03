using System;
using System.Security.Cryptography;
using System.Web;
using System.Web.Caching;

namespace WebApiDavExtension.Authentication
{
    public class DigestNonce
    {
        public static string Generate()
        {
            var bytes = new byte[16];
            var rng = RandomNumberGenerator.Create();
            rng.GetBytes(bytes);

            var nonce = bytes.ToMd5Hash();
            SetCache(nonce, 0);
            return nonce;
        }

        private static void SetCache(string nonce, int count)
        {
            var key = CreateNonceKey(nonce);
            HttpContext.Current.Cache.Add(key, count, null, Cache.NoAbsoluteExpiration, TimeSpan.FromMinutes(10), CacheItemPriority.Normal, null);
        }

        private static int? GetFromCache(string nonce)
        {
            var key = CreateNonceKey(nonce);
            return HttpContext.Current.Cache.Get(key) as int?;
        }

        private static string CreateNonceKey(string nonce)
        {
            return "Nonce_" + nonce;
        }

        public static bool IsValid(string nonce, string nonceCount)
        {
            var count = GetFromCache(nonce);

            if (!count.HasValue)
                return false;

            if (int.Parse(nonceCount) <= count.Value)
                return false;

            SetCache(nonce, count.Value + 1);
            return true;
        }
    }
}
