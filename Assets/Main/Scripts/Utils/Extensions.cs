using System;
using System.Diagnostics.CodeAnalysis;
using Fusion;

namespace Main.Scripts.Utils
{
    public static class Extensions
    {
        public static T ThrowWhenNull<T>([NotNull] this T? value, string message = "")
        {
            return value ?? throw new ArgumentNullException(message);
        }

        public static bool Equals<K, V>(this NetworkDictionary<K, V> value, NetworkDictionary<K, V> other)
        {
            if (value.Count != other.Count)
            {
                return false;
            }

            foreach (var (key, val) in value)
            {
                if (!other.ContainsKey(key) || !other.Get(key)!.Equals(val))
                {
                    return false;
                }
            }

            return true;
        }
    }
}