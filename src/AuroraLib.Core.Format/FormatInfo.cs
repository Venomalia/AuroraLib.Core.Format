using AuroraLib.Core.Format.Identifier;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;

namespace AuroraLib.Core.Format
{
    /// <inheritdoc cref="IFormatInfo"/>
    [DebuggerDisplay("{FullName} ({MIMEType})")]
    public sealed class FormatInfo : IFormatInfo
    {
        /// <inheritdoc/>
        public string FullName { get; }

        /// <inheritdoc/>
        public MediaType MIMEType { get; }

        /// <inheritdoc/>
        public IEnumerable<string> FileExtensions { get; }

        /// <inheritdoc/>
        public IIdentifier? Identifier { get; }

        /// <inheritdoc/>
        public int IdentifierOffset { get; }

        /// <inheritdoc/>
        public Type? Class { get; }

        private readonly MatchAction? IsMatchAction;

        /// <inheritdoc cref="IFormatRecognition.IsMatch(Stream, ReadOnlySpan{char})"/>
        public delegate bool MatchAction(Stream stream, ReadOnlySpan<char> fileNameAndExtension = default);

        public FormatInfo(string fullName, MediaType mediaType, string fileExtension, IIdentifier? identifier = null, int identifierOffset = 0, Type? @class = null, MatchAction? isMatchAction = null)
            : this(fullName, mediaType, new string[] { fileExtension }, identifier, identifierOffset, @class, isMatchAction)
        { }

        public FormatInfo(string fullName, MediaType mediaType, IEnumerable<string> fileExtensions, IIdentifier? identifier = null, int identifierOffset = 0, Type? @class = null, MatchAction? isMatchAction = null)
        {
            if (fullName is null) throw new ArgumentNullException(nameof(fullName));
            if (mediaType is null) throw new ArgumentNullException(nameof(mediaType));
            if (fileExtensions is null) throw new ArgumentNullException(nameof(fileExtensions));

            FullName = fullName;
            MIMEType = mediaType;
            FileExtensions = fileExtensions;
            Identifier = identifier;
            IdentifierOffset = identifierOffset;
            IsMatchAction = isMatchAction;
            Class = @class;

            if (!(Class is null) && IsMatchAction is null)
            {
                object? instance = CreateInstance();
                if (instance is IFormatRecognition recognition)
                    IsMatchAction = recognition.IsMatch;
            }
        }

        /// <inheritdoc/>
        public object? CreateInstance()
        {
            if (Class is null) throw new ArgumentNullException(nameof(Class));
            return Activator.CreateInstance(Class);
        }

        /// <inheritdoc/>
        public bool IsMatch(Stream stream, ReadOnlySpan<char> fileNameAndExtension = default)
        {
            if (!(IsMatchAction is null))
                return IsMatchAction(stream, fileNameAndExtension);

            if (!(Identifier is null))
            {
                ReadOnlySpan<byte> identifier = Identifier.AsSpan();
                if (stream.Length >= IdentifierOffset + identifier.Length)
                {
                    stream.Seek(IdentifierOffset, SeekOrigin.Begin);
                    return stream.Match(identifier);
                }
                return false;
            }

#if NET20_OR_GREATER || NETSTANDARD2_0
            ReadOnlySpan<char> extension = Path.GetExtension(fileNameAndExtension.ToString()).AsSpan();
#else
            ReadOnlySpan<char> extension = Path.GetExtension(fileNameAndExtension);
#endif
            foreach (var value in FileExtensions)
            {
                if (extension.Contains(value.AsSpan(), StringComparison.InvariantCultureIgnoreCase))
                    return true;
            }
            return false;
        }

        /// <inheritdoc/>
        public bool Equals(IFormatInfo? other) => !(other is null) && other.MIMEType.Equals(MIMEType);

        /// <inheritdoc/>
        public override bool Equals(object? obj) => obj is IFormatInfo info && Equals(info);

        /// <inheritdoc/>
        public override int GetHashCode() => MIMEType.GetHashCode();
    }
}
