using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Media;
using Microsoft.UI.Xaml.Media.Imaging;

namespace Fluens.UI.Helpers;

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

            if (faviconUrl == UIConstants.LoadingFaviconUri)
            {
                return UIConstants.LoadingPageIcon;
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
            if (Uri.TryCreate(faviconUrl, UriKind.Absolute, out Uri? faviconUri))
            {
                return faviconUri.AbsolutePath.EndsWith(".svg", StringComparison.OrdinalIgnoreCase)
                    ? new SvgImageSource(faviconUri)
                    : new BitmapImage(faviconUri);
            }

            return UIConstants.EmptyBitmapImage;
        }
    }
}
