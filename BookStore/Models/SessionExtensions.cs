using Microsoft.IdentityModel.Tokens;
using System.Text.Json;

namespace BookStore.Models
{
    /// <summary>
    /// Extends <see cref="ISession"/> with JSON-based get/set so complex objects (e.g., the
    /// shopping cart) can be stored in the server-side session without manual serialization.
    /// </summary>
    public static class SessionExtensions
    {
        public static void Set<T>(this ISession session, string key, T value)
        {
            session.SetString(key, JsonSerializer.Serialize(value));
        }
        public static T? Get<T>(this ISession session, string key)
        {
            var json = session.GetString(key);
            if (string.IsNullOrEmpty(json))
            {
                return default;
            }
            else
            {
                return JsonSerializer.Deserialize<T>(json);
            }
        }
    }
}
