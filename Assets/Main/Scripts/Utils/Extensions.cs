using System;
using System.Diagnostics.CodeAnalysis;

namespace Main.Scripts.Utils
{
    public static class Extensions
    {
        public static T ThrowWhenNull<T>([NotNull] this T? value, string message = "")
        {
            return value ?? throw new ArgumentNullException(message);
        }
    }
}