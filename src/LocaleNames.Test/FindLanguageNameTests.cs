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

            Assert.AreEqual("čeština", localeNames.FindLanguageName("cs"));

            Assert.AreEqual("čeština", localeNames.FindLanguageName("cs-CZ"));

            Assert.AreEqual("angličtina (USA)", localeNames.FindLanguageName("en-US"));

            Assert.AreEqual("němčina (walser)", localeNames.FindLanguageName("wae"));

            Assert.AreEqual("čínština (tradiční)", localeNames.FindLanguageName("zh-Hant"));

            Assert.AreEqual(null, localeNames.FindLanguageName("unknown code"));

            localeNames = LocaleTranslations.ForCultureInfo(new CultureInfo("yue"));

            Assert.AreEqual("德文", localeNames.FindLanguageName("de"));

            Assert.AreEqual("捷克文", localeNames.FindLanguageName("cs"));

            Assert.AreEqual("英文 (美國)", localeNames.FindLanguageName("en-US"));
        }

        [TestMethod]
        public void LocaleNames_Find_language_name_for_current_culture()
        {
            var localeNames = LocaleTranslations.ForCurrentCulture();

            Assert.AreEqual("angličtina (USA)", localeNames.FindLanguageName("en-US"));

            Assert.AreEqual("němčina (walser)", localeNames.FindLanguageName("wae"));

            Assert.AreEqual("čínština (tradiční)", localeNames.FindLanguageName("zh-Hant"));
        }

        [TestMethod]
        public void LocaleNames_Find_language_name_for_language_code()
        {
            var localeNames = LocaleTranslations.ForLanguageCode("en-US");

            Assert.AreEqual("German", localeNames.FindLanguageName("de"));

            Assert.AreEqual("Czech", localeNames.FindLanguageName("cs"));

            Assert.AreEqual("Czech", localeNames.FindLanguageName("cs-CZ"));

            Assert.AreEqual("American English", localeNames.FindLanguageName("en-US"));

            Assert.AreEqual("Chinese, Cantonese", localeNames.FindLanguageName("yue", Enumerations.AltVariant.Menu));
        }

        [TestMethod]
        public void LocaleNames_Find_language_name_for_unknown_language_code()
        {
            var localeNames = LocaleTranslations.ForLanguageCode("unknown-CODE");

            Assert.AreEqual(null, localeNames.FindLanguageName("de"));
        }
    }
}
