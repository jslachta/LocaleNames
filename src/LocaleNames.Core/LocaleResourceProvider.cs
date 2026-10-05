using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;

namespace LocaleNames
{
    /// <summary>
    /// Finds the embedded CLDR resources (<c>language.{locale}.{type}.json.gz</c>) the translations are loaded from.
    /// </summary>
    /// <remarks>
    /// The resources are looked up in, in this order:
    /// <list type="number">
    /// <item>assemblies passed to <see cref="Register"/>,</item>
    /// <item>loaded assemblies marked with <c>[assembly: AssemblyMetadata("LocaleNames.Resources", "true")]</c>
    /// (done automatically by the LocaleNames.Embed package),</item>
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

        private static readonly object Sync = new();
        private static readonly List<Assembly> RegisteredAssemblies = new();

        private static readonly ConcurrentDictionary<Assembly, string[]> ResourceNames = new();
        private static readonly ConcurrentDictionary<Assembly, bool> MarkedAssemblies = new();

        private static readonly Lazy<Assembly> DataAssembly = new(LoadDataAssembly, true);

        /// <summary>
        /// Registers an assembly with embedded locale resources, so it is searched before any other one.
        /// </summary>
        /// <param name="assembly">The assembly.</param>
        public static void Register(Assembly assembly)
        {
            if (assembly == null)
            {
                throw new ArgumentNullException(nameof(assembly));
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
        /// Opens the resource stream for the given file name, e.g. <c>language.cs.languages.json.gz</c>.
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

        private static Stream TryOpen(Assembly assembly, string fileName)
        {
            var names = ResourceNames.GetOrAdd(assembly, a => a.GetManifestResourceNames());

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
            catch (Exception)
            {
                return null;
            }
        }
    }
}
