using Fluens.AppCore.Contracts;
using Fluens.AppCore.ViewModels;

namespace Fluens.UI.Services;
internal class TabViewsManager(WindowsManager windowsManager) : ITabPageManager
{
    public ITabPage GetParentTabPage(AppTabViewModel tab)
    {
        return windowsManager.GetParentTabPage(tab);
    }
}
