using Fluens.AppCore.Contracts;
using Fluens.AppCore.ViewModels;
using ReactiveUI;

namespace Fluens.UI.Services;

internal class TabViewsManager(WindowsManager windowsManager) : ITabPageManager
{
    public IViewFor<AppPageViewModel> GetParentTabPage(AppTabViewModel tab)
    {
        return windowsManager.GetParentTabPage(tab);
    }
}
