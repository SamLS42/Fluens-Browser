using Fluens.AppCore.Contracts;
using Fluens.AppCore.Helpers;
using Fluens.AppCore.Services;
using Fluens.AppCore.ViewModels;
using Fluens.AppCore.ViewModels.Settings;
using Fluens.AppCore.ViewModels.Settings.History;
using Fluens.AppCore.ViewModels.Settings.OnStartup;
using Fluens.Data;
using Fluens.UI.Services;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.UI.Xaml;
using ReactiveUI;
using System.Reactive.Linq;
using WinRT;

namespace Fluens.UI;

public partial class App : Application
{
    private IHost _host = null!;
    private MainWindow _window = null!;

    public App()
    {
        InitializeComponent();

        PlatformRegistrationManager.SetRegistrationNamespaces(RegistrationNamespace.WinUI);

        _host = Host.CreateDefaultBuilder()
            .ConfigureServices((context, services) =>
            {
                // Logging
                services.AddLogging(builder =>
                {
                    builder.AddDebug(); // shows in Debug output
                    builder.SetMinimumLevel(LogLevel.Information);
                });

                services.AddTransient<AppPageViewModel>()
                    .AddTransient<AppTabViewModel>()
                    .AddSingleton<OnStartupConfigViewModel>()
                    .AddTransient<HistoryPageViewModel>()
                    .AddTransient<SettingsViewModel>()
                    .AddSingleton<IWindowsManager, WindowsManager>()
                    .AddSingleton<TabPersistencyService>()
                    .AddSingleton<HistoryService>()
                    .AddSingleton<ILocalSettingService, LocalSettingService>()
                    .AddPooledDbContextFactory<BrowserDbContext>(opts =>
                    {
                        opts.UseSqlite("Data Source=BrowserStorage.db");
                    });
            })
            .Build();


        ServiceLocator.SetLocator(_host.Services);
    }

    protected override async void OnLaunched(LaunchActivatedEventArgs args)
    {
        await _host.StartAsync();

        IDbContextFactory<BrowserDbContext> dbContextFactory = ServiceLocator.GetRequiredService<IDbContextFactory<BrowserDbContext>>();
        using BrowserDbContext dbContext = dbContextFactory.CreateDbContext();

        dbContext.Database.Migrate();

        _window = ServiceLocator.GetRequiredService<IWindowsManager>().CreateWindow().As<MainWindow>();

        //TODO: Save and recover window state (maximized, size, etc.)
        //_window.AppWindow.Presenter.As<OverlappedPresenter>().Maximize();

        ILocalSettingService localSetting = ServiceLocator.GetRequiredService<ILocalSettingService>();

        localSetting.OnStartupSettingChanges.Take(1)
            .Subscribe(async onStartupSetting =>
            {
                await _window.ApplyOnStartupSettingAsync(onStartupSetting);
                _window.Activate();
            });
    }
}
