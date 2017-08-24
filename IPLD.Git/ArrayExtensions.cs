using System;

namespace IPLD.Git
{
    internal static class ArrayExtensions
    {
        public static T[] Skip<T>(this T[] array, int count)
        {
            var result = new T[Math.Min(array.Length, array.Length - count)];
            Array.Copy(array, count, result, 0, result.Length);
            return result;
        }

        public static T[] Take<T>(this T[] array, int count)
        {
            var result = new T[Math.Min(array.Length, count)];
            Array.Copy(array, result, result.Length);
            return result;
        }

        public static T[] Append<T>(this T[] array, params T[] items)
        {
            var result = new T[array.Length + items.Length];
            Array.Copy(array, result, array.Length);
            Array.Copy(items, 0, result, array.Length, items.Length);
            return result;
        }
    }
}