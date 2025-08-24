using Fluens.AppCore.Enums;

namespace Fluens.AppCore.Contracts;
public interface ILocalSettingService : IDisposable
{
    IObservable<OnStartupSetting> OnStartupSettingChanges { get; }
    void SetStartupConfig(OnStartupSetting onStartupSetting);
}