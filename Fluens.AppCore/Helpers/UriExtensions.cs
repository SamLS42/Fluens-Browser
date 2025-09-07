namespace Fluens.AppCore.Helpers;

internal static class UriExtensions
{
    extension(Uri uri)
    {
        public Uri EnforceHttps()
        {
            if (uri.Scheme.Equals(Uri.UriSchemeHttps, StringComparison.Ordinal))
            {
                return uri;
            }

            UriBuilder builder = new(uri)
            {
                Scheme = Uri.UriSchemeHttps
            };

            return builder.Uri;
        }
    }
}
