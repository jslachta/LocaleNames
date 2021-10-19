using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Globalization;
using LocaleNames;
using System.Linq;

namespace LocaleNames.Test
{
    [TestClass]
    public class FindLanguageCodeTests
    {
        [TestMethod]
        public void LocaleNames_All_Language_Codes_Should_Not_Provide_Variants()
        {
            var localeNames = LocaleTranslations.ForLanguageCode("en-US");

            var languageCodes = localeNames.AllLanguageCodes;

            Assert.IsTrue(languageCodes.Any(), "Testing on empty collection does not make sense.");

            /*
             * language code should not provide language variants, only the unique list of language codes
             */

            Assert.IsFalse(languageCodes.Any(i => i.Contains("-alt-variant")));
            Assert.IsFalse(languageCodes.Any(i => i.Contains("-alt-long")));
            Assert.IsFalse(languageCodes.Any(i => i.Contains("-alt-menu")));
            Assert.IsFalse(languageCodes.Any(i => i.Contains("-alt-short")));
        }

        [TestMethod]
        public void LocaleNames_Find_language_code_by_name()
        {
            var localeNames = LocaleTranslations.ForLanguageCode("en-US");

            Assert.AreEqual("de", localeNames.FindLanguageCode("German"));

            Assert.AreEqual("cs", localeNames.FindLanguageCode("Czech"));

            Assert.AreEqual("en-US", localeNames.FindLanguageCode("American English"));

            Assert.AreEqual("und", localeNames.FindLanguageCode("Unknown language"));

            //TODO: Shouldn't we expect 'und'?
            Assert.AreEqual(null, localeNames.FindLanguageCode("Value not in dictionary"));
        }
    }
}
