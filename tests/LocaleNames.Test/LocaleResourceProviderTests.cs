using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;

namespace LocaleNames.Test
{
    /// <summary>
    /// LocaleResourceProvider tests.
    /// </summary>
    [TestClass]
    public class LocaleResourceProviderTests
    {
        private const string ExistingFile = "language.cs.territories.lnb";

        private static Assembly DataAssembly => Assembly.Load(LocaleResourceProvider.DataAssemblyName);

        /// <summary>
        /// The registrations are static, a test must not see the ones of another test.
        /// </summary>
        [TestCleanup]
        public void Cleanup()
            => LocaleResourceProvider.ClearRegistrations();

        /// <summary>
        /// An assembly whose attributes cannot be read.
        /// </summary>
        private sealed class UnreadableAssembly : Assembly
        {
            public override object[] GetCustomAttributes(bool inherit) => throw new NotSupportedException();

            public override object[] GetCustomAttributes(Type attributeType, bool inherit) => throw new NotSupportedException();
        }

        /// <summary>
        /// An assembly with the given resource names.
        /// </summary>
        private sealed class FakeAssembly : Assembly
        {
            private readonly string[] names;

            public FakeAssembly(params string[] names) => this.names = names;

            public override AssemblyName GetName() => new AssemblyName("Fake");

            public override string[] GetManifestResourceNames() => names;

            public override Stream GetManifestResourceStream(string name) => new MemoryStream(new byte[] { 1, 2, 3 });
        }

        /// <summary>
        /// Only a language resource of the .lnb type counts, with or without a root namespace.
        /// </summary>
        [TestMethod]
        public void HasLocaleResources_RecognizesLanguageResources()
        {
            Assert.IsTrue(LocaleResourceProvider.HasLocaleResources(new FakeAssembly("language.cs.territories.lnb")));
            Assert.IsTrue(LocaleResourceProvider.HasLocaleResources(new FakeAssembly("Root.Namespace.language.cs.territories.lnb")));
            Assert.IsFalse(LocaleResourceProvider.HasLocaleResources(new FakeAssembly("language.cs.territories.txt")));
            Assert.IsFalse(LocaleResourceProvider.HasLocaleResources(new FakeAssembly("territories.lnb", "Root.territories.lnb")));
            Assert.IsFalse(LocaleResourceProvider.HasLocaleResources(new FakeAssembly()));
        }

        /// <summary>
        /// An assembly with resources named without a root namespace can be registered and opened.
        /// </summary>
        [TestMethod]
        public void Open_AssemblyWithResourcesWithoutNamespace_ReturnsStream()
        {
            var fake = new FakeAssembly("language.qq.fake.lnb");

            LocaleResourceProvider.Register(fake);

            using (var stream = LocaleResourceProvider.Open("language.qq.fake.lnb"))
            {
                Assert.IsNotNull(stream);
            }
        }

        /// <summary>
        /// A registered assembly that is marked too is offered once.
        /// </summary>
        [TestMethod]
        public void GetCandidateAssemblies_RegisteredMarkedAssembly_IsOfferedOnce()
        {
            var own = typeof(LocaleResourceProviderTests).Assembly;

            LocaleResourceProvider.Register(own);

            Assert.AreEqual(1, LocaleResourceProvider.GetCandidateAssemblies().Count(i => i == own));
        }

        /// <summary>
        /// Without the data assembly the others are still offered.
        /// </summary>
        [TestMethod]
        public void GetCandidateAssemblies_NoDataAssembly_IsNotOffered()
        {
            Assert.IsFalse(LocaleResourceProvider.IsMarked(DataAssembly), "The data assembly must not be marked, it would be offered as one.");
            Assert.IsFalse(LocaleResourceProvider.GetCandidateAssemblies(() => null).Contains(DataAssembly));
        }

        /// <summary>
        /// The data assembly is loaded.
        /// </summary>
        [TestMethod]
        public void LoadDataAssembly_Existing_ReturnsAssembly()
        {
            var assembly = LocaleResourceProvider.LoadDataAssembly(LocaleResourceProvider.DataAssemblyName, out var error);

            Assert.IsNotNull(assembly);
            Assert.IsNull(error);
        }

