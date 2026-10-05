using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Globalization;
using System.Linq;
using System.Reflection;

namespace LocaleNames.Embed.Test
{
    /// <summary>
    /// Tests of the resources embedded by LocaleNames.Embed.
    /// </summary>
    [TestClass]
    public class EmbedTests
    {
        private static readonly Assembly ThisAssembly = typeof(EmbedTests).Assembly;

        /// <summary>
        /// GenerateAssemblyInfo is off in this project, so the assembly carries no marker attribute and
        /// LocaleNames.Core finds the embedded resources only once the assembly is registered.
        /// </summary>
        /// <param name="context">The context.</param>
        [AssemblyInitialize]
        public static void AssemblyInit(TestContext context)
        {
            Assert.IsFalse(ThisAssembly
                .GetCustomAttributes<AssemblyMetadataAttribute>()
                .Any(i => i.Key == LocaleResourceProvider.AssemblyMetadataKey));

            Assert.IsTrue(LocaleTranslationsFactory.ForLanguageCode("cs").AreCountryNameTranslationsEmpty,
                "resources must not be found before the assembly is registered");

            LocaleResourceProvider.Register(ThisAssembly);
        }

        /// <summary>
        /// Only the requested locales are embedded.
        /// </summary>
        [TestMethod]
        public void Resources_ContainOnlyRequestedLocales()
        {
            var names = ThisAssembly.GetManifestResourceNames()
                .Where(i => i.StartsWith("LocaleNames.Resources.language.", StringComparison.Ordinal))
                .ToList();

            CollectionAssert.IsSubsetOf(
                new[]
                {
                    "LocaleNames.Resources.language.cs.languages.json.gz",
                    "LocaleNames.Resources.language.cs.territories.json.gz",
                    "LocaleNames.Resources.language.cs.currencies.json.gz",
                    "LocaleNames.Resources.language.en_GB.territories.json.gz",
                },
                names);

            Assert.IsTrue(names.All(i =>
                i.Contains(".language.cs.") || i.Contains(".language.en.") || i.Contains(".language.en_GB.")));
        }

        /// <summary>
        /// The data assembly is not needed, nor loaded.
        /// </summary>
        [TestMethod]
        public void DataAssembly_IsNotUsed()
        {
            LocaleTranslationsFactory.ForLanguageCode("cs").FindCountryName("DE");

            Assert.IsFalse(AppDomain.CurrentDomain.GetAssemblies()
                .Any(i => i.GetName().Name == LocaleResourceProvider.DataAssemblyName));
        }

        /// <summary>
        /// Translations are loaded from the embedded resources.
        /// </summary>
        [TestMethod]
        public void Translations_AreLoadedFromEmbeddedResources()
        {
            var cs = LocaleTranslationsFactory.ForCultureInfo(new CultureInfo("cs-CZ"));
            var en = LocaleTranslationsFactory.ForCultureInfo(new CultureInfo("en-US"));

            Assert.AreEqual("Německo", cs.FindCountryName("DE"));
            Assert.AreEqual("DE", cs.FindCountryCode("Německo"));
            Assert.AreEqual("čeština", cs.FindLanguageName("cs"));
            Assert.AreEqual("česká koruna", cs.FindCurrencyName("CZK"));
            Assert.AreEqual("Germany", en.FindCountryName("DE"));
        }

        /// <summary>
        /// A locale that was not embedded has no translations.
        /// </summary>
        [TestMethod]
        public void Translations_OfNotEmbeddedLocale_AreEmpty()
        {
            var de = LocaleTranslationsFactory.ForLanguageCode("de");

            Assert.IsTrue(de.AreCountryNameTranslationsEmpty);
            Assert.IsTrue(de.AreLanguageTranslationsEmpty);
            Assert.IsTrue(de.AreCurrencyTranslationsEmpty);
        }
    }
}
