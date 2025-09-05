using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Media.Imaging;

namespace Fluens.UI.Helpers;

internal class UIConstants
{
    internal static readonly FontIconSource BlankPageIcon = new() { Glyph = "\uE909" };
    internal static readonly BitmapImage EmptyBitmapImage = new();
    internal const string NewTabTitle = "New Tab";
    internal const int HistoryPaginationSize = 100;
    internal static readonly FontIconSource LoadingPageIcon = new() { Glyph = "\uF16A" };
    internal const string LoadingFaviconUri = "LoadingIcon";

    public UIConstants()
    {
    }
}
