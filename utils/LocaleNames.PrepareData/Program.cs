using LocaleNames.Utils;
using System.Text.Json;
using LocaleNames.PrepareData.Model;
using Spectre.Console;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Net.Http;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace LocaleNames.PrepareData;

class Program
{
    const string ResourceExtension = ".lnb";

    // the release is a tag of cldr-json, kept in a file so that the cldr-update workflow can raise it
    static string CldrVersion
        => File.ReadAllText(Path.Combine(RepositoryRoot, "utils", "LocaleNames.PrepareData", "cldr-version.txt")).Trim();

    static string DownloadSite
        => $"https://github.com/unicode-org/cldr-json/archive/refs/tags/{CldrVersion}.zip";

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

    // the build output lies at a depth that depends on the configuration, so the root is found by its solution file
    public static string RepositoryRoot
    {
        get
        {
            for (var directory = new DirectoryInfo(AssemblyDirectory); directory != null; directory = directory.Parent)
            {
                if (File.Exists(Path.Combine(directory.FullName, "LocaleNames.slnx")))
                {
                    return directory.FullName;
                }
            }

            throw new InvalidOperationException("The repository root (LocaleNames.slnx) was not found above " + AssemblyDirectory + ".");
        }
    }

    public static string ResourceTargets
    {
        get => Path.Combine(RepositoryRoot, "src", "LocaleNames.Data", "Resources.include");
    }

    public static string ResourceDirectory
    {
        get => Path.Combine(RepositoryRoot, "src", "LocaleNames.Data", "Resources");
    }

    static async Task Main(string[] args)
    {
        AnsiConsole.MarkupLine("[bold]LocaleNames data preparation[/]");
        AnsiConsole.MarkupLineInterpolated($"CLDR [blue]{DownloadSite}[/]");
        AnsiConsole.MarkupLineInterpolated($"Target [blue]{ResourceDirectory}[/]");
        AnsiConsole.WriteLine();

        await CreateProgress().StartAsync(async context =>
        {
            await DownloadData(context.AddTask("Downloading CLDR data"));
            ExtractData(context.AddTask("Extracting CLDR data"));
        });

        var count = 0;

        await CreateProgress().StartAsync(context =>
        {
            count = PrepareLocaleData(context.AddTask("Preparing LocaleNames resources"));

            return Task.CompletedTask;
        });

        AnsiConsole.WriteLine();
        AnsiConsole.MarkupLineInterpolated($"[green]Done:[/] {count} resource files written.");
    }

    static Progress CreateProgress()
        => AnsiConsole.Progress()
            .AutoClear(false)
            .Columns(
                new TaskDescriptionColumn { Alignment = Justify.Left },
                new ProgressBarColumn(),
                new PercentageColumn(),
                new ElapsedTimeColumn(),
                new SpinnerColumn());

    static async Task DownloadData(ProgressTask progress)
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
        using var response = await client.GetAsync(DownloadSite, HttpCompletionOption.ResponseHeadersRead);

        response.EnsureSuccessStatusCode();

        // the size is not always announced, the bar then only pulses
        var total = response.Content.Headers.ContentLength;

        progress.IsIndeterminate = total == null;
        progress.MaxValue = total ?? 1;

        await using var source = await response.Content.ReadAsStreamAsync();
        await using var target = File.Create(ArchivePath);

        var buffer = new byte[81920];
        int read;

        while ((read = await source.ReadAsync(buffer)) > 0)
        {
            await target.WriteAsync(buffer.AsMemory(0, read));

            progress.Increment(total == null ? 0 : read);
        }

        progress.IsIndeterminate = false;
        progress.Value = progress.MaxValue;
    }

    static void ExtractData(ProgressTask progress)
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
                && (i.FullName.Contains("languages") || i.FullName.Contains("territories") || i.FullName.Contains("currencies")))
            .ToList();

        progress.MaxValue = Math.Max(zipEntries.Count, 1);

        foreach (var entry in zipEntries)
        {
            progress.Increment(1);

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

    private static int PrepareLocaleData(ProgressTask progress)
    {
        if (Directory.Exists(ResourceDirectory))
        {
            Directory.Delete(ResourceDirectory, true);
        }

        Directory.CreateDirectory(ResourceDirectory);

        var written = 0;
        var files = Directory.GetFiles(UnpackedArchivePath);

        progress.MaxValue = Math.Max(files.Length, 1);

        foreach (var filePath in files)
        {
            progress.Increment(1);

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
            var cldrContainer = JsonSerializer.Deserialize<CldrContainer>(json);

            if (sourceFilename.Contains("territories"))
            {
                targetDictionary = cldrContainer.Main.Data.LocaleDisplayNames.Territories;

                targetFilename = $"language.{sourceFilenameComponents[1]}.territories";
            }
            else if (sourceFilename.Contains("languages"))
            {
                targetDictionary = cldrContainer.Main.Data.LocaleDisplayNames.Languages;

                targetFilename = $"language.{sourceFilenameComponents[1]}.languages";
            }
            else if (sourceFilename.Contains("currencies"))
            {
                targetDictionary = FlattenCurrencies(cldrContainer.Main.Data.Numbers?.Currencies);

                targetFilename = $"language.{sourceFilenameComponents[1]}.currencies";
            }

            if (targetDictionary == null)
            {
                continue;
            }

            string targetFilePath = Path.Combine(
              ResourceDirectory,
              targetFilename + ResourceExtension
              );

            File.WriteAllBytes(targetFilePath, LocaleResourceFormat.Serialize(targetDictionary));

            written++;
        }

        return written;
    }

    /// <summary>
    /// Flattens the CLDR currencies into a single dictionary.
    /// The display name is stored under the currency code (e.g. "CZK"),
    /// other values under the currency code with suffix
    /// (e.g. "CZK-count-few", "CZK-symbol", "CZK-symbol-alt-narrow").
    /// Formatting values (decimal, group, pattern) are skipped.
    /// </summary>
    private static Dictionary<string, string> FlattenCurrencies(Dictionary<string, Dictionary<string, string>> currencies)
    {
        Dictionary<string, string> result = new();

        if (currencies == null)
        {
            return result;
        }

        foreach (var currency in currencies)
        {
            foreach (var value in currency.Value)
            {
                if (value.Key == "displayName")
                {
                    result[currency.Key] = value.Value;
                }
                else if (value.Key.StartsWith("displayName-count-"))
                {
                    result[$"{currency.Key}-{value.Key["displayName-".Length..]}"] = value.Value;
                }
                else if (value.Key.StartsWith("symbol"))
                {
                    result[$"{currency.Key}-{value.Key}"] = value.Value;
                }
            }
        }

        return result;
    }
}
