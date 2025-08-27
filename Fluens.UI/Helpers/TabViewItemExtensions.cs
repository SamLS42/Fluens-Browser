using Fluens.AppCore.ViewModels;
using Fluens.UI.Helpers;
using Fluens.UI.Views;
using Microsoft.UI.Xaml.Controls;
using WinRT;

namespace Fluens.UI.Helpers;
internal static class TabViewItemExtensions
{
    extension(TabViewItem tvi)
    {
        public AppTabViewModel ViewModel => tvi.Content.As<AppTab>().ViewModel!;
    }
}
