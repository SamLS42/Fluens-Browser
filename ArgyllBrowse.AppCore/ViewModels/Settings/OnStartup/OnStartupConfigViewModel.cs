using ArgyllBrowse.AppCore.Contracts;
using ArgyllBrowse.AppCore.Enums;
using ReactiveUI;

namespace ArgyllBrowse.AppCore.ViewModels.Settings.OnStartup;

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
