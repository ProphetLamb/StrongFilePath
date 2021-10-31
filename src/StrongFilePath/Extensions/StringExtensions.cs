using System;
using System.Runtime.CompilerServices;

namespace StrongFilePath
{
    public static class StringExtensions
    {
        /// <summary>
        ///     Indicates whether the specified string is <see langword="null" /> or an empty string (<c>""</c>).
        /// </summary>
        /// <param name="value">The string</param>
        /// <returns>
        ///     <see langword="true" /> if the string is <see langword="null" /> or an empty string (<c>""</c>); otherwise,
        ///     <see langword="false" />.
        /// </returns>
        internal static bool IsNullOrEmpty(this string? value)
        {
            return String.IsNullOrEmpty(value);
        }

        /// <summary>
        ///     Indicates whether the specified string is <see langword="null" />, empty or consists only of white-space
        ///     characters.
        /// </summary>
        /// <param name="value">The string</param>
        /// <returns>
        ///     <see langword="true" /> if the string is <see langword="null" />, empty or consists only of white-space
        ///     characters; otherwise, <see langword="false" />.
        /// </returns>
        internal static bool IsNullOrWhitespace(this string? value)
        {
            return String.IsNullOrWhiteSpace(value);
        }

        /// <summary>
        ///     Initializes a new <see cref="FilePath"/> from the <see cref="string"/>.
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static FilePath ToFilePath(this string self)
        {
            return new FilePath(self);
        }

        /// <summary>
        ///     Initializes a new <see cref="FilePath"/> from the <see cref="ReadOnlySpan{Char}"/>.
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static FilePath ToFilePath(this ReadOnlySpan<char> self)
        {
            return new FilePath(self.ToString());
        }
    }
}
