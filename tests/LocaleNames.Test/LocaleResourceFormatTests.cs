using LocaleNames.Utils;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.IO.Compression;
using System.Linq;

namespace LocaleNames.Test
{
    /// <summary>
    /// LocaleResourceFormat tests.
    /// </summary>
    [TestClass]
    public class LocaleResourceFormatTests
    {
        private static readonly Dictionary<string, string> Sample = new()
        {
            ["CZ"] = "Czechia",
            ["DE"] = "Germany",
            ["AT"] = "Austria",
            ["CH"] = "Switzerland",
            ["SK"] = "Slovakia",
        };

        private static Dictionary<string, string> Roundtrip(byte[] data)
        {
            using var stream = new MemoryStream(data);

            return LocaleResourceFormat.Deserialize(stream);
        }

        // a valid header followed by the compressed payload
        private static byte[] Resource(params byte[] payload)
        {
            using var file = new MemoryStream();
            file.Write(new byte[] { (byte)'L', (byte)'N', 1 }, 0, 3);

            using (var deflate = new DeflateStream(file, CompressionMode.Compress, true))
            {
                deflate.Write(payload, 0, payload.Length);
            }

            return file.ToArray();
        }

        /// <summary>
        /// Values come back the same, in the same order.
        /// </summary>
        [TestMethod]
        public void Serialize_Deserialize_PreservesValuesAndOrder()
        {
            var values = new Dictionary<string, string>
            {
                ["zz"] = "last alphabetically, first in the file",
                ["CZK-count-few"] = "české koruny",
                ["AA"] = "",
                ["日本"] = "日本語 🇯🇵 \u0000 \"quoted\" \\ back",
            };

            var result = Roundtrip(LocaleResourceFormat.Serialize(values));

            CollectionAssert.AreEqual(values.ToList(), result.ToList());
        }

        /// <summary>
        /// A null value (or key) is written as an empty string.
        /// </summary>
        [TestMethod]
        public void Serialize_NullValue_WrittenAsEmpty()
        {
            var values = new Dictionary<string, string> { ["CZ"] = null };

            var result = Roundtrip(LocaleResourceFormat.Serialize(values));

            Assert.AreEqual(1, result.Count);
            Assert.AreEqual(string.Empty, result["CZ"]);
        }

        /// <summary>
        /// An empty resource is valid.
        /// </summary>
        [TestMethod]
        public void Serialize_Deserialize_Empty()
        {
            Assert.AreEqual(0, Roundtrip(LocaleResourceFormat.Serialize(new Dictionary<string, string>())).Count);
        }

        /// <summary>
        /// A large resource (multi-byte lengths) goes through.
        /// </summary>
        [TestMethod]
        public void Serialize_Deserialize_Large()
        {
            var values = Enumerable.Range(0, 5000).ToDictionary(i => "key" + i, i => new string('x', i % 400) + i);

            CollectionAssert.AreEqual(values.ToList(), Roundtrip(LocaleResourceFormat.Serialize(values)).ToList());
        }

        /// <summary>
        /// Garbage is rejected with InvalidDataException.
        /// </summary>
        [TestMethod]
        public void Deserialize_InvalidData_Throws()
        {
            var valid = LocaleResourceFormat.Serialize(Sample);

            Assert.ThrowsExactly<InvalidDataException>(() => Roundtrip(Array.Empty<byte>()));
            Assert.ThrowsExactly<InvalidDataException>(() => Roundtrip(new byte[] { (byte)'X', (byte)'N', 1 }));
            Assert.ThrowsExactly<InvalidDataException>(() => Roundtrip(new byte[] { (byte)'L', (byte)'N', 99 }));
            Assert.ThrowsExactly<InvalidDataException>(() => Roundtrip(new byte[] { (byte)'L', (byte)'N', 1 }));
            Assert.ThrowsExactly<InvalidDataException>(() => Roundtrip(valid.Take(valid.Length - 4).ToArray()));
        }

