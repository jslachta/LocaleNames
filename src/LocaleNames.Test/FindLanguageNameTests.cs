using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Globalization;
using LocaleNames;

namespace LocaleNames.Test
{
    [TestClass]
    public class FindLanguageNameTests
    {

        [TestMethod]
        public void LocaleNames_Find_language_name_for_given_culture()
        {
            var localeNames = LocaleTranslations.ForCultureInfo(new CultureInfo("cs-CZ"));

            Assert.AreEqual(localeNames.FindLanguageName("en-US"), "angličtina (USA)");

            Assert.AreEqual(localeNames.FindLanguageName("wae"), "němčina (walser)");

            Assert.AreEqual(localeNames.FindLanguageName("zh-Hant"), "čínština (tradiční)");

            Assert.AreEqual(localeNames.FindLanguageName("unknown code"), null);

            localeNames = LocaleTranslations.ForCultureInfo(new CultureInfo("yue"));

            Assert.AreEqual(localeNames.FindLanguageName("de"), "德文");

            Assert.AreEqual(localeNames.FindLanguageName("cs"), "捷克文");

            Assert.AreEqual(localeNames.FindLanguageName("en-US"), "英文 (美國)");
        }

        [TestMethod]
        public void LocaleNames_Find_language_name_for_current_culture()
        {
            var localeNames = LocaleTranslations.ForCurrentCulture();

            Assert.AreEqual(localeNames.FindLanguageName("en-US"), "angličtina (USA)");

            Assert.AreEqual(localeNames.FindLanguageName("wae"), "němčina (walser)");

            Assert.AreEqual(localeNames.FindLanguageName("zh-Hant"), "čínština (tradiční)");
        }

        [TestMethod]
        public void LocaleNames_Find_language_name_for_language_code()
        {
            var localeNames = LocaleTranslations.ForLanguageCode("en-US");

            Assert.AreEqual(localeNames.FindLanguageName("de"), "German");

            Assert.AreEqual(localeNames.FindLanguageName("cs"), "Czech");

            Assert.AreEqual(localeNames.FindLanguageName("cs-CZ"), "Czech");

            Assert.AreEqual(localeNames.FindLanguageName("en-US"), "American English");
        }
    }
}
