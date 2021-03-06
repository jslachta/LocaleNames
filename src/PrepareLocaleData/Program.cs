﻿using LocaleNames.Utils;
using Newtonsoft.Json;
using PrepareLocaleData.Model;
using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Net;
using System.Reflection;
using System.Text;

namespace PrepareLocaleData
{
    class Program
    {
        const string DownloadSite = "https://github.com/unicode-org/cldr-json/releases/download/39.0.0/cldr-39.0.0-json-full.zip";

        static string TempDirectory
        {
            get => Path.Combine(Path.GetTempPath(), "LocaleNames");
        }

        static string UnpackedArchivePath
        {
            get => Path.Combine(TempDirectory, "cldr-data");
        }

        static string ArchivePath
        {
            get => Path.Combine(TempDirectory, "cldr-localenames-full.zip");
        }

        public static string AssemblyDirectory
        {
            get
            {
                string codeBase = Assembly.GetExecutingAssembly().CodeBase;
                UriBuilder uri = new UriBuilder(codeBase);
                string path = Uri.UnescapeDataString(uri.Path);
                return Path.GetDirectoryName(path);
            }
        }

        public static string ResourceTargets
        {
            get => Path.Combine(
                AssemblyDirectory,
                "..", "..", "..", "..",
                "LocaleNames", "Resources.include");
        }

        public static string ResourceDirectory
        {
            get => Path.Combine(
                AssemblyDirectory,
                "..", "..", "..", "..",
                "LocaleNames", "Resources");
        }

        static void Main(string[] args)
        {
            DownloadData();
            ExtractData();
            PrepareLocaleData();

            Console.WriteLine("Hello World!");
        }

        static void DownloadData()
        {
            if (!Directory.Exists(TempDirectory))
            {
                Directory.CreateDirectory(TempDirectory);
            }

            if (File.Exists(ArchivePath))
            {
                File.Delete(ArchivePath);
            }

            using (WebClient client = new WebClient())
            {
                client.DownloadFile(DownloadSite, ArchivePath);
            }
        }

        static void ExtractData()
        {
            using (ZipArchive zip = ZipFile.OpenRead(ArchivePath))
            {
                if (Directory.Exists(UnpackedArchivePath))
                {
                    Directory.Delete(UnpackedArchivePath, true);
                }

                Directory.CreateDirectory(UnpackedArchivePath);

                var zipEntries = zip.Entries
                    .Where(i =>
                        i.FullName.Count(f => f == '/') > 2
                        && i.FullName.EndsWith(".json")
                        && (i.FullName.Contains("languages") || i.FullName.Contains("territories")));

                foreach (var entry in zipEntries)
                {
                    var x = entry.FullName.Split('/');

                    string languageCode = x[2];
                    string filename = x[3];

                    if (!string.IsNullOrWhiteSpace(languageCode)! && !string.IsNullOrWhiteSpace(filename))
                    {
                        string namepart = filename.Split(".")[0];

                        var targetPath = Path.Combine(UnpackedArchivePath, $"{namepart}.{languageCode}.json");

                        entry.ExtractToFile(targetPath, true);

                        var content = File.ReadAllText(targetPath);
                        content = content.Replace("\"" + languageCode + "\": {", "\"data\": {");
                        File.WriteAllText(targetPath, content, Encoding.UTF8);
                    }
                }
            }
        }

        private static void PrepareLocaleData()
        {
            if (Directory.Exists(ResourceDirectory))
            {
                Directory.Delete(ResourceDirectory, true);
            }

            Directory.CreateDirectory(ResourceDirectory);

            StringBuilder sb = new StringBuilder();
            sb.Append("<Project DefaultTargets=\"Build\" xmlns=\"http://schemas.microsoft.com/developer/msbuild/2003\">\n");
            sb.Append("\t<ItemGroup>\n");

            foreach (var filePath in Directory.GetFiles(UnpackedArchivePath))
            {
                /*
                 * Prepare file paths.
                 */
                var sourceFilename = Path.GetFileNameWithoutExtension(filePath).Replace("-", "_");
                var sourceFilenameComponents = sourceFilename.Split(".");
                string targetFilename = string.Empty;

                Dictionary<string, string> targetDictionary = null;

                /*
                 * Load JSON data and deserialize.
                 */
                var json = File.ReadAllText(filePath);
                var cldrContainer = JsonConvert.DeserializeObject<CldrContainer>(json);

                if (sourceFilename.Contains("territories"))
                {
                    targetDictionary = cldrContainer.Main.Data.LocaleDisplayNames.Territories;

                    targetFilename = $"language.{sourceFilenameComponents[1]}.territories.json.gz";
                }
                else if (sourceFilename.Contains("languages"))
                {
                    targetDictionary = cldrContainer.Main.Data.LocaleDisplayNames.Languages;

                    targetFilename = $"language.{sourceFilenameComponents[1]}.languages.json.gz";
                }

                if (targetDictionary == null)
                {
                    continue;
                }
                 
                string targetFilePath = Path.Combine(
                  ResourceDirectory,
                  targetFilename
                  );

                ResourceLocale dict = new ResourceLocale()
                {
                    Values = targetDictionary
                };

                var targetJson = JsonConvert.SerializeObject(dict, Formatting.None);
                File.WriteAllText(targetFilePath, GzipUtils.Compress(targetJson), new UTF8Encoding(false));

                sb.Append($"\t\t<EmbeddedResource Include=\"Resources\\{targetFilename}\" />\n");
            }

            sb.Append("\t</ItemGroup>\n");
            sb.Append("</Project>");

            File.WriteAllText(ResourceTargets, sb.ToString(), new UTF8Encoding(false));
        }
    }
}
