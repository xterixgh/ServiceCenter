using Avalonia;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Markup.Xaml;
using ServiceCenter.Avalonia.Pages;
using ServiceCenter.Data;
using ServiceCenter.Data.Models;

namespace ServiceCenter.Avalonia;

public partial class App : Application
{
    public static AppDbContext DbContext { get; } = new AppDbContext();
    public static User? CurrentUser { get; set; }

    public override void Initialize()
    {
        AvaloniaXamlLoader.Load(this);
    }

    public override void OnFrameworkInitializationCompleted()
    {
        DbSeed.Ensure(DbContext);

        if (ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop)
        {
            desktop.MainWindow = new MainWindow();
        }

        base.OnFrameworkInitializationCompleted();
    }
}
