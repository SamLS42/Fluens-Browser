using Fluens.AppCore.ViewModels;
using ReactiveUI;

namespace Fluens.AppCore.Services;
public interface IViewForFactory
{
    public IViewFor<AppTabViewModel> CreateAppTab(AppTabViewModel viewModel);
}
