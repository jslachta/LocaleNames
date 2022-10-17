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
            LocaleNamesFactory.ClearCache();
            var localeName = LocaleNamesFactory.ForCultureInfo(new CultureInfo("cs-CZ"));

            Assert.IsFalse(localeName.IsFromCache);

            localeName = LocaleNamesFactory.ForCultureInfo(new CultureInfo("cs-CZ"));

            Assert.IsTrue(localeName.IsFromCache);
        }
    }
}
