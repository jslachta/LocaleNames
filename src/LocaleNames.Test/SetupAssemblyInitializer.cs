using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Globalization;
using System.Threading;

namespace LocaleNames.Test
{
    /// <summary>
    /// AssemblyInitializer
    /// </summary>
    [TestClass]
    public class SetupAssemblyInitializer
    {
        /// <summary>
        /// The Assembly initialization.
        /// </summary>
        /// <param name="context">The context.</param>
        [AssemblyInitialize]
        public static void AssemblyInit(TestContext context)
        {
            var cultureInfo = new CultureInfo("cs-CZ");

            Thread.CurrentThread.CurrentCulture = cultureInfo;
            Thread.CurrentThread.CurrentUICulture = cultureInfo;

            CultureInfo.CurrentCulture = cultureInfo;
            CultureInfo.CurrentUICulture = cultureInfo;
            CultureInfo.DefaultThreadCurrentCulture = cultureInfo;
            CultureInfo.DefaultThreadCurrentUICulture = cultureInfo;
        }
    }
}
