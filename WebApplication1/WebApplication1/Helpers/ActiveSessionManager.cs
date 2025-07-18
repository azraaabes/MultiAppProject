using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace WebApplication1.Helpers
{
    public class ActiveSessionManager
    {
        // <username , sonToken>
        private static readonly ConcurrentDictionary<string, string> _activeTokens = new ConcurrentDictionary<string, string>();

        public static void Set(string username, string token)   // yeni login
        {
            _activeTokens[username] = token;
        }
        // Her istek geldiğinde çağrılır → Token hâlâ geçerli mi?
        public static bool IsValid(string username, string token)
        {
            if (_activeTokens.TryGetValue(username, out string mevcutToken)) //bellekte kullanıcıya ait bir token var mı?
            {
                return mevcutToken == token;
            }

            return false;
        }
        // Logout veya oturum çökünce token silinir
        public static void Remove(string username)
        {
            if (!string.IsNullOrEmpty(username))
                _activeTokens.TryRemove(username, out _);
        }
    }
}