using Fluens.AppCore.ViewModels;
using ReactiveUI;

namespace Fluens.AppCore.Contracts;

public interface ITabPage : IViewFor<AppPageViewModel>
{
    IViewFor<AppTabViewModel> CreateTabViewItem(AppTabViewModel vm);
    bool HasTab(AppTabViewModel tab);
}
