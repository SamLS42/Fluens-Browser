using Fluens.AppCore.ViewModels;
using Fluens.UI.Views;
using Microsoft.UI.Xaml.Controls;
using System;
using System.Collections.Generic;
using System.Text;
using WinRT;

namespace Fluens.UI.Helpers;

static class TabViewItemExtensions
{
    extension(TabViewItem tabViewItem)
    {
        public AppTabViewModel ViewModel => tabViewItem.Content.As<AppTabContent>().ViewModel!;
    }
}
