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

            Assert.AreEqual("de", localeNames.FindLanguageCode("German"));

            Assert.AreEqual("cs", localeNames.FindLanguageCode("Czech"));

            Assert.AreEqual("en-US", localeNames.FindLanguageCode("American English"));

            Assert.AreEqual("und", localeNames.FindLanguageCode("Unknown language"));

            //TODO: Shouldn't we expect 'und'?
            Assert.AreEqual(null, localeNames.FindLanguageCode("Value not in dictionary"));
        }
    }
}