        /// <summary>
        /// Counts and lengths exceeding the data are rejected, they do not allocate.
        /// </summary>
        [TestMethod]
        public void Deserialize_HugeCount_Throws()
        {
            using var file = new MemoryStream();
            file.Write(new byte[] { (byte)'L', (byte)'N', 1 }, 0, 3);

            using (var deflate = new DeflateStream(file, CompressionMode.Compress, true))
            {
                // count = int.MaxValue, no entries
                deflate.Write(new byte[] { 0xFF, 0xFF, 0xFF, 0xFF, 0x07 }, 0, 5);
            }

            Assert.ThrowsExactly<InvalidDataException>(() => Roundtrip(file.ToArray()));
        }

        /// <summary>
        /// Bytes after the last entry are rejected.
        /// </summary>
        [TestMethod]
        public void Deserialize_TrailingData_Throws()
        {
            // count = 0, followed by a stray byte
            var ex = Assert.ThrowsExactly<InvalidDataException>(() => Roundtrip(Resource(0x00, 0x00)));

            StringAssert.Contains(ex.Message, "trailing data");
        }

        /// <summary>
        /// A string longer than the remaining data is rejected.
        /// </summary>
        [TestMethod]
        public void Deserialize_StringExceedsData_Throws()
        {
            // count = 1, key length = 5, only one byte of the key follows
            var ex = Assert.ThrowsExactly<InvalidDataException>(() => Roundtrip(Resource(0x01, 0x05, (byte)'a')));

            StringAssert.Contains(ex.Message, "string exceeds");
        }

        /// <summary>
        /// A number that is too long or does not fit the int is rejected.
        /// </summary>
        [TestMethod]
        public void Deserialize_InvalidNumber_Throws()
        {
            // never-ending continuation bits
            Assert.ThrowsExactly<InvalidDataException>(() => Roundtrip(Resource(0x80, 0x80, 0x80, 0x80, 0x80, 0x00)));

            // uint.MaxValue
            Assert.ThrowsExactly<InvalidDataException>(() => Roundtrip(Resource(0xFF, 0xFF, 0xFF, 0xFF, 0x0F)));
        }

        /// <summary>
        /// A number cut off by the end of the data is rejected.
        /// </summary>
        [TestMethod]
        public void Deserialize_TruncatedNumber_Throws()
        {
            // no payload at all, the entry count is missing
            var ex = Assert.ThrowsExactly<InvalidDataException>(() => Roundtrip(Resource()));

            StringAssert.Contains(ex.Message, "unexpected end of data");

            // the entry count announces another byte that never comes
            Assert.ThrowsExactly<InvalidDataException>(() => Roundtrip(Resource(0x80)));

            // count = 1, the key length is cut off after a continuation byte
            Assert.ThrowsExactly<InvalidDataException>(() => Roundtrip(Resource(0x01, 0x80)));
        }

        /// <summary>
        /// Later entries replace the earlier ones of the same key.
        /// </summary>
        [TestMethod]
        public void Deserialize_DuplicateKey_LastWins()
        {
            // count = 2: "a" = "1", "a" = "2"
            var result = Roundtrip(Resource(0x02, 0x01, (byte)'a', 0x01, (byte)'1', 0x01, (byte)'a', 0x01, (byte)'2'));

            Assert.AreEqual(1, result.Count);
            Assert.AreEqual("2", result["a"]);
        }

        /// <summary>
        /// The embedded data of every culture loads.
        /// </summary>
        [TestMethod]
        public void EmbeddedData_OfEveryCulture_Loads()
        {
            var loaded = 0;

            foreach (var culture in CultureInfo.GetCultures(CultureTypes.AllCultures).Where(i => i.Name.Length > 0))
            {
                var translations = LocaleTranslationsFactory.ForCultureInfo(culture);

                // touches all three dictionaries
                if (!translations.AreLanguageTranslationsEmpty && !translations.AreCountryNameTranslationsEmpty && !translations.AreCurrencyTranslationsEmpty)
                {
                    loaded++;
                }
            }

            Assert.IsGreaterThan(100, loaded);
        }
    }
}
