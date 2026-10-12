using Avalonia;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Markup.Xaml;
using System.Linq;

namespace LocaleNames.Viewer
{
    public class App : Application
    {
        public override void Initialize() => AvaloniaXamlLoader.Load(this);

        public override void OnFrameworkInitializationCompleted()
        {
            if (ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop)
            {
                // a file passed on the command line (or "open with") is shown right away
                desktop.MainWindow = new MainWindow(desktop.Args?.FirstOrDefault());
            }

            base.OnFrameworkInitializationCompleted();
        }
    }
}
