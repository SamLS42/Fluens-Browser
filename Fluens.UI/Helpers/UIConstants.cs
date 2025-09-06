using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Media.Imaging;

namespace Fluens.UI.Helpers;

internal static class UIConstants
{
    internal static readonly FontIconSource BlankPageIcon = new() { Glyph = "\uE909" };
    internal static readonly BitmapImage EmptyBitmapImage = new();
    internal static readonly FontIconSource LoadingPageIcon = new() { Glyph = "\uF16A" };
}
