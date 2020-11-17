using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Globalization;
using LocaleNames;

namespace LocaleNames.Test
{
    [TestClass]
    public class FindLanguageCodeTests
    {
        [TestMethod]
        public void LocaleNames_Find_language_code_by_name()
        {
            var localeNames = LocaleTranslations.ForLanguageCode("en-US");

            Assert.AreEqual(localeNames.FindLanguageCode("German"), "de");

            Assert.AreEqual(localeNames.FindLanguageCode("Czech"), "cs");

            Assert.AreEqual(localeNames.FindLanguageCode("American English"), "en-US");

            Assert.AreEqual(localeNames.FindLanguageCode("Unknown language"), "und");

            //TODO: Shouldn't we expect 'und'?
            Assert.AreEqual(localeNames.FindLanguageCode("Value not in dictionary"), null);
        }
    }
}
