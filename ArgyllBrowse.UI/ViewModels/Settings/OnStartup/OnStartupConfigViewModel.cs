using ArgyllBrowse.UI.Enums;
using ArgyllBrowse.UI.ViewModels.Contracts;
using ReactiveUI;
using ReactiveUI.SourceGenerators;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reactive;
using System.Text;
using System.Threading.Tasks;
using Windows.Graphics.Printing;

namespace ArgyllBrowse.UI.ViewModels.Settings.OnStartup;

public partial class OnStartupConfigViewModel : ReactiveObject
{
    public IObservable<OnStartupSetting> OnStartupSettingChanges => LocalSettingService.OnStartupSettingChanges;

    public OnStartupConfigViewModel(ILocalSettingService localSettingService)
    {
        LocalSettingService = localSettingService;
    }

    private ILocalSettingService LocalSettingService { get; }

    public void SetOnStartupSetting(OnStartupSetting onStartupSetting)
    {
        LocalSettingService.SetStartupConfig(onStartupSetting);
    }
}
