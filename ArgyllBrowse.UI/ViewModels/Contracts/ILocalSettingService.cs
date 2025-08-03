using ArgyllBrowse.UI.Enums;
using System;

namespace ArgyllBrowse.UI.ViewModels.Contracts;
public interface ILocalSettingService : IDisposable
{
    IObservable<OnStartupSetting> OnStartupSettingChanges { get; }
    void SetStartupConfig(OnStartupSetting onStartupSetting);
}