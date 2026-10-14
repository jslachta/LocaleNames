using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Globalization;
using System.Linq;
using System.Reflection;

namespace LocaleNames.Data.Test
{
    /// <summary>
    /// Tests of the selection of locales (LocaleNamesLocales): only the requested resources are embedded.
    /// </summary>
    [TestClass]
    public class DataSelectionTests
    {
        private static readonly Assembly ThisAssembly = typeof(DataSelectionTests).Assembly;

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

            // Without any data the lookup fails loudly and says how to fix it, it does not return empty translations.
            var error = Assert.ThrowsExactly<InvalidOperationException>(
                () => LocaleTranslationsFactory.ForLanguageCode("cs").AreCountryNameTranslationsEmpty);

            StringAssert.Contains(error.Message, "LocaleNames found no locale data");
            StringAssert.Contains(error.Message, "How to fix it");
            StringAssert.Contains(error.Message, "GenerateAssemblyInfo=true");
            StringAssert.Contains(error.Message, "LocaleResourceProvider.Register");
            StringAssert.Contains(error.Message, "TrimmerRootAssembly");

            LocaleResourceProvider.Register(ThisAssembly);
        }

        /// <summary>
        /// Registering an assembly without the resources is a mistake that is reported at once.
        /// </summary>
        [TestMethod]
        public void Register_AssemblyWithoutResources_Throws()
        {
            var error = Assert.ThrowsExactly<ArgumentException>(
                () => LocaleResourceProvider.Register(typeof(string).Assembly));

            StringAssert.Contains(error.Message, "contains no LocaleNames resources");
            StringAssert.Contains(error.Message, "LocaleNamesLocales");
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
                    "LocaleNames.Resources.language.cs.languages.lnb",
                    "LocaleNames.Resources.language.cs.territories.lnb",
                    "LocaleNames.Resources.language.cs.currencies.lnb",
                    "LocaleNames.Resources.language.en_GB.territories.lnb",
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
