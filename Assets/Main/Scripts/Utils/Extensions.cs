using System;
using System.Diagnostics.CodeAnalysis;
using Fusion;
using Main.Scripts.Core.Architecture;
using UnityEngine;
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

        public static T? GetInterface<T>(this Component component)
        {
            var holder = component.GetComponent<InterfacesHolder>();
            return holder != null ? holder.GetInterface<T>() : default;
        }

        public static T? GetInterface<T>(this GameObject gameObject)
        {
            var holder = gameObject.GetComponent<InterfacesHolder>();
            return holder != null ? holder.GetInterface<T>() : default;
        }

        public static bool TryGetInterface<T>(this Component component, out T typed)
        {
            var holder = component.GetComponent<InterfacesHolder>();
            if (holder != null)
            {
                return holder.TryGetInterface(out typed);
            }

            typed = default!;
            return false;
        }

        public static bool TryGetInterface<T>(this GameObject gameObject, out T typed)
        {
            var holder = gameObject.GetComponent<InterfacesHolder>();
            if (holder != null)
            {
                return holder.TryGetInterface(out typed);
            }

            typed = default!;
            return false;
        }
    }
}