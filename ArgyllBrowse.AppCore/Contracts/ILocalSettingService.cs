using ArgyllBrowse.AppCore.Enums;

namespace ArgyllBrowse.AppCore.Contracts;
public interface ILocalSettingService : IDisposable
{
    IObservable<OnStartupSetting> OnStartupSettingChanges { get; }
    void SetStartupConfig(OnStartupSetting onStartupSetting);
}