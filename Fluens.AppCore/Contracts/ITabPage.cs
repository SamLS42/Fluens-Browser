using Fluens.AppCore.ViewModels;
using ReactiveUI;

namespace Fluens.AppCore.Contracts;

public interface ITabPage : IViewFor<AppPageViewModel>
{
    void CreateTabViewItem(AppTabViewModel vm);
    bool HasTab(AppTabViewModel tab);
}
