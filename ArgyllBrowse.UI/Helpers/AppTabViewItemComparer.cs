using ArgyllBrowse.UI.Helpers;
using Microsoft.UI.Xaml.Controls;
using System.Diagnostics.CodeAnalysis;
using WinRT;

namespace ArgyllBrowse.UI.Views;

public sealed partial class AppPage
{
    private class AppTabViewItemComparer : IEqualityComparer<TabViewItem>
    {
        private static readonly Lazy<AppTabViewItemComparer> lazy = new(() => new AppTabViewItemComparer());

        public static AppTabViewItemComparer Instance => lazy.Value;

        public bool Equals(TabViewItem? x, TabViewItem? y)
        {
            if (ReferenceEquals(x, y))
            {
                return true;
            }

            if (x is null || y is null)
            {
                return false;
            }

            return x.Content.As<AppTab>().ViewModel!.Id == y.Content.As<AppTab>().ViewModel!.Id;
        }

        public int GetHashCode([DisallowNull] TabViewItem obj)
        {
            return obj.Content.As<AppTab>().ViewModel!.Id.GetHashCode();
        }
    }
}
