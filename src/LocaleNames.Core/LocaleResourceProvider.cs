using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;

namespace LocaleNames
{
    /// <summary>
    /// Finds the embedded CLDR resources (<c>language.{locale}.{type}.lnb</c>) the translations are loaded from.
    /// </summary>
    /// <remarks>
    /// The resources are looked up in, in this order:
    /// <list type="number">
    /// <item>assemblies passed to <see cref="Register"/>,</item>
    /// <item>loaded assemblies marked with <c>[assembly: AssemblyMetadata("LocaleNames.Resources", "true")]</c>
    /// (done automatically by LocaleNames.Data when locales are selected),</item>
    /// <item>the <c>LocaleNames.Data</c> assembly, if it is available.</item>
    /// </list>
    /// </remarks>
    public static class LocaleResourceProvider
    {
        /// <summary>
        /// Key of the <see cref="AssemblyMetadataAttribute"/> marking an assembly with embedded locale resources.
        /// </summary>
        public const string AssemblyMetadataKey = "LocaleNames.Resources";

        /// <summary>
        /// Name of the assembly with the default data.
        /// </summary>
        public const string DataAssemblyName = "LocaleNames.Data";

        private const string ResourceExtension = ".lnb";
        private const string Documentation = "https://github.com/jslachta/LocaleNames#selecting-locales";

        private static readonly object Sync = new();
        private static readonly List<Assembly> RegisteredAssemblies = new();

        private static readonly ConcurrentDictionary<Assembly, string[]> ResourceNames = new();
        private static readonly ConcurrentDictionary<Assembly, bool> MarkedAssemblies = new();
        private static readonly ConcurrentDictionary<Assembly, bool> AssembliesWithLocaleResources = new();

        private static readonly Lazy<Assembly> DataAssembly = new(LoadDataAssembly, true);

        // why the data assembly could not be loaded, for the message when no data is found
        private static string dataAssemblyError;

        /// <summary>
        /// Registers an assembly with embedded locale resources, so it is searched before any other one.
        /// </summary>
        /// <param name="assembly">The assembly.</param>
        /// <exception cref="ArgumentException">The assembly contains no locale resources.</exception>
        public static void Register(Assembly assembly)
        {
            if (assembly == null)
            {
                throw new ArgumentNullException(nameof(assembly));
            }

            if (!HasLocaleResources(assembly))
            {
                throw new ArgumentException(
                    $"The assembly '{assembly.GetName().Name}' contains no LocaleNames resources (language.*.lnb). "
                    + "Register the assembly the locales are embedded into: set LocaleNamesLocales in its project "
                    + "and reference the LocaleNames.Data package, see " + Documentation + ".",
                    nameof(assembly));
            }

            lock (Sync)
            {
                if (!RegisteredAssemblies.Contains(assembly))
                {
                    RegisteredAssemblies.Add(assembly);
                }
            }

            LocaleTranslationsFactory.ClearCache();
        }

        /// <summary>
        /// Opens the resource stream for the given file name, e.g. <c>language.cs.languages.lnb</c>.
        /// </summary>
        /// <param name="fileName">Name of the resource file.</param>
        /// <returns>The stream, or <c>null</c> if the resource is not found. Caller disposes it.</returns>
        internal static Stream Open(string fileName)
        {
            foreach (var assembly in GetCandidateAssemblies())
            {
                var stream = TryOpen(assembly, fileName);

                if (stream != null)
                {
                    return stream;
                }
            }

            // Not finding a locale is normal (not every culture has data), not finding any data at all is a setup
            // error, which must not look like empty translations.
            if (!GetCandidateAssemblies().Any(HasLocaleResources))
            {
                throw new InvalidOperationException(BuildNoDataMessage());
            }

            return null;
        }

        private static IEnumerable<Assembly> GetCandidateAssemblies()
        {
            Assembly[] registered;

            lock (Sync)
            {
                registered = RegisteredAssemblies.ToArray();
            }

            foreach (var assembly in registered)
            {
                yield return assembly;
            }

            foreach (var assembly in AppDomain.CurrentDomain.GetAssemblies())
            {
                if (!assembly.IsDynamic && HasResources(assembly) && !registered.Contains(assembly))
                {
                    yield return assembly;
                }
            }

            var data = DataAssembly.Value;

            if (data != null)
            {
                yield return data;
            }
        }

