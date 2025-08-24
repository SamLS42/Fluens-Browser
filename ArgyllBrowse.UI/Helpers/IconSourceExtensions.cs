using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Media;
using Microsoft.UI.Xaml.Media.Imaging;

namespace ArgyllBrowse.UI.Helpers;
internal static class IconSourceExtensions
{
    extension(IconSource src)
    {
        public static IconSource GetFromUrl(string faviconUrl)
        {
            if (string.IsNullOrWhiteSpace(faviconUrl))
            {
                return UIConstants.BlankPageIcon;
            }

            Uri faviconUri = new(faviconUrl);

            return faviconUri.AbsolutePath.EndsWith(".svg", StringComparison.OrdinalIgnoreCase)
                ? new ImageIconSource() { ImageSource = new SvgImageSource(faviconUri) }
                : new ImageIconSource() { ImageSource = new BitmapImage(faviconUri) };
        }
    }

}

internal static class ImageSourceExtensions
{
    extension(ImageSource src)
    {
        public static ImageSource GetFromUrl(string faviconUrl)
        {
            if (string.IsNullOrWhiteSpace(faviconUrl))
            {
                return UIConstants.EmptyBitmapImage;
            }

            Uri faviconUri = new(faviconUrl);

            return faviconUri.AbsolutePath.EndsWith(".svg", StringComparison.OrdinalIgnoreCase)
                ? new SvgImageSource(faviconUri)
                : new BitmapImage(faviconUri);
        }
    }
}
