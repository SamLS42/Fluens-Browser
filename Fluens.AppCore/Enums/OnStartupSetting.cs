using System.ComponentModel;

namespace Fluens.AppCore.Enums;

public enum OnStartupSetting
{
    [Description("Open the New tab page")]
    OpenNewTab,
    [Description("Continue where you left off")]
    RestoreOpenTabs,
    //OpenSpecificTabs,
    [Description("Continue where you left off and open a New Tab page")]
    RestoreAndOpenNewTab
}
