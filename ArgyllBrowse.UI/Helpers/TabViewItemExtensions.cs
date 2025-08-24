using ArgyllBrowse.AppCore.ViewModels;
using ArgyllBrowse.UI.Views;
using Microsoft.UI.Xaml.Controls;
using WinRT;

namespace ArgyllBrowse.UI.Helpers;
internal static class TabViewItemExtensions
{
    extension(TabViewItem tvi)
    {
        public AppTabViewModel ViewModel => tvi.Content.As<AppTab>().ViewModel!;
    }
}
