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