        /// <summary>
        /// A data assembly that cannot be loaded is not an error by itself, the reason is kept for the message.
        /// </summary>
        [TestMethod]
        public void LoadDataAssembly_Missing_ReturnsNullAndError()
        {
            var assembly = LocaleResourceProvider.LoadDataAssembly("LocaleNames.Test.NotExisting", out var error);

            Assert.IsNull(assembly);
            StringAssert.StartsWith(error, "FileNotFoundException: ");
        }

        /// <summary>
        /// A null assembly is rejected.
        /// </summary>
        [TestMethod]
        public void Register_Null_Throws()
        {
            var error = Assert.ThrowsExactly<ArgumentNullException>(() => LocaleResourceProvider.Register(null));

            Assert.AreEqual("assembly", error.ParamName);
        }

        /// <summary>
        /// An assembly without the resources is reported at once.
        /// </summary>
        [TestMethod]
        public void Register_AssemblyWithoutResources_Throws()
        {
            var error = Assert.ThrowsExactly<ArgumentException>(() => LocaleResourceProvider.Register(typeof(string).Assembly));

            Assert.AreEqual("assembly", error.ParamName);
            StringAssert.Contains(error.Message, "contains no LocaleNames resources");
            StringAssert.Contains(error.Message, "LocaleNamesLocales");
            StringAssert.Contains(error.Message, "https://github.com/jslachta/LocaleNames#selecting-locales");
        }

        /// <summary>
        /// A registered assembly is offered only once even if registered repeatedly.
        /// </summary>
        [TestMethod]
        public void Register_AssemblyWithResources_IsOfferedOnce()
        {
            var data = DataAssembly;

            LocaleResourceProvider.Register(data);
            LocaleResourceProvider.Register(data);

            Assert.AreEqual(1, LocaleResourceProvider.GetCandidateAssemblies().Count(i => i == data));
        }

        /// <summary>
        /// Registration drops the cached translations, so the new assembly is taken into account.
        /// </summary>
        [TestMethod]
        public void Register_ClearsCache()
        {
            var before = LocaleTranslationsFactory.ForLanguageCode("cs");

            LocaleResourceProvider.Register(DataAssembly);

            Assert.AreNotSame(before, LocaleTranslationsFactory.ForLanguageCode("cs"));
        }

        /// <summary>
        /// Registered assemblies, the marked ones and the data assembly are all offered.
        /// </summary>
        [TestMethod]
        public void GetCandidateAssemblies_OffersAllSources()
        {
            var registered = new FakeAssembly("language.qq.registered.lnb");

            LocaleResourceProvider.Register(registered);

            var candidates = LocaleResourceProvider.GetCandidateAssemblies().ToList();

            CollectionAssert.Contains(candidates, registered);
            CollectionAssert.Contains(candidates, typeof(LocaleResourceProviderTests).Assembly);
            CollectionAssert.Contains(candidates, DataAssembly);
        }

        /// <summary>
        /// Without a registration the data assembly is still found.
        /// </summary>
        [TestMethod]
        public void GetCandidateAssemblies_ContainsDataAssembly()
        {
            Assert.IsTrue(LocaleResourceProvider.GetCandidateAssemblies().Contains(DataAssembly));
        }

        /// <summary>
        /// Dynamic assemblies are never searched.
        /// </summary>
        [TestMethod]
        public void GetCandidateAssemblies_SkipsDynamicAssemblies()
        {
            var dynamicAssembly = AssemblyBuilder.DefineDynamicAssembly(new AssemblyName("LocaleNames.Test.Dynamic"), AssemblyBuilderAccess.Run);

            Assert.IsFalse(LocaleResourceProvider.GetCandidateAssemblies().Contains(dynamicAssembly));
        }

        /// <summary>
        /// Assemblies without the marker are not searched.
        /// </summary>
        [TestMethod]
        public void GetCandidateAssemblies_SkipsNotMarkedAssemblies()
        {
            Assert.IsFalse(LocaleResourceProvider.GetCandidateAssemblies().Contains(typeof(string).Assembly));
        }

        /// <summary>
        /// The marker is recognized.
        /// </summary>
        [TestMethod]
        public void IsMarked_MarkedAssembly_True()
        {
            Assert.IsTrue(LocaleResourceProvider.IsMarked(typeof(LocaleResourceProviderTests).Assembly));
            Assert.IsFalse(LocaleResourceProvider.IsMarked(typeof(string).Assembly));
        }

