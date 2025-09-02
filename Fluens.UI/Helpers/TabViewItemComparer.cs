using Microsoft.UI.Xaml.Controls;
using System.Diagnostics.CodeAnalysis;

namespace Fluens.UI.Helpers;

public class TabViewItemComparer : IEqualityComparer<TabViewItem>
{
    private static readonly Lazy<TabViewItemComparer> lazy = new(() => new TabViewItemComparer());
    public static TabViewItemComparer Instance => lazy.Value;

    public bool Equals(TabViewItem? x, TabViewItem? y)
    {
        if (x is null || y is null)
        {
            return false;
        }

        return x.ViewModel.Id == y.ViewModel.Id;
    }

    public int GetHashCode([DisallowNull] TabViewItem obj)
    {
        return obj.ViewModel.Id.GetHashCode();
    }
}
