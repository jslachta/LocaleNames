using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Linq;

namespace LocaleNames.Test
{
    /// <summary>
    /// FindLanguageCode tests.
    /// </summary>
    [TestClass]
    public class FindLanguageCodeTests
    {
        /// <summary>
        /// All language codes should not provide variants.
        /// </summary>
        [TestMethod]
        public void All_Language_Codes_Should_Not_Provide_Variants()
        {
            var localeNames = LocaleTranslationsFactory.ForLanguageCode("en-US");

            var languageCodes = localeNames.GetAllLanguageCodes();

            Assert.IsTrue(languageCodes.Any(), "Testing on empty collection does not make sense.");

            /*
             * language code should not provide language variants, only the unique list of language codes
             */

            Assert.IsFalse(languageCodes.Any(i => i.Contains("-alt-variant")));
            Assert.IsFalse(languageCodes.Any(i => i.Contains("-alt-long")));
            Assert.IsFalse(languageCodes.Any(i => i.Contains("-alt-menu")));
            Assert.IsFalse(languageCodes.Any(i => i.Contains("-alt-short")));
        }

        /// <summary>
        /// Finds the language code by name.
        /// </summary>
        [TestMethod]
        public void Find_language_code_by_name()
        {
            var localeNames = LocaleTranslationsFactory.ForLanguageCode("en-US");

            Assert.AreEqual("de", localeNames.FindLanguageCode("German"));

            Assert.AreEqual("cs", localeNames.FindLanguageCode("Czech"));

            Assert.AreEqual("en-US", localeNames.FindLanguageCode("American English"));

            Assert.AreEqual("und", localeNames.FindLanguageCode("Unknown language"));

            //TODO: Shouldn't we expect 'und'?
            Assert.AreEqual(null, localeNames.FindLanguageCode("Value not in dictionary"));
        }
    }
}
