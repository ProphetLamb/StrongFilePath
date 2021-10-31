using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Runtime.CompilerServices;

namespace StrongFilePath
{
    internal static class CollectionExtensions
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool IsNullOrEmpty<T>([NotNullWhen(false)] this IReadOnlyCollection<T>? self)
        {
            return self is null || self.Count == 0;
        }

        public static T? Get<T>(this IReadOnlyList<T>? self, int index)
        {
            if (self is null || (uint)index >= (uint)self.Count)
            {
                return default(T);
            }

            return self[index];
        }

        public static int IndexOf<T>(this T[] self, in T item)
            where T: IEquatable<T>
        {
            for (int i = 0; i < self.Length; i++)
            {
                if (self[i].Equals(item))
                {
                    return i;
                }
            }
            return -1;
        }

        public static bool Contains<T>(this T[] self, in T item)
            where T: IEquatable<T>
        {
            return self.IndexOf(item) != -1;
        }

        public static bool SwapRemove<T>(this IList<T> self, T item)
        {
            int index = self.IndexOf(item);
            if (index == -1)
            {
                return false;
            }
            SwapRemoveAt(self, index);
            return true;
        }

        public static void SwapRemoveAt<T>(this IList<T> self, int index)
        {
            int last = self.Count - 1;
            if ((nuint)index >= (nuint)last)
            {
                throw new ArgumentOutOfRangeException(nameof(index));
            }
            if (last == 0)
            {
                self.Clear();
            }
            else
            {
                self[index] = self[last];
                // No array copy when removing the last element
                self.RemoveAt(last);
            }
        }
    }
}
