using ArgyllBrowse.UI.Enums;
using ArgyllBrowse.UI.ViewModels.Contracts;
using System;
using System.Reactive.Linq;
using System.Reactive.Subjects;
using Windows.Storage;

namespace ArgyllBrowse.UI.Services;

public partial class LocalSettingService : ILocalSettingService
{
    public IObservable<OnStartupSetting> OnStartupSettingChanges => _onStartupSettingChanges.AsObservable();

    private const OnStartupSetting defaultOnStartupSetting = OnStartupSetting.OpenNewTab;

    public LocalSettingService()
    {
        if (GetStartupConfig() is OnStartupSetting savedSetting)
        {
            _onStartupSettingChanges = new(savedSetting);
        }
        else
        {
            _onStartupSettingChanges = new(defaultOnStartupSetting);
            SetStartupConfig(defaultOnStartupSetting);
        }
    }

    private const string OnStartupSettingKey = "OnStartupSetting";
    private static readonly ApplicationDataContainer localSettings = ApplicationData.Current.LocalSettings;
    private readonly BehaviorSubject<OnStartupSetting> _onStartupSettingChanges;

    private OnStartupSetting? GetStartupConfig()
    {
        if (localSettings.Values.TryGetValue(OnStartupSettingKey, out object? rawSetting))
        {
            if (Enum.TryParse(rawSetting.ToString(), out OnStartupSetting parsedValue))
            {
                return parsedValue;
            }
        }

        return null;
    }

    public void SetStartupConfig(OnStartupSetting onStartupSetting)
    {
        localSettings.Values[OnStartupSettingKey] = onStartupSetting.ToString();
        _onStartupSettingChanges.OnNext(GetStartupConfig()!.Value);
    }

    public void Dispose()
    {
        Dispose(true);
        GC.SuppressFinalize(this);
    }

    protected virtual void Dispose(bool dispose)
    {
        _onStartupSettingChanges.OnCompleted();
        _onStartupSettingChanges.Dispose();
    }
}
