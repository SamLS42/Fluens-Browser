using Fluens.AppCore.Services;
using Fluens.AppCore.ViewModels;
using Fluens.UI.Views;
using ReactiveUI;
using System;
using System.Collections.Generic;
using System.Text;

namespace Fluens.UI.Services;

internal class ViewForFactory : IViewForFactory
{
    public IViewFor<AppTabViewModel> CreateAppTab(AppTabViewModel viewModel)
    {
        return new AppTabContent() { ViewModel = viewModel };
    }
}
