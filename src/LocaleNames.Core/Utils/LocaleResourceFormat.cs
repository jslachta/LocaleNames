using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Text;
using LocaleNames.Infrastructure.Diagnostics;

namespace LocaleNames.Utils
{
    /// <summary>
    /// Binary format of the embedded locale resources (<c>language.{locale}.{type}.lnb</c>).
    /// </summary>
    /// <remarks>
    /// <para>
    /// The format is read and written by hand, so it needs no reflection and is safe for trimming and native AOT.
    /// </para>
    /// <code>
    /// file    = 'L' 'N' version(1 byte) deflate(payload)
    /// payload = count entry*
    /// entry   = key value
    /// key     = length utf8-bytes
    /// value   = length utf8-bytes
    /// count, length = unsigned LEB128 varint
    /// </code>
    /// The entries keep the order they were written in, which is the order of the dictionary they are read into
    /// (lookup by a name returns the first match, so the order is part of the data).
    /// </remarks>
    public static class LocaleResourceFormat
    {
        /// <summary>
        /// Version of the format.
        /// </summary>
        public const byte CurrentVersion = 1;

        private const byte Magic0 = (byte)'L';
        private const byte Magic1 = (byte)'N';

        /// <summary>
        /// Serializes the values into the binary resource format.
        /// </summary>
        /// <param name="values">Code as a key, translation as a value. The enumeration order is preserved.</param>
        /// <returns>The content of the resource file.</returns>
        public static byte[] Serialize(IReadOnlyDictionary<string, string> values)
        {
            ExceptionGuard.ThrowIfNull(values);

            using (var payload = new MemoryStream())
            {
                WriteVarint(payload, values.Count);

                foreach (var item in values)
                {
                    WriteString(payload, item.Key);
                    WriteString(payload, item.Value);
                }

                using (var result = new MemoryStream())
                {
                    result.WriteByte(Magic0);
                    result.WriteByte(Magic1);
                    result.WriteByte(CurrentVersion);

#if NET6_0_OR_GREATER
                    const CompressionLevel level = CompressionLevel.SmallestSize;
#else
                    const CompressionLevel level = CompressionLevel.Optimal;
#endif

                    using (var deflate = new DeflateStream(result, level, leaveOpen: true))
                    {
                        payload.WriteTo(deflate);
                    }

                    return result.ToArray();
                }
            }
        }

        /// <summary>
        /// Deserializes the binary resource format.
        /// </summary>
        /// <param name="stream">The stream with the content of the resource file.</param>
        /// <returns>Code as a key, translation as a value, in the order they were written in.</returns>
        /// <exception cref="InvalidDataException">The stream is not a valid resource.</exception>
        public static Dictionary<string, string> Deserialize(Stream stream)
        {
            ExceptionGuard.ThrowIfNull(stream);

            if (ReadByte(stream) != Magic0 || ReadByte(stream) != Magic1)
            {
                throw new InvalidDataException("Not a LocaleNames resource.");
            }

            var version = ReadByte(stream);

            if (version != CurrentVersion)
            {
                throw new InvalidDataException($"Unsupported LocaleNames resource version {version}.");
            }

            using (var payload = new MemoryStream())
            {
                using (var deflate = new DeflateStream(stream, CompressionMode.Decompress, leaveOpen: true))
                {
                    deflate.CopyTo(payload);
                }

                return Parse(payload.GetBuffer(), (int)payload.Length);
            }
        }

        private static Dictionary<string, string> Parse(byte[] buffer, int length)
        {
            var position = 0;
            var count = ReadVarint(buffer, length, ref position);

            // every entry takes at least two bytes (the lengths of the key and the value)
            if (count > (length - position) / 2)
            {
                throw new InvalidDataException("Invalid LocaleNames resource: the entry count exceeds the data.");
            }

            var result = new Dictionary<string, string>(count);

            for (var i = 0; i < count; i++)
            {
                var key = ReadString(buffer, length, ref position);
                var value = ReadString(buffer, length, ref position);

                // later entries replace the earlier ones of the same key
                result[key] = value;
            }

            if (position != length)
            {
                throw new InvalidDataException("Invalid LocaleNames resource: unexpected trailing data.");
            }

            return result;
        }

        private static int ReadByte(Stream stream)
        {
            var value = stream.ReadByte();

            if (value < 0)
            {
                throw new InvalidDataException("Invalid LocaleNames resource: unexpected end of data.");
            }

            return value;
        }

        private static void WriteString(MemoryStream stream, string value)
        {
            var bytes = Encoding.UTF8.GetBytes(value ?? string.Empty);

            WriteVarint(stream, bytes.Length);
            stream.Write(bytes, 0, bytes.Length);
        }

        private static void WriteVarint(MemoryStream stream, int value)
        {
            var rest = (uint)value;

            while (rest >= 0x80)
            {
                stream.WriteByte((byte)(rest | 0x80));
                rest >>= 7;
            }

            stream.WriteByte((byte)rest);
        }

        private static string ReadString(byte[] buffer, int length, ref int position)
        {
            var size = ReadVarint(buffer, length, ref position);

            if (size > length - position)
            {
                throw new InvalidDataException("Invalid LocaleNames resource: the string exceeds the data.");
            }

            var value = Encoding.UTF8.GetString(buffer, position, size);

            position += size;

            return value;
        }

        private static int ReadVarint(byte[] buffer, int length, ref int position)
        {
            uint result = 0;

            for (var shift = 0; shift < 35; shift += 7)
            {
                if (position >= length)
                {
                    throw new InvalidDataException("Invalid LocaleNames resource: unexpected end of data.");
                }

                var current = buffer[position++];

                result |= (uint)(current & 0x7F) << shift;

                if ((current & 0x80) == 0)
                {
                    if (result > int.MaxValue)
                    {
                        break;
                    }

                    return (int)result;
                }
            }

            throw new InvalidDataException("Invalid LocaleNames resource: invalid number.");
        }
    }
}
