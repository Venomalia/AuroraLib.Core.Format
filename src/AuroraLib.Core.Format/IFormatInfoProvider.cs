using System;
using System.IO;

namespace AuroraLib.Core.Format
{
    /// <summary>
    /// Defines a provider that supplies file format information.
    /// </summary>
    public interface IFormatInfoProvider : IFormatRecognition
    {
        /// <summary>
        /// Gets the file format information associated with the provider.
        /// </summary>
        IFormatInfo Info { get; }

#if NET8_0_OR_GREATER
        /// <inheritdoc cref="IFormatRecognition.IsMatch(Stream, ReadOnlySpan{char})"/>
        static abstract bool IsMatchStatic(Stream stream, ReadOnlySpan<char> fileNameAndExtension = default);
#endif
    }
}
