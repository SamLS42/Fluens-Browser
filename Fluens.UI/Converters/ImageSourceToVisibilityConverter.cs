using Fluens.AppCore.Helpers;
using Fluens.UI.Helpers;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Data;
using Microsoft.UI.Xaml.Media;

namespace Fluens.UI.Converters;

public partial class ImageSourceToVisibilityConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, string language)
    {
        return value is ImageSource imageSource && imageSource == UIConstants.EmptyBitmapImage ? Visibility.Visible : Visibility.Collapsed;
    }

    public object ConvertBack(object value, Type targetType, object parameter, string language)
    {
        throw new NotImplementedException();
    }
}

public partial class StringToIconSourceConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, string language)
    {
        return IconSource.GetFromUrl(value as string ?? string.Empty);
    }

    public object ConvertBack(object value, Type targetType, object parameter, string language)
    {
        throw new NotImplementedException();
    }
}

public partial class StringToTabHeaderConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, string language)
    {
        return GetCorrectTitle(value as string ?? string.Empty);
    }

    public object ConvertBack(object value, Type targetType, object parameter, string language)
    {
        throw new NotImplementedException();
    }


    private static string GetCorrectTitle(string title)
    {
        return string.IsNullOrWhiteSpace(title)
                            || title.Equals(Constants.AboutBlankUri.ToString(), StringComparison.Ordinal)
                            ? Constants.NewTabTitle
                            : title;
    }
}
