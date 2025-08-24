using Fluens.AppCore.Contracts;
using Fluens.AppCore.Enums;
using ReactiveUI;

namespace Fluens.AppCore.ViewModels.Settings.OnStartup;

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
