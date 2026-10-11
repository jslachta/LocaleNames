using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Media;
using Avalonia.Platform.Storage;
using LocaleNames.Utils;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace LocaleNames.Viewer
{
    public partial class MainWindow : Window
    {
        private List<LnbRow> _rows = new();
        private string _fileDescription = string.Empty;

        public MainWindow()
            : this(null)
        {
        }

        public MainWindow(string? path)
        {
            InitializeComponent();

            OpenButton.Click += async (_, _) => await PickFileAsync();
            FilterBox.TextChanged += (_, _) => ApplyFilter();
            AddHandler(DragDrop.DropEvent, OnDrop);

            if (!string.IsNullOrEmpty(path))
            {
                Opened += (_, _) => LoadFile(path);
            }
        }

        private async Task PickFileAsync()
        {
            var files = await StorageProvider.OpenFilePickerAsync(new FilePickerOpenOptions
            {
                Title = "Open a LocaleNames resource",
                AllowMultiple = false,
                FileTypeFilter = new[]
                {
                    new FilePickerFileType("LocaleNames resource") { Patterns = new[] { "*.lnb" } },
                    FilePickerFileTypes.All,
                },
            });

            var path = files.FirstOrDefault()?.TryGetLocalPath();

            if (path != null)
            {
                LoadFile(path);
            }
        }

        private void OnDrop(object? sender, DragEventArgs e)
        {
            var path = e.DataTransfer.TryGetFiles()?.FirstOrDefault()?.TryGetLocalPath();

            if (path != null)
            {
                LoadFile(path);
            }
        }

        private void LoadFile(string path)
        {
            try
            {
                var size = new FileInfo(path).Length;

                Dictionary<string, string> values;

                using (var stream = File.OpenRead(path))
                {
                    values = LocaleResourceFormat.Deserialize(stream);
                }

                var index = 0;

                _rows = values.Select(i => new LnbRow(index++, i.Key, i.Value)).ToList();
                _fileDescription = Describe(path, size, _rows.Count);

                InfoText.ClearValue(TextBlock.ForegroundProperty);
                FilterBox.IsEnabled = true;
                FilterBox.Text = string.Empty;
                Title = $"{Path.GetFileName(path)} - LocaleNames viewer";

                ApplyFilter();
            }
            catch (Exception ex) when (ex is IOException or InvalidDataException or UnauthorizedAccessException)
            {
                _rows = new List<LnbRow>();
                Grid.ItemsSource = _rows;
                FilterBox.IsEnabled = false;
                InfoText.Foreground = Brushes.IndianRed;
                InfoText.Text = $"{Path.GetFileName(path)}: {ex.Message}";
                StatusText.Text = string.Empty;
            }
        }

        private void ApplyFilter()
        {
            var filter = FilterBox.Text?.Trim();

            var shown = string.IsNullOrEmpty(filter)
                ? _rows
                : _rows.Where(i =>
                    i.Key.Contains(filter, StringComparison.OrdinalIgnoreCase)
                    || i.Value.Contains(filter, StringComparison.OrdinalIgnoreCase)).ToList();

            Grid.ItemsSource = shown;
            InfoText.Text = _fileDescription;
            StatusText.Text = shown.Count == _rows.Count
                ? $"{_rows.Count:N0} entries"
                : $"{shown.Count:N0} of {_rows.Count:N0} entries";
        }

        /// <summary>
        /// "language.cs.territories.lnb" is described as "cs · territories".
        /// </summary>
        private static string Describe(string path, long size, int count)
        {
            var parts = Path.GetFileNameWithoutExtension(path).Split('.');
            var what = parts.Length == 3 && parts[0] == "language"
                ? $"{parts[1]} · {parts[2]}"
                : Path.GetFileName(path);

            return $"{what}  —  {count:N0} entries, {size:N0} bytes on disk";
        }
    }
}
