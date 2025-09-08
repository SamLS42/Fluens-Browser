using Fluens.AppCore.ViewModels;
using ReactiveUI;

namespace Fluens.AppCore.Contracts;

public interface ITabPageManager
{
    IViewFor<AppPageViewModel> GetParentTabPage(AppTabViewModel tab);
}