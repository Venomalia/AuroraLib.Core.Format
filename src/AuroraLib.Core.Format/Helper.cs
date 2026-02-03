using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Text;

namespace AuroraLib.Core.Format
{
    internal static class Helper
    {

        /// <summary>
        /// The default encoding used for operations.
        /// </summary>
        public static readonly Encoding DefaultEncoding = Encoding.GetEncoding(28591);

        [DebuggerStepThrough]
        public static string GetCString(ReadOnlySpan<byte> bytes, Encoding encoder, byte terminator = 0x0)
        {
            int length = bytes.IndexOf(terminator);
            if (length == -1) length = bytes.Length;
            return encoder.GetString(bytes.Slice(0, length));
        }

        [DebuggerStepThrough]
        public static string GetCString(ReadOnlySpan<byte> bytes, byte terminator = 0x0) => GetCString(bytes, DefaultEncoding, terminator);

        [DebuggerStepThrough]
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static int SequenceGetHashCode<T>(this ReadOnlySpan<T> span) where T : unmanaged
        {
#if NET20_OR_GREATER || NETSTANDARD2_0
            unchecked
            {
                int hash = 17;
                foreach (T item in span)
                {
                    hash = hash * 31 + item.GetHashCode();
                }
                return hash;
            }
#else

#if !NETSTANDARD
            // If char use string implementation
            if (typeof(T) == typeof(char))
            {
                ReadOnlySpan<char> chars = MemoryMarshal.Cast<T, char>(span);
                return string.GetHashCode(chars);
            }
#endif
            HashCode gen = default;
#if NET6_0_OR_GREATER 
            ReadOnlySpan<byte> buffer = MemoryMarshal.Cast<T, byte>(span);
            gen.AddBytes(buffer);
#else
            foreach (T b in span)
            {
                gen.Add(b);
            }
#endif
            return gen.ToHashCode();
#endif
        }

        public static bool Match(this Stream stream, ReadOnlySpan<byte> expected)
        {
#if NET20_OR_GREATER || NETSTANDARD2_0
            byte[] buffer = new byte[expected.Length];
            int i = stream.Read(buffer, 0, expected.Length);
            return i == expected.Length && buffer.AsSpan().SequenceEqual(expected);
#else
            Span<byte> buffer = stackalloc byte[expected.Length];
            int i = stream.Read(buffer);
            return i == expected.Length && buffer.SequenceEqual(expected);
#endif
        }

#if NET20_OR_GREATER || NETSTANDARD2_0

        [DebuggerStepThrough]
        public static unsafe string GetString(this Encoding encoding, ReadOnlySpan<byte> bytes)
        {
            fixed (byte* bytePtr = bytes)
            {
                return encoding.GetString(bytePtr, bytes.Length);
            }
        }

        [DebuggerStepThrough]
        public static unsafe int GetBytes(this Encoding encoding, ReadOnlySpan<char> chars, Span<byte> bytes)
        {
            fixed (char* charPtr = chars)
            fixed (byte* bytePtr = bytes)
            {
                return encoding.GetBytes(charPtr, chars.Length, bytePtr, bytes.Length);
            }
        }

        [DebuggerStepThrough]
        public static bool TryAdd<TKey, TValue>(this IDictionary<TKey, TValue> dictionary, TKey key, TValue value) where TKey : notnull
        {
            if (dictionary is null) throw new ArgumentNullException(nameof(dictionary));

            if (!dictionary.ContainsKey(key))
            {
                dictionary.Add(key, value);
                return true;
            }

            return false;
        }

        [DebuggerStepThrough]
        public static bool Remove<TKey, TValue>(this IDictionary<TKey, TValue> dictionary, TKey key, out TValue value) where TKey : notnull
        {
            if (dictionary is null) throw new ArgumentNullException(nameof(dictionary));

            if (dictionary.TryGetValue(key, out value))
            {
                dictionary.Remove(key);
                return true;
            }

            value = default;
            return false;
        }
#endif
    }
}
