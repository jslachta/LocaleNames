using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Globalization;

namespace LocaleNames.Test
{
    /// <summary>
    /// LocaleNames tests.
    /// </summary>
    [TestClass]
    public class LocaleNamesTests
    {
        /// <summary>
        /// Check whether the translations are loaded from cache.
        /// </summary>
        [TestMethod]
        public void Is_loading_from_cache()
        {
            LocaleTranslationsFactory.ClearCache();
            var localeName = LocaleTranslationsFactory.ForCultureInfo(new CultureInfo("cs-CZ"));

            Assert.IsFalse(localeName.IsFromCache);

            localeName = LocaleTranslationsFactory.ForCultureInfo(new CultureInfo("cs-CZ"));

            Assert.IsTrue(localeName.IsFromCache);
        }
    }
}
