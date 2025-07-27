using ArgyllBrowse.Data;
using ArgyllBrowse.Data.Services;
using ArgyllBrowse.UI.Enums;
using ArgyllBrowse.UI.Helpers;
using ArgyllBrowse.UI.Services;
using ArgyllBrowse.UI.ViewModels;
using ArgyllBrowse.UI.Views;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Internal;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Controls.Primitives;
using Microsoft.UI.Xaml.Data;
using Microsoft.UI.Xaml.Input;
using Microsoft.UI.Xaml.Media;
using Microsoft.UI.Xaml.Navigation;
using Microsoft.UI.Xaml.Shapes;
using ReactiveUI;

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.ApplicationModel;
using Windows.ApplicationModel.Activation;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.Storage;

namespace ArgyllBrowse.UI;

public partial class App : Application
{
    private AppWindow _window = null!;

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
            .AddSingleton<WindowsManager>()
            .AddSingleton<BrowserDataService>()
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

        _window.ApplyOnStartupSetting();

        _window.Activate();
    }
}
