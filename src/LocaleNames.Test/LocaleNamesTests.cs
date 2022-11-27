using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Globalization;
using LocaleNames;

namespace LocaleNames.Test
{
    [TestClass]
    public class LocaleNamesTests
    {
        [TestMethod]
        public void LocaleNames_Is_loading_from_cache()
        {
            LocaleTranslationsFactory.ClearCache();
            var localeName = LocaleTranslationsFactory.ForCultureInfo(new CultureInfo("cs-CZ"));

            Assert.IsFalse(localeName.IsFromCache);

            localeName = LocaleTranslationsFactory.ForCultureInfo(new CultureInfo("cs-CZ"));

            Assert.IsTrue(localeName.IsFromCache);
        }
    }
}