        private static bool HasResources(Assembly assembly)
            => MarkedAssemblies.GetOrAdd(assembly, IsMarked);

        private static bool IsMarked(Assembly assembly)
        {
            try
            {
                return assembly
                    .GetCustomAttributes<AssemblyMetadataAttribute>()
                    .Any(i => i.Key == AssemblyMetadataKey);
            }
            catch (Exception)
            {
                return false;
            }
        }

        private static bool HasLocaleResources(Assembly assembly)
            => AssembliesWithLocaleResources.GetOrAdd(
                assembly,
                a => GetResourceNames(a).Any(i => i.EndsWith(ResourceExtension, StringComparison.Ordinal)
                    && (i.Contains(".language.") || i.StartsWith("language.", StringComparison.Ordinal))));

        private static string[] GetResourceNames(Assembly assembly)
            => ResourceNames.GetOrAdd(assembly, a => a.GetManifestResourceNames());

        private static string BuildNoDataMessage()
        {
            Assembly[] registered;

            lock (Sync)
            {
                registered = RegisteredAssemblies.ToArray();
            }

            var marked = AppDomain.CurrentDomain.GetAssemblies()
                .Where(i => !i.IsDynamic && HasResources(i))
                .Select(i => i.GetName().Name)
                .ToList();

            var data = DataAssembly.Value;

            return "LocaleNames found no locale data (language.*.lnb resources), so no translation can be looked up." + Environment.NewLine
                + "Searched:" + Environment.NewLine
                + "  - registered assemblies: " + Describe(registered.Select(i => i.GetName().Name)) + Environment.NewLine
                + "  - assemblies marked '" + AssemblyMetadataKey + "': " + Describe(marked) + Environment.NewLine
                + "  - assembly " + DataAssemblyName + ": "
                + (data != null ? "loaded, but it contains no locale resources" : "not available (" + (dataAssemblyError ?? "unknown reason") + ")") + Environment.NewLine
                + "How to fix it:" + Environment.NewLine
                + "  1. Reference the LocaleNames package (all locales), or LocaleNames.Data." + Environment.NewLine
                + "  2. Published with trimming or native AOT: the " + DataAssemblyName + " assembly must be kept. The package does it in "
                + "build/LocaleNames.Data.targets (TrimmerRootAssembly), so do not exclude the build assets of the package "
                + "(ExcludeAssets/PrivateAssets 'build'), or add <TrimmerRootAssembly Include=\"" + DataAssemblyName + "\" /> to the project." + Environment.NewLine
                + "  3. Selected locales (LocaleNamesLocales): the assembly of the project is found by an assembly attribute, which needs "
                + "GenerateAssemblyInfo=true. Otherwise call LocaleResourceProvider.Register(typeof(SomeTypeInYourAssembly).Assembly) once at startup, "
                + "before the first lookup." + Environment.NewLine
                + "  4. Resources in an assembly of your own: call LocaleResourceProvider.Register(assembly)." + Environment.NewLine
                + "See " + Documentation + ".";
        }

        private static string Describe(IEnumerable<string> names)
        {
            var list = names.ToList();

            return list.Count == 0 ? "none" : string.Join(", ", list);
        }

        private static Stream TryOpen(Assembly assembly, string fileName)
        {
            var names = GetResourceNames(assembly);

            // The name is prefixed by a root namespace, which differs between the assemblies.
            var name = names.FirstOrDefault(i => i.EndsWith(fileName, StringComparison.Ordinal));

            return name == null ? null : assembly.GetManifestResourceStream(name);
        }

        private static Assembly LoadDataAssembly()
        {
            try
            {
                return Assembly.Load(new AssemblyName(DataAssemblyName));
            }
            catch (Exception ex)
            {
                dataAssemblyError = ex.GetType().Name + ": " + ex.Message;

                return null;
            }
        }
    }
}
