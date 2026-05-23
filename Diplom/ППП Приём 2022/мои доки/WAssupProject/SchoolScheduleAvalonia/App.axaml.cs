using Avalonia;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Markup.Xaml;
using SchoolScheduleAvalonia.Services;
using SchoolScheduleAvalonia.ViewModels;
using SchoolScheduleAvalonia.Views;

namespace SchoolScheduleAvalonia;

public partial class App : Application
{
    public override void Initialize()
    {
        AvaloniaXamlLoader.Load(this);
    }

    public override void OnFrameworkInitializationCompleted()
    {
        if (ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop)
        {
            var apiBaseUrl = SettingsLoader.LoadApiBaseUrl();
            var apiClient = new ApiClient(apiBaseUrl);

            desktop.MainWindow = new MainWindow
            {
                DataContext = new MainWindowViewModel(apiClient)
            };
        }

        base.OnFrameworkInitializationCompleted();
    }
}
