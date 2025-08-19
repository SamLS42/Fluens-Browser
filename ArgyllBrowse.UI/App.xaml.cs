using ArgyllBrowse.AppCore.Contracts;
using ArgyllBrowse.AppCore.Helpers;
using ArgyllBrowse.AppCore.ViewModels;
using ArgyllBrowse.AppCore.ViewModels.Settings.OnStartup;
using ArgyllBrowse.Data;
using ArgyllBrowse.Data.Services;
using ArgyllBrowse.UI.Services;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.UI.Xaml;
using ReactiveUI;

using System;
using System.Linq;
using System.Reactive.Linq;

namespace ArgyllBrowse.UI;

public partial class App : Application
{
    private MainWindow _window = null!;

    public App()
    {
        InitializeComponent();

        PlatformRegistrationManager.SetRegistrationNamespaces(RegistrationNamespace.WinUI);

        RegisterServices();
    }

    private static void RegisterServices()
    {
        IServiceCollection serviceCollection = new ServiceCollection()
            .AddTransient<AppPageViewModel>()
            .AddTransient<AppTabViewModel>()
            .AddSingleton<OnStartupConfigViewModel>()
            .AddSingleton<WindowsManager>()
            .AddSingleton<BrowserDataService>()
            .AddSingleton<ILocalSettingService, LocalSettingService>()
            .AddPooledDbContextFactory<BrowserDbContext>(opts =>
            {
                opts.UseSqlite("Data Source=BrowserStorage.db");
            });

        ServiceLocator.SetLocator(serviceCollection.BuildServiceProvider());


    }

    protected override void OnLaunched(Microsoft.UI.Xaml.LaunchActivatedEventArgs args)
    {
        IDbContextFactory<BrowserDbContext> dbContextFactory = ServiceLocator.GetRequiredService<IDbContextFactory<BrowserDbContext>>();
        using BrowserDbContext dbContext = dbContextFactory.CreateDbContext();

        dbContext.Database.Migrate();

        _window = ServiceLocator.GetRequiredService<WindowsManager>().CreateWindow();

        //TODO: Save and recover window state (maximized, size, etc.)
        //_window.AppWindow.Presenter.As<OverlappedPresenter>().Maximize();

        ILocalSettingService localSetting = ServiceLocator.GetRequiredService<ILocalSettingService>();

        localSetting.OnStartupSettingChanges.Take(1)
            .Subscribe(onStartupSetting =>
            {
                _window.ApplyOnStartupSetting(onStartupSetting);
                _window.Activate();
            });
    }
}