        /// <summary>
        /// An assembly whose attributes cannot be read is not marked, it does not fail the lookup.
        /// </summary>
        [TestMethod]
        public void IsMarked_UnreadableAssembly_False()
        {
            Assert.IsFalse(LocaleResourceProvider.IsMarked(new UnreadableAssembly()));
        }

        /// <summary>
        /// An existing resource is opened.
        /// </summary>
        [TestMethod]
        public void Open_ExistingResource_ReturnsStream()
        {
            using (var stream = LocaleResourceProvider.Open(ExistingFile))
            {
                Assert.IsNotNull(stream);
                Assert.IsGreaterThan(0, stream.Length);
            }
        }

        /// <summary>
        /// A missing locale is normal, there is no stream for it.
        /// </summary>
        [TestMethod]
        public void Open_MissingResource_ReturnsNull()
        {
            Assert.IsNull(LocaleResourceProvider.Open("language.zz.territories.lnb"));
        }

        /// <summary>
        /// A candidate without the resource is skipped, the next one is used.
        /// </summary>
        [TestMethod]
        public void Open_SkipsCandidateWithoutResource()
        {
            using (var stream = LocaleResourceProvider.Open(ExistingFile, () => new[] { typeof(string).Assembly, DataAssembly }))
            {
                Assert.IsNotNull(stream);
            }
        }

        /// <summary>
        /// Candidates with data but without the requested file give null, not an error.
        /// </summary>
        [TestMethod]
        public void Open_DataWithoutRequestedFile_ReturnsNull()
        {
            Assert.IsNull(LocaleResourceProvider.Open("language.zz.territories.lnb", () => new[] { DataAssembly }));
        }

        /// <summary>
        /// No data at all is a setup error, not empty translations.
        /// </summary>
        [TestMethod]
        public void Open_NoDataAnywhere_Throws()
        {
            var error = Assert.ThrowsExactly<InvalidOperationException>(
                () => LocaleResourceProvider.Open(ExistingFile, () => new[] { typeof(string).Assembly }));

            StringAssert.Contains(error.Message, "LocaleNames found no locale data");
        }

        /// <summary>
        /// The message lists what was searched, and how to fix the setup.
        /// </summary>
        [TestMethod]
        public void BuildNoDataMessage_DescribesSearchAndFix()
        {
            var message = LocaleResourceProvider.BuildNoDataMessage(new[] { "First", "Second" }, new[] { "Marked" }, null, "FileNotFoundException: missing");

            StringAssert.Contains(message, "LocaleNames found no locale data (language.*.lnb resources)");
            StringAssert.Contains(message, "registered assemblies: First, Second");
            StringAssert.Contains(message, "assemblies marked 'LocaleNames.Resources': Marked");
            StringAssert.Contains(message, "assembly LocaleNames.Data: not available (FileNotFoundException: missing)");
            StringAssert.Contains(message, "How to fix it:");
            StringAssert.Contains(message, "the LocaleNames.Data assembly must be kept");
            StringAssert.Contains(message, "<TrimmerRootAssembly Include=\"LocaleNames.Data\" />");
            StringAssert.Contains(message, "GenerateAssemblyInfo=true");
            StringAssert.Contains(message, "LocaleResourceProvider.Register(assembly)");
            Assert.IsTrue(message.EndsWith("See https://github.com/jslachta/LocaleNames#selecting-locales.", StringComparison.Ordinal));
        }

        /// <summary>
        /// Nothing found and no known reason are said so.
        /// </summary>
        [TestMethod]
        public void BuildNoDataMessage_NothingFound_SaysNone()
        {
            var message = LocaleResourceProvider.BuildNoDataMessage(Array.Empty<string>(), new List<string>(), null, null);

            StringAssert.Contains(message, "registered assemblies: none");
            StringAssert.Contains(message, "assemblies marked 'LocaleNames.Resources': none");
            StringAssert.Contains(message, "not available (unknown reason)");
        }

        /// <summary>
        /// A loaded data assembly without the resources is said so.
        /// </summary>
        [TestMethod]
        public void BuildNoDataMessage_LoadedDataAssembly_SaysItHasNoResources()
        {
            var message = LocaleResourceProvider.BuildNoDataMessage(Array.Empty<string>(), Array.Empty<string>(), typeof(string).Assembly, null);

            StringAssert.Contains(message, "assembly LocaleNames.Data: loaded, but it contains no locale resources");
        }
    }
}
