using ArgyllBrowse.AppCore.Enums;
using ArgyllBrowse.AppCore.Helpers;
using ArgyllBrowse.AppCore.ViewModels.Settings.OnStartup;
using Microsoft.UI.Xaml.Controls;
using ReactiveUI;
using System.Reactive.Linq;
using WinRT;

namespace ArgyllBrowse.UI.Views.Settings.OnStartup;

public sealed partial class OnStartupConfig : UserControl
{
    public OnStartupConfigViewModel ViewModel { get; set; }
    public OnStartupConfig()
    {
        InitializeComponent();

        ViewModel = ServiceLocator.GetRequiredService<OnStartupConfigViewModel>();

        foreach (OnStartupSetting onStartupSetting in Enum.GetValues<OnStartupSetting>())
        {
            OnStartupRBtn.Items.Add(new OnStartupSettingItem(onStartupSetting));
        }

        ViewModel.OnStartupSettingChanges
            .Subscribe(onStartupSetting =>
            {
                OnStartupRBtn.SelectedItem = OnStartupRBtn.Items.Cast<OnStartupSettingItem>().Single(i => i.OnStartupSetting == onStartupSetting);
            });

        Observable.FromEventPattern<SelectionChangedEventArgs>(OnStartupRBtn, nameof(OnStartupRBtn.SelectionChanged))
            .Skip(1)
            .Select(ep => ep.EventArgs.AddedItems.SingleOrDefault())
            .WhereNotNull()
            .Select(i => i.As<OnStartupSettingItem>())
            .Subscribe(i => ViewModel.SetOnStartupSetting(i.OnStartupSetting));
    }
}

internal class OnStartupSettingItem(OnStartupSetting onStartupSetting)
{
    public OnStartupSetting OnStartupSetting => onStartupSetting;
    public override string ToString()
    {
        return onStartupSetting switch
        {
            OnStartupSetting.OpenNewTab => "Open the New tab page",
            OnStartupSetting.RestoreOpenTabs => "Continue where you left off",
            OnStartupSetting.RestoreAndOpenNewTab => "Continue where you left off and open a New Tab page",
            _ => throw new NotImplementedException()
        };
    }
}
