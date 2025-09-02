using Fluens.AppCore.Enums;

namespace Fluens.AppCore.Contracts;
public interface IMainWindow
{
    Task ApplyOnStartupSettingAsync(OnStartupSetting onStartupSetting);
}
