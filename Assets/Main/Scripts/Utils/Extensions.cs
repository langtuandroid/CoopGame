using System;
using System.Diagnostics.CodeAnalysis;
using Fusion;
using UnityEngine.UIElements;

namespace Main.Scripts.Utils
{
    public static class Extensions
    {
        public static T ThrowWhenNull<T>([NotNull] this T? value)
        {
            return value ?? throw new ArgumentNullException(typeof(T).ToString());
        }
        
        public static T ThrowWhenNull<T>([NotNull] this Nullable<T> value) where T : struct
        {
            return value ?? throw new ArgumentNullException(typeof(T).ToString());
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

        public static void SetVisibility(this UIDocument doc, bool isVisible)
        {
            doc.rootVisualElement.style.display = isVisible ? DisplayStyle.Flex : DisplayStyle.None;
        }
    }
}