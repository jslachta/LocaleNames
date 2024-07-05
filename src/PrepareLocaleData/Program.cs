using LocaleNames.Model;
using LocaleNames.Utils;
using Newtonsoft.Json;
using PrepareLocaleData.Model;
using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Net.Http;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace PrepareLocaleData;

class Program
{
    const string DownloadSite = "https://github.com/unicode-org/cldr-json/archive/refs/tags/45.0.0.zip";

    static string TempDirectory
        => Path.Combine(Path.GetTempPath(), "LocaleNames");

    static string UnpackedArchivePath
        => Path.Combine(TempDirectory, "cldr-data");

    static string ArchivePath
        => Path.Combine(TempDirectory, "cldr-localenames-full.zip");

    public static string AssemblyDirectory
    {
        get
        {
            string codeBase = Assembly.GetExecutingAssembly().Location;
            UriBuilder uri = new(codeBase);
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

    static async Task Main(string[] args)
    {
        Console.WriteLine("Downloading data started.");
        await DownloadData();
        Console.WriteLine("Downloading data finished.");

        Console.WriteLine("Extracting data started.");
        ExtractData();
        Console.WriteLine("Extracting data finished.");

        Console.WriteLine("Preparation LocaleNames data started.");
        PrepareLocaleData();
        Console.WriteLine("Preparation LocaleNames data finished.");
    }

    static async Task DownloadData()
    {
        if (!Directory.Exists(TempDirectory))
        {
            Directory.CreateDirectory(TempDirectory);
        }

        if (File.Exists(ArchivePath))
        {
            File.Delete(ArchivePath);
        }

        using HttpClient client = new();
        
        byte[] fileBytes = await client.GetByteArrayAsync(DownloadSite);
        await File.WriteAllBytesAsync(ArchivePath, fileBytes);
    }

    static void ExtractData()
    {
        using ZipArchive zip = ZipFile.OpenRead(ArchivePath);

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
            string languageCode = new DirectoryInfo(Path.GetDirectoryName(entry.FullName)).Name;
            string filename = Path.GetFileName(entry.FullName);

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

    private static void PrepareLocaleData()
    {
        if (Directory.Exists(ResourceDirectory))
        {
            Directory.Delete(ResourceDirectory, true);
        }

        Directory.CreateDirectory(ResourceDirectory);

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

            ResourceLocale dict = new()
            {
                Values = targetDictionary
            };

            var targetJson = JsonConvert.SerializeObject(dict, Formatting.None);
            File.WriteAllText(targetFilePath, GzipUtils.Compress(targetJson), new UTF8Encoding(false));
        }
    }
}
